﻿using biscuit_net.Datalog;
using biscuit_net.Proto;
using ProtoBuf;
using System.Buffers;

namespace biscuit_net;

public class BiscuitBuilder
{
    public BlockBuilder Authority { get; private set; }
    SignatureCreator _signatureCreator;
            
    public BiscuitBuilder(SignatureCreator signatureCreator)
    {
        Authority = new BlockBuilder();
        _signatureCreator = signatureCreator;
    }

    public BiscuitBuilder AddAuthority(Fact fact) { Authority.Add(fact); return this; }
    public BiscuitBuilder AddAuthority(RuleConstrained rule) { Authority.Add(rule); return this; }
    public BiscuitBuilder AddAuthority(Check check) { Authority.Add(check); return this; }   

    public Proto.Biscuit ToProto()
    {
        var authority = new SignedBlock();

        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, Authority.ToProto());
        
        var nextKey = _signatureCreator.GetNextKey();

        authority.Block = bufferWriter.WrittenMemory.ToArray();
        authority.nextKey = new Proto.PublicKey() 
        {
            algorithm = Proto.PublicKey.Algorithm.Ed25519,
            Key = nextKey.Public
        };

        
        var buffer = SignatureHelper.MakeBuffer(authority.Block, authority.nextKey.algorithm, authority.nextKey.Key);
        authority.Signature = _signatureCreator.Sign(new ReadOnlySpan<byte>(buffer));
        
        var biscuit = new Proto.Biscuit();
        biscuit.Authority = authority;
        biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return biscuit;    
    }

    public ReadOnlySpan<byte> Serialize()
    {        
        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, ToProto());

        return bufferWriter.WrittenSpan;
    }
}

public class BlockBuilder
{
    public List<Fact> Facts { get; } = new List<Fact>();
    public List<RuleConstrained> Rules { get; } = new List<RuleConstrained>();
    public List<Check> Checks { get; } = new List<Check>();

    public BlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public BlockBuilder Add(RuleConstrained rule) { Rules.Add(rule); return this; }
    public BlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    public Proto.Block ToProto()
    {
        var symbols = new SymbolTable();
        var blockV2 = new Proto.Block();

        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.Symbols.AddRange(symbols.Symbols);
        blockV2.Version = 3;

        blockV2.Scopes.Add(new Proto.Scope() { scopeType = Proto.Scope.ScopeType.Authority });

        return blockV2;
    }
}


public static class ProtoConverters
{
    static public IEnumerable<FactV2> ToFactsV2(this IEnumerable<Fact> facts, SymbolTable symbols)
    {
        return facts.Select(fact => ToFactV2(fact, symbols)).ToList();
    }

    static public FactV2 ToFactV2(Fact fact, SymbolTable symbols)
    {       
        var factV2 = new FactV2();
        factV2.Predicate = new PredicateV2();

        factV2.Predicate.Name = symbols.LookupOrAdd(fact.Name);
        factV2.Predicate.Terms.AddRange(fact.Terms.Select(t => ToTermV2(t, symbols)));

        return factV2;
    }

    static public TermV2 ToTermV2(Term term, SymbolTable symbols)
    {
        return term switch 
        {
            (Variable v) => new TermV2() {Variable = symbols.LookupOrAdd(v.Name)},
            (Symbol s) => new TermV2() {String = symbols.LookupOrAdd(s.Name)},
            (Date d) => new TermV2() {Date = Date.ToTAI64(d.DateTime)},
            (Datalog.String s) => new TermV2() {String = symbols.LookupOrAdd(s.Value)},
            (Datalog.Boolean b) => new TermV2() {Bool = b.Value},
            (Integer i) => new TermV2() {Integer = i.Value},
            (Bytes b) => new TermV2() {Bytes = b.Value},
            (Set s) => ToTermV2(s, symbols),            
            _ => throw new NotImplementedException($"{term.GetType()}")
        };
    }

    static public TermV2 ToTermV2(Set s, SymbolTable symbols)
    {        
        var termSet = new TermSet();
        termSet.Sets.AddRange(s.Values.Select(v => ToTermV2(v, symbols)).ToList());
        return new TermV2() { Set = termSet }; 
    }
}