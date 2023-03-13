using biscuit_net.Datalog;
using biscuit_net.Proto;
using ProtoBuf;
using System.Buffers;

namespace biscuit_net;

public interface IBiscuitBuilder
{
    BlockBuilder AddBlock();
    Proto.Biscuit ToProto();    
}

public static class BiscuitBuilderExtensions
{
    public static ReadOnlySpan<byte> Serialize(this IBiscuitBuilder builder)
    {        
        return builder.ToProto().Serialize();        
    }

    public static ReadOnlySpan<byte> Serialize(this Proto.Biscuit proto)
    {        
        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, proto);

        return bufferWriter.WrittenSpan;
    }

    public static ReadOnlySpan<byte> Seal(this IBiscuitBuilder builder) 
    {
        var biscuit = builder.ToProto();
        var signer = new SignatureCreator(biscuit.Proof.nextSecret);

        var finalSignature = signer.Sign(SignatureHelper.MakeFinalSignatureBuffer(biscuit));
        biscuit.Proof = new Proto.Proof() { finalSignature = finalSignature };
        return biscuit.Serialize();
    }
}

public class BiscuitBuilder : IBiscuitBuilder
{
    BlockBuilder _authority;
    List<BlockBuilder> _blocks = new List<BlockBuilder>();

    SignatureCreator _signatureCreator;
            
    public BiscuitBuilder(SignatureCreator signatureCreator)
    {
        _authority = new BlockBuilder(this);
        _signatureCreator = signatureCreator;
    }

    public BlockBuilder AuthorityBlock() => _authority;
    
    public BlockBuilder AddBlock()
    {
        var block = new BlockBuilder(this);
        _blocks.Add(block);
        return block;
    }

    public Proto.Biscuit ToProto()
    {
        var nextKey = _signatureCreator.GetNextKey();
        var symbols = new SymbolTable();

        var biscuit = new Proto.Biscuit() 
        {
            Authority = SignBlock(_authority.ToProto(symbols), nextKey, _signatureCreator)
        };
        
        foreach(var block in _blocks)
        {
            var nextSigner = new SignatureCreator(nextKey);

            nextKey = _signatureCreator.GetNextKey();            
            biscuit.Blocks.Add(SignBlock(block.ToProto(symbols), nextKey, nextSigner));
        }

        biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return biscuit;    
    }

    public static Proto.SignedBlock SignBlock(Proto.Block block, SignatureCreator.NextKey nextKey, SignatureCreator signer)
    {
        var signedBlock = new SignedBlock();

        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, block);
        
        signedBlock.Block = bufferWriter.WrittenMemory.ToArray();
        signedBlock.nextKey = new Proto.PublicKey() 
        {
            algorithm = Proto.PublicKey.Algorithm.Ed25519,
            Key = nextKey.Public
        };
        
        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.nextKey.algorithm, signedBlock.nextKey.Key);
        signedBlock.Signature = signer.Sign(new ReadOnlySpan<byte>(buffer));
        
        return signedBlock;    
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

    static public IEnumerable<CheckV2> ToChecksV2(IEnumerable<Check> checks, SymbolTable symbols)
    {
        return checks.Select(check => ToCheckV2(check, symbols)).ToList();
    }

    static public CheckV2 ToCheckV2(Check check, SymbolTable symbols)
    {
        var checkV2 = new CheckV2();

        checkV2.kind = (CheckV2.Kind) check.Kind;
        checkV2.Queries.AddRange(ToRulesV2(check.Rules, symbols));
        
        return checkV2;
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