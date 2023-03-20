namespace biscuit_net.Datalog;

public static class Evaluator
{
    public static HashSet<Fact> Evaluate(this IEnumerable<Fact> kb, Rule rule)
    {
        var nextKb = rule.Apply(kb);
        
        if (nextKb.IsProperSupersetOf(kb))
        {
            nextKb.UnionWith(kb);
            return nextKb.Evaluate(rule);
        }
        
        return nextKb;
    }

    public static IEnumerable<Substitution> Match(this IEnumerable<Fact> body, IEnumerable<Fact> kb)
    {
        // The initial collection of bindings from which to build upon
        var seed = new[] {new Substitution()}.AsEnumerable();
            
        // Attempt to match (unify) the rule's body with the collection of Facts.
        // Returns all successful bindings.
        var matches = body.Aggregate(seed, (envs, a) => a.UnifyWith(kb, envs)).ToList();
        return matches;
    }

    static HashSet<Fact> Apply(this Rule rule, IEnumerable<Fact> kb)
    {
        bool EvalExpressions(Substitution s) => 
            rule.Constraints.All(ex =>
                    Expressions.Evaluator.Evaluate(ex.Ops, v => v.Apply(s))
            );

        return rule.Body
            .Match(kb)
            .Where(EvalExpressions)
            .Select(rule.Head.Apply)
            .ToHashSet();
    }
}