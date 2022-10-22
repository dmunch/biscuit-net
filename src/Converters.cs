using VeryNaiveDatalog;

namespace biscuit_net;
using Proto;

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
        if(term.ShouldSerializeVariable())
            return (Term)new Variable(Lookup(term.Variable, blockSymbols));
        if(term.ShouldSerializeDate())
            return new Date(term.Date);
        if(term.ShouldSerializeString())
            return new Symbol(Lookup(term.String, blockSymbols));
        
        /*
        if(t.ShouldSerializeBool())
            return new Symbol(t.Bool.ToString());
        if(t.ShouldSerializeBytes())
            return new Symbol(t.Bytes.ToString());
        
        if(t.ShouldSerializeInteger())
            return new Symbol(t.Integer.ToString());
        if(t.ShouldSerializeSet())
            return new Symbol(t.Set.ToString());
        */
        throw new Exception();
    }

    static public IEnumerable<Rule> ToRules(this IEnumerable<RuleV2> rules, List<string> blockSymbols)
    {
        return rules.Select(rule =>  {
            var head = ToAtom(rule.Head, blockSymbols);
            var body = rule.Bodies.Select(body => ToAtom(body, blockSymbols));
            
            return new Rule(head, body);
        }).ToList();
    }

    static public IEnumerable<Rule> ToQueries(this IEnumerable<CheckV2> checks, List<string> blockSymbols)
    {
        return checks.SelectMany(check => {
            return check.Queries.Select(query => {
                var head = ToAtom(query.Head, blockSymbols);
                var body = query.Bodies.Select(body => ToAtom(body, blockSymbols));

                return new Rule(head, body);
            });
        });
    }

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