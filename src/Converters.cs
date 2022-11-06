using VeryNaiveDatalog;

namespace biscuit_net;
using Proto;
using parser;


public static class Converters
{
    static public IEnumerable<Atom> ToAtoms(this IEnumerable<FactV2> facts, SymbolTable symbols)
    {
        return facts.Select(fact => ToAtom(fact.Predicate, symbols)).ToList();
    }

    static public Atom ToAtom(PredicateV2 predicate, SymbolTable symbols)
    {       
        var terms = predicate.Terms.Select(t => {
            return ToAtom(t, symbols);
        });
        return new Atom(symbols.Lookup(predicate.Name), terms);
    }

    static public Term ToAtom(TermV2 term, SymbolTable symbols)
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
                new Set(term.Set.Sets.Select(item => ToAtom(item, symbols)).ToList()),
            _ => throw new NotImplementedException($"{term.ContentCase}")
        };
    }

    static public IEnumerable<RuleExpressions> ToRules(this IEnumerable<RuleV2> rules, SymbolTable symbols)
    {
        return rules.Select(rule =>  {
            var head = ToAtom(rule.Head, symbols);
            var body = rule.Bodies.Select(body => ToAtom(body, symbols));
            
            return new RuleExpressions(head, body, ToParserExpr(rule.Expressions, symbols));
        }).ToList();
    }

    static public IEnumerable<Check> ToChecks(this IEnumerable<CheckV2> checks, SymbolTable symbols)
    {
        return checks.Select(check => {
                var rules = check.Queries.Select(query => {
                    var head = ToAtom(query.Head, symbols);
                    var body = query.Bodies.Select(body => ToAtom(body, symbols));

                    return new RuleExpressions(head, body, ToParserExpr(query.Expressions, symbols));
                });
                return new Check(rules);
        });
    }

    static public IEnumerable<parser.Expression> ToParserExpr(IEnumerable<Proto.ExpressionV2> exprs, SymbolTable symbols)
        => exprs.Select(expr => new parser.Expression(ToParserOp(expr.Ops, symbols).ToList()));

    static public IEnumerable<parser.Op> ToParserOp(IEnumerable<Proto.Op> ops, SymbolTable symbols)
        => ops.Select(op => ToParserOp(op, symbols));

    static public parser.Op ToParserOp(Proto.Op op, SymbolTable symbols)
     => op.ContentCase switch
        {
            Proto.Op.ContentOneofCase.None => new parser.Op(),
            Proto.Op.ContentOneofCase.Value => new parser.Op(Converters.ToAtom(op.Value, symbols)),
            Proto.Op.ContentOneofCase.Binary => new parser.Op(new parser.OpBinary((parser.OpBinary.Kind)op.Binary.kind)),
            Proto.Op.ContentOneofCase.Unary =>  new parser.Op(new parser.OpUnary((parser.OpUnary.Kind)op.Unary.kind)),
            _ => throw new NotImplementedException()
        };
}