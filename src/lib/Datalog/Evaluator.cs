namespace biscuit_net.Datalog;

public static class Evaluator
{
    /*
    // Just a lifting of Rule.Apply to an IEnumerable<Rule>.
    public static HashSet<Fact> Apply(this IEnumerable<RuleConstrained> rules, FactSet facts, BlockTrustedOriginSet trustedOrigins)
    {
        var seed = new HashSet<Fact>();

        foreach(var r in rules)
        {
            var kb = facts.Filter(trustedOrigins.Origins(r.Scope));

            //not interested in the expression result, but still need to evaluate it
            var nextKb = r.Apply(kb, out _);
            seed.UnionWith(nextKb);
        }
        return seed;
    }

    public static HashSet<Fact> Evaluate(this FactSet facts, IEnumerable<RuleConstrained> rules, BlockTrustedOriginSet trustedOrigins)
    {
        var nextKb = rules.Apply(facts, trustedOrigins);

        var kb = facts.Filter(trustedOrigins.Origins());
        
        if (nextKb.IsProperSupersetOf(kb))
        {
            facts.UnionWith(trustedOrigins.BlockId, nextKb);
            
            return facts.Evaluate(rules, trustedOrigins);
        }

        facts.UnionWith(trustedOrigins.BlockId, nextKb);
        return nextKb;
    }
    */

    public static HashSet<Fact> Apply(this IEnumerable<RuleConstrained> rules, IEnumerable<Fact> blockFacts, FactSet facts, BlockTrustedOriginSet trustedOrigins)
    {
        var seed = new HashSet<Fact>();

        foreach(var r in rules)
        {
            var kb = r.Scope.IsEmpty ? blockFacts : facts.Filter(trustedOrigins.Origins(r.Scope));

            //not interested in the expression result, but still need to evaluate it
            var nextKb = r.Apply(kb, out _);
            seed.UnionWith(nextKb);
        }
        return seed;
    }

    public static HashSet<Fact> Evaluate(this IEnumerable<Fact> blockFacts, IEnumerable<RuleConstrained> rules, FactSet facts, BlockTrustedOriginSet trustedOrigins)
    {
        var nextKb = rules.Apply(blockFacts, facts, trustedOrigins);

        var kb = facts.Filter(trustedOrigins.Origins());
        
        if (nextKb.IsProperSupersetOf(blockFacts))
        {
            nextKb.UnionWith(blockFacts);
            
            return blockFacts.Evaluate(rules, facts, trustedOrigins);
        }

        return nextKb;
    }

    public static HashSet<Fact> Evaluate(this IEnumerable<Fact> kb, RuleConstrained rule, out bool expressionResult)
    {
        var nextKb = rule.Apply(kb, out expressionResult);
        
        if (nextKb.IsProperSupersetOf(kb))
        {
            nextKb.UnionWith(kb);
            return nextKb.Evaluate(rule, out expressionResult);
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
        return rule.Body
            .Match(kb)
            .Select(rule.Head.Apply)
            .ToHashSet();
    }

    static HashSet<Fact> Apply(this RuleConstrained rule, IEnumerable<Fact> kb, out bool expressionResult)
    {
        var matches = rule.Body.Match(kb);

        var s = new Substitution();
        foreach(var m in matches)
        {
            foreach(var kvp in m)
            {
                s.Add(kvp.Key, kvp.Value);
            }
        }

        expressionResult = rule.Constraints.All(ex =>
            Expressions.Evaluator.Evaluate(ex.Ops, v => v.Apply(s))
        );

        if(expressionResult)
            // Apply the bindings accumulated in the rule's body (the premises) to the rule's head (the conclusion),
            // thus obtaining the new Facts.
            return matches.Select(rule.Head.Apply).ToHashSet();
        else
            return new HashSet<Fact>();
    }
}