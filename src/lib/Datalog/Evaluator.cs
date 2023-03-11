namespace biscuit_net.Datalog;

public static class Evaluator
{
    public static HashSet<Fact> Apply(this IEnumerable<Rule> rules, IEnumerable<Fact> blockFacts, Func<Scope, IEnumerable<Fact>> additionalFacts)
    {
        var seed = new HashSet<Fact>();

        foreach(var r in rules)
        {
            var kb = r.Scope.IsEmpty ? blockFacts : additionalFacts(r.Scope);

            var nextKb = r.Apply(kb);
            seed.UnionWith(nextKb);
        }
        return seed;
    }

    public static HashSet<Fact> Evaluate(this IEnumerable<Fact> blockFacts, IEnumerable<Rule> rules, Func<Scope, IEnumerable<Fact>> additionalFacts)
    {
        var nextKb = rules.Apply(blockFacts, additionalFacts);

        if (nextKb.IsProperSupersetOf(blockFacts))
        {
            nextKb.UnionWith(blockFacts);
            return blockFacts.Evaluate(rules, additionalFacts);
        }

        return nextKb;
    }

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