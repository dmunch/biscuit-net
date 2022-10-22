using VeryNaiveDatalog;

namespace biscuit_net;

/// This code is copied form VeryNaiveDatalog to allow easy adjustments
public static class Evaluator
{
    // Just a lifting of Rule.Apply to an IEnumerable<Rule>.
    public static IEnumerable<Atom> ApplyWithExpressions(this IEnumerable<Rule> rules, IEnumerable<Atom> kb) => rules.SelectMany(r => r.ApplyWithExpressions(kb)).ToHashSet();

    public static IEnumerable<Atom> EvaluateWithExpressions(this IEnumerable<Atom> kb, IEnumerable<Rule> rules)
    {
        var nextKb = rules.ApplyWithExpressions(kb);
        if (nextKb.Except(kb).Any())
        {
            var union = kb.Union(nextKb);
            return union.EvaluateWithExpressions(rules);
        }

        return nextKb;
    }

    public static IEnumerable<Atom> ApplyWithExpressions(this Rule rule, IEnumerable<Atom> kb)
    {
        // The initial collection of bindings from which to build upon
        var seed = new[] {new Substitution()}.AsEnumerable();
            
        // Attempt to match (unify) the rule's body with the collection of atoms.
        // Returns all successful bindings.
        var matches = rule.Body.Aggregate(seed, (envs, a) => a.UnifyWith(kb, envs));
            
        
        // Apply the bindings accumulated in the rule's body (the premises) to the rule's head (the conclusion),
        // thus obtaining the new atoms.
        return matches.Select(rule.Head.Apply);
    }

    public static IEnumerable<Substitution> Query(this IEnumerable<Atom> kb, Atom q, IEnumerable<Rule> rules) =>
        q.UnifyWith(kb.Evaluate(rules), new Substitution());
}