using VeryNaiveDatalog;

namespace biscuit_net;
using Proto;
using parser;


public static class Converters
{
    static public IEnumerable<Atom> ToAtoms(this IEnumerable<FactV2> facts, List<string> blockSymbols)
    {
        return facts.Select(fact => ToAtom(fact.Predicate, blockSymbols)).ToList();
    }

    static public Atom ToAtom(PredicateV2 predicate, List<string> blockSymbols)
    {       
        var terms = predicate.Terms.Select(t => {
            return ToAtom(t, blockSymbols);
        });
        return new Atom(Lookup(predicate.Name, blockSymbols), terms);
    }

    static public Term ToAtom(TermV2 term, List<string> blockSymbols)
    {
        return term.ContentCase switch 
        {
            TermV2.ContentOneofCase.Variable => 
                (Term)new Variable(Lookup(term.Variable, blockSymbols)),
            TermV2.ContentOneofCase.Date => 
                new Date(term.Date),
            TermV2.ContentOneofCase.String => 
                new String(Lookup(term.String, blockSymbols)),
            TermV2.ContentOneofCase.Bool => 
                new Boolean(term.Bool),
            TermV2.ContentOneofCase.Integer => 
                new Integer(term.Integer),
            TermV2.ContentOneofCase.Bytes => 
                new Bytes(term.Bytes),
            TermV2.ContentOneofCase.Set => 
                new Set(term.Set.Sets.Select(item => ToAtom(item, blockSymbols)).ToList()),
            _ => throw new NotImplementedException($"{term.ContentCase}")
        };
    }

    static public IEnumerable<RuleExpressions> ToRules(this IEnumerable<RuleV2> rules, List<string> blockSymbols)
    {
        return rules.Select(rule =>  {
            var head = ToAtom(rule.Head, blockSymbols);
            var body = rule.Bodies.Select(body => ToAtom(body, blockSymbols));
            
            return new RuleExpressions(head, body, ToParserExpr(rule.Expressions, blockSymbols));
        }).ToList();
    }

    static public IEnumerable<Check> ToChecks(this IEnumerable<CheckV2> checks, List<string> blockSymbols)
    {
        return checks.Select(check => {
                var rules = check.Queries.Select(query => {
                    var head = ToAtom(query.Head, blockSymbols);
                    var body = query.Bodies.Select(body => ToAtom(body, blockSymbols));

                    return new RuleExpressions(head, body, ToParserExpr(query.Expressions, blockSymbols));
                });
                return new Check(rules);
        });
    }

    static public IEnumerable<parser.Expression> ToParserExpr(IEnumerable<Proto.ExpressionV2> exprs, List<string> symbols)
        => exprs.Select(expr => new parser.Expression(ToParserOp(expr.Ops, symbols).ToList()));

    static public IEnumerable<parser.Op> ToParserOp(IEnumerable<Proto.Op> ops, List<string> symbols)
        => ops.Select(op => ToParserOp(op, symbols));

    static public parser.Op ToParserOp(Proto.Op op, List<string> symbols)
     => op.ContentCase switch
        {
            Proto.Op.ContentOneofCase.None => new parser.Op(),
            Proto.Op.ContentOneofCase.Value => new parser.Op(Converters.ToAtom(op.Value, symbols)),
            Proto.Op.ContentOneofCase.Binary => new parser.Op(new parser.OpBinary((parser.OpBinary.Kind)op.Binary.kind)),
            Proto.Op.ContentOneofCase.Unary =>  new parser.Op(new parser.OpUnary((parser.OpUnary.Kind)op.Unary.kind)),
            _ => throw new NotImplementedException()
        };

    public static string Lookup(ulong pos, List<string> blockSymbols)
    {
        var table = new []{
            "read",
            "write",
            "resource",
            "operation",
            "right",
            "time",
            "role",
            "owner",
            "tenant",
            "namespace",
            "user",
            "team",
            "service",
            "admin",
            "email",
            "group",
            "member",
            "ip_address",
            "client",
            "client_ip",
            "domain",
            "path",
            "version",
            "cluster",
            "node",
            "hostname",
            "nonce",
            "query"
        };

        if(pos < 1024)
            return table[pos];

        return blockSymbols[(int)pos - 1024];
    }
}