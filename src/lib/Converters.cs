namespace biscuit_net;
using Proto;
using Datalog;

public static class Converters
{
    static public IEnumerable<Fact> ToFacts(this IEnumerable<FactV2> facts, SymbolTable symbols)
    {
        return facts.Select(fact => ToFact(fact.Predicate, symbols)).ToList();
    }

    static public Fact ToFact(PredicateV2 predicate, SymbolTable symbols)
    {       
        var terms = predicate.Terms.Select(t => {
            return ToFact(t, symbols);
        });
        return new Fact(symbols.Lookup(predicate.Name), terms);
    }

    static public Term ToFact(TermV2 term, SymbolTable symbols)
    {
        return term.ContentCase switch 
        {
            TermV2.ContentOneofCase.Variable => 
                (Term)new Variable(symbols.Lookup(term.Variable)),
            TermV2.ContentOneofCase.Date => 
                new Date(term.Date),
            TermV2.ContentOneofCase.String => 
                new String(symbols.Lookup(term.String)),
            TermV2.ContentOneofCase.Bool => 
                new Boolean(term.Bool),
            TermV2.ContentOneofCase.Integer => 
                new Integer(term.Integer),
            TermV2.ContentOneofCase.Bytes => 
                new Bytes(term.Bytes),
            TermV2.ContentOneofCase.Set => 
                new Set(term.Set.Sets.Select(item => ToFact(item, symbols)).ToList()),
            _ => throw new NotImplementedException($"{term.ContentCase}")
        };
    }

    static public IEnumerable<RuleConstrained> ToRules(this IEnumerable<RuleV2> rules, SymbolTable symbols, KeyTable keys)
    {
        return rules.Select(rule =>  {
            var head = ToFact(rule.Head, symbols);
            var body = rule.Bodies.Select(body => ToFact(body, symbols));

            return new RuleConstrained(head, body, ToParserExpr(rule.Expressions, symbols), ToScope(rule.Scopes, keys));
        }).ToList();
    }

    static public IEnumerable<Check> ToChecks(this IEnumerable<CheckV2> checks, SymbolTable symbols, KeyTable keys)
    {
        return checks.Select(check => {
                var rules = check.Queries.Select(query => {
                    var head = ToFact(query.Head, symbols);
                    var body = query.Bodies.Select(body => ToFact(body, symbols));

                    

                    return new RuleConstrained(head, body, ToParserExpr(query.Expressions, symbols), ToScope(query.Scopes, keys));
                });
                
                var kind = check.kind switch 
                {
                    CheckV2.Kind.One => Check.CheckKind.One,
                    CheckV2.Kind.All => Check.CheckKind.All,
                    _ => throw new NotSupportedException($"Check kind {check.kind} not supported")
                };
                return new Check(rules, kind);
        });
    }

    static public Datalog.Scope ToScope(IEnumerable<Proto.Scope> scopes, KeyTable keys)
    {
        return new Datalog.Scope(
                ToScopeTypes(scopes),
                ToTrustedPublicKeys(scopes, keys)
        );
    }

    static public IEnumerable<PublicKey> ToTrustedPublicKeys(IEnumerable<Proto.Scope> scopes, KeyTable keys)
    {
        return scopes
            .Where(scope => scope.ContentCase == Proto.Scope.ContentOneofCase.publicKey)
            .Select(scope => keys.Lookup(scope.publicKey))
            .ToList();
    }

    static public IEnumerable<ScopeType> ToScopeTypes(IEnumerable<Proto.Scope> scopes)
    {
        return scopes
            .Where(scope => scope.ContentCase == Proto.Scope.ContentOneofCase.scopeType)
            .Select(scope => (ScopeType) scope.scopeType)
            .ToList();
    }

    static public PublicKey ToPublicKey(Proto.PublicKey publicKey)
    {
        return new PublicKey((Algorithm)publicKey.algorithm, publicKey.Key);
    }

    static public IEnumerable<Expressions.Expression> ToParserExpr(IEnumerable<Proto.ExpressionV2> exprs, SymbolTable symbols)
        => exprs.Select(expr => new Expressions.Expression(ToParserOp(expr.Ops, symbols).ToList()));

    static public IEnumerable<Expressions.Op> ToParserOp(IEnumerable<Proto.Op> ops, SymbolTable symbols)
        => ops.Select(op => ToParserOp(op, symbols));

    static public Expressions.Op ToParserOp(Proto.Op op, SymbolTable symbols)
     => op.ContentCase switch
        {
            Proto.Op.ContentOneofCase.None => new Expressions.Op(),
            Proto.Op.ContentOneofCase.Value => new Expressions.Op(Converters.ToFact(op.Value, symbols)),
            Proto.Op.ContentOneofCase.Binary => new Expressions.Op(new Expressions.OpBinary((Expressions.OpBinary.Kind)op.Binary.kind)),
            Proto.Op.ContentOneofCase.Unary =>  new Expressions.Op(new Expressions.OpUnary((Expressions.OpUnary.Kind)op.Unary.kind)),
            _ => throw new NotImplementedException()
        };
}