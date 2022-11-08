using System.Diagnostics.CodeAnalysis;

namespace biscuit_net.Datalog;

public record Check(IEnumerable<RuleExpressions> Rules)
{
    public Check(params RuleExpressions[] rules) : this(rules.AsEnumerable()) {}
}

public record World(HashSet<Atom> Atoms, List<Check> Checks);

public static class Checks
{
    public static bool TryCheckBlock(World world, IBlock block, HashSet<Atom> blockAtoms, int blockId, [NotNullWhen(false)] out Error? err)
    {
        if(!TryCheck(world, blockAtoms, block.Checks, out var failedCheckId, out var failedCheck))
        {
            err = new Error(new FailedBlockCheck(blockId, failedCheckId.Value/*, failedRule*/));
            return false;
        }

        if(!TryCheck(world, blockAtoms, world.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }

        err = null;
        return true;
    }

    public static HashSet<Atom> EvaluateBlockRules(World world, IBlock block, HashSet<Atom> authorityAtoms)
    {
        var rulesAtoms = world.Atoms.Evaluate(block.Rules);
        rulesAtoms.UnionWith(authorityAtoms);

        return rulesAtoms;
    }

    static bool TryCheck(World world, HashSet<Atom> blockAtoms, IEnumerable<Check> checks, [NotNullWhen(false)] out int? failedCheckId, [NotNullWhen(false)] out Check? failedCheck)
    {
        var result = true; 
        var checkId = 0;
        foreach(var check in checks)
        {
            var ruleResult = false; 
            foreach(var rule in check.Rules)
            {
                var eval = blockAtoms.Evaluate(rule, out var expressionResult);
                eval.UnionWith(blockAtoms);
                var subs = rule.Head.UnifyWith(eval, new Substitution());

                if(rule.Body.Any())
                {
                    ruleResult |= subs.Any();
                }
                if(rule.Expressions.Any())
                {
                    ruleResult |= expressionResult;
                }
            }

            result &= ruleResult;
            if(!result) 
            {
                //check failed? we return false, alongside
                //the check index and the actual rule used for the check
                failedCheckId = checkId;
                failedCheck = check;
                return false;
            }
            checkId++;
        }

        failedCheckId = null;
        failedCheck = null;
        return true;
    }

    public static bool TryCheckBoundVariables(IBlock block, [NotNullWhen(false)] out int? invalidRuleId)
    {
        int ruleId = 0;
        foreach(var rule in block.Rules)
        {
            var headVariables = rule.Head.Terms.OfType<Variable>();
            var bodyVariables = rule.Body.SelectMany(b => b.Terms).OfType<Variable>().ToHashSet();
            
            if(!headVariables.All(hv => bodyVariables.Contains(hv)))
            {
                invalidRuleId = ruleId;
                return false;
            }
            ruleId++;
        }

        invalidRuleId = null;
        return true;
    }

    public static bool CheckBoundVariables(IBiscuit b, [NotNullWhen(false)] out InvalidBlockRule? invalidBlockRule)
    {
        if(!Checks.TryCheckBoundVariables(b.Authority, out var invalidRuleId))
        {
            invalidBlockRule = new InvalidBlockRule(invalidRuleId.Value);
            return false;
        }

        foreach(var block in b.Blocks)
        {
            if(!Checks.TryCheckBoundVariables(block, out var invalidBlockRuleId))
            {
                invalidBlockRule = new InvalidBlockRule(invalidBlockRuleId.Value);
                return false;
            }
        }

        invalidBlockRule = null;
        return true;
    }
}