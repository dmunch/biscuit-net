using biscuit_net.Datalog;
using biscuit_net.Proto;

namespace biscuit_net.Builder;

public static class ProtoConverters
{
    static public Proto.PublicKey ToPublicKey(PublicKey publicKey)
    {
        return new Proto.PublicKey() 
        {
            algorithm = (Proto.PublicKey.Algorithm) publicKey.Algorithm,
            Key = publicKey.Key
        };
    }
    
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