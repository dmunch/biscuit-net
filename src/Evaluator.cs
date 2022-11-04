using biscuit_net.Proto;
using parser;
using VeryNaiveDatalog;

namespace biscuit_net;

/// This code is copied form VeryNaiveDatalog to allow easy adjustments
public static class Evaluator
{
    // Just a lifting of Rule.Apply to an IEnumerable<Rule>.
    public static IEnumerable<Atom> Apply(this IEnumerable<RuleExpressions> rules, IEnumerable<Atom> kb, List<string> symbols) => rules.SelectMany(r => r.Apply(kb, symbols)).ToHashSet();

    public static IEnumerable<Atom> Evaluate(this IEnumerable<Atom> kb, IEnumerable<RuleExpressions> rules, List<string> symbols)
    {
        var nextKb = rules.Apply(kb, symbols);
        if (nextKb.Except(kb).Any())
        {
            var union = kb.Union(nextKb);
            return union.Evaluate(rules, symbols);
        }

        return nextKb;
    }

    public static IEnumerable<Atom> Apply(this RuleExpressions rule, IEnumerable<Atom> kb, List<string> symbols)
    {
        // The initial collection of bindings from which to build upon
        var seed = new[] {new Substitution()}.AsEnumerable();
            
        // Attempt to match (unify) the rule's body with the collection of atoms.
        // Returns all successful bindings.
        var matches = rule.Body.Aggregate(seed, (envs, a) => a.UnifyWith(kb, envs));

        var s = new Substitution();
        foreach(var m in matches)   
        {
            foreach(var kvp in m)
            {
                s.Add(kvp.Key, kvp.Value);
            }
        }

        //if we have expressions, but not substitutions, we fail rule application
        //otherwise we evaluate the expression 
        var passed = rule.Expressions.Any() && !s.Any()
            ? false
            : rule.Expressions.All(ex => {
                    var parserOps = ex.Ops.Select(op => Converters.ToParserOp(op, symbols)).ToList();
                     return ExpressionEvaluator.Evaluate(parserOps, v => v.Apply(s));
                }                
            );
        
        if(passed)
            // Apply the bindings accumulated in the rule's body (the premises) to the rule's head (the conclusion),
            // thus obtaining the new atoms.
            return matches.Select(rule.Head.Apply);
        else
            return Enumerable.Empty<Atom>();
    }
}