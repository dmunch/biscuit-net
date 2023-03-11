using biscuit_net.Datalog;
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
    public BiscuitBuilder AddAuthority(Rule rule) { Authority.Add(rule); return this; }
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
    public List<Rule> Rules { get; } = new List<Rule>();
    public List<Check> Checks { get; } = new List<Check>();

    public BlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public BlockBuilder Add(Rule rule) { Rules.Add(rule); return this; }
    public BlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    public Proto.Block ToProto()
    {
        var symbols = new SymbolTable();
        var blockV2 = new Proto.Block();

        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.RulesV2s.AddRange(ProtoConverters.ToRulesV2(Rules, symbols));
        
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

    static public IEnumerable<RuleV2> ToRulesV2(IEnumerable<Rule> rules, SymbolTable symbols)
    {
        return rules.Select(rule => ToRuleV2(rule, symbols)).ToList();
    }

    static public RuleV2 ToRuleV2(Rule rule, SymbolTable symbols)
    {
        var ruleV2 = new RuleV2();
        ruleV2.Head = ToFactV2(rule.Head, symbols).Predicate;
        ruleV2.Bodies.AddRange(rule.Body.Select(t => ToFactV2(t, symbols).Predicate));
        ruleV2.Expressions.AddRange(rule.Constraints.Select(c => ToExpressionsV2(c, symbols)));
        
        ruleV2.Scopes.Add(new Proto.Scope() { scopeType = Proto.Scope.ScopeType.Authority });
        
        return ruleV2;
    }

    static public ExpressionV2 ToExpressionsV2(Expressions.Expression expr, SymbolTable symbols)
    {
        var exprV2 = new ExpressionV2();
        exprV2.Ops.AddRange(expr.Ops.Select(op => ToOp(op, symbols)));
        return exprV2;
    }

    static public Op ToOp(Expressions.Op op, SymbolTable symbols)
    {
        var protoOp = new Op();
        
        switch(op.Type)
        {
            case Expressions.Op.OpType.None:
                break;
            case Expressions.Op.OpType.Unary:
                protoOp.Unary = new OpUnary() { kind = (OpUnary.Kind) op.UnaryOp.OpKind };
                break;
            case Expressions.Op.OpType.Binary:
                protoOp.Binary = new OpBinary() { kind = (OpBinary.Kind) op.BinaryOp.OpKind };
                break;
            case Expressions.Op.OpType.Value:
                protoOp.Value = ToTermV2(op.Value, symbols);
                break;
        }
        
        return protoOp;
    }

}