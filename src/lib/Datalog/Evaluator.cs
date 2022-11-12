namespace biscuit_net.Datalog;

public static class Evaluator
{
    // Just a lifting of Rule.Apply to an IEnumerable<Rule>.
    public static HashSet<Atom> Apply(this IEnumerable<RuleExpressions> rules, IEnumerable<Atom> kb)
    {
        var seed = new HashSet<Atom>();

        foreach(var r in rules)
        {
            //not interested in the expression result, but still need to evaluate it
            var atoms = r.Apply(kb, out _);
            seed.UnionWith(atoms);
        }
        return seed;
    }
    
    public static HashSet<Atom> Evaluate(this IEnumerable<Atom> kb, IEnumerable<RuleExpressions> rules)
    {
        var nextKb = rules.Apply(kb);
        
        if (nextKb.IsProperSupersetOf(kb))
        {
            nextKb.UnionWith(kb);
            return nextKb.Evaluate(rules);
        }

        return nextKb;
    }

    public static HashSet<Atom> Evaluate(this IEnumerable<Atom> kb, RuleExpressions rule, out bool expressionResult)
    {
        var nextKb = rule.Apply(kb, out expressionResult);
        
        if (nextKb.IsProperSupersetOf(kb))
        {
            nextKb.UnionWith(kb);
            return nextKb.Evaluate(rule, out expressionResult);
        }
        
        return nextKb;
    }

    public static HashSet<Atom> Evaluate(this IEnumerable<Atom> kb, RuleExpressions rule)
    {
        var nextKb = rule.Apply(kb);
    
        if (nextKb.IsProperSupersetOf(kb))
        {
            nextKb.UnionWith(kb);
            return nextKb.Evaluate(rule);
        }
        
        return nextKb;
    }

    public static IEnumerable<Substitution> Match(this IEnumerable<Atom> body, IEnumerable<Atom> kb)
    {
        // The initial collection of bindings from which to build upon
        var seed = new[] {new Substitution()}.AsEnumerable();
            
        // Attempt to match (unify) the rule's body with the collection of atoms.
        // Returns all successful bindings.
        var matches = body.Aggregate(seed, (envs, a) => a.UnifyWith(kb, envs)).ToList();
        return matches;
    }

    static HashSet<Atom> Apply(this RuleExpressions rule, IEnumerable<Atom> kb)
    {
        return rule.Body
            .Match(kb)
            .Select(rule.Head.Apply)
            .ToHashSet();
    }

    static HashSet<Atom> Apply(this RuleExpressions rule, IEnumerable<Atom> kb, out bool expressionResult)
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

        expressionResult = rule.Expressions.All(ex =>
            Expressions.Evaluator.Evaluate(ex.Ops, v => v.Apply(s))
        );

        if(expressionResult)
            // Apply the bindings accumulated in the rule's body (the premises) to the rule's head (the conclusion),
            // thus obtaining the new atoms.
            return matches.Select(rule.Head.Apply).ToHashSet();
        else
            return new HashSet<Atom>();
    }
}