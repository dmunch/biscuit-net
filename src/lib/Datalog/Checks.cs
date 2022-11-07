using System.Diagnostics.CodeAnalysis;

namespace biscuit_net.Datalog;

public record Check(IEnumerable<RuleExpressions> Rules);
public record World(List<Atom> Atoms, List<Check> Checks);

public static class Checks
{
    public static bool TryCheckBlock(World world, Block block, IEnumerable<Atom> blockAtoms, int blockId, [NotNullWhen(false)] out Error? err)
    {
        var (blockCheck, failedCheckId, failedRule) = Check(blockAtoms, block.Checks, world);
        
        if(!blockCheck) 
        {
            err = new Error(new FailedBlockCheck(blockId, failedCheckId/*, failedRule*/));
            return false;
        }

        var (blockAuthorizerCheck, failedAuthorizerCheckId, failedAuthorizerRule) = Check(blockAtoms, world.Checks, world);
        if(!blockAuthorizerCheck) 
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId/*, failedAuthorizerRule*/));
            return false;
        }

        err = null;
        return true;
    }

    public static IEnumerable<Atom> EvaluateBlockRules(World world, Block block, IEnumerable<Atom> authorityAtoms)
    {
        var rulesAtoms = world.Atoms.Evaluate(block.Rules);

        var blockScopedAtoms = authorityAtoms.ToList();
        blockScopedAtoms.AddRange(rulesAtoms);

        return blockScopedAtoms;
    }

    static (bool, int, Check?) Check(IEnumerable<Atom> blockAtoms, IEnumerable<Check> checks, World world)
    {
        var result = true; 
        var checkId = 0;
        foreach(var check in checks)
        {
            var ruleResult = false; 
            foreach(var rule in check.Rules)
            {
                var eval = blockAtoms.Evaluate(rule, out var expressionResult);

                var checkScopedAtoms = blockAtoms.ToList();
                checkScopedAtoms.AddRange(eval);
                var subs = rule.Head.UnifyWith(checkScopedAtoms, new Substitution());

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
                return (false, checkId, check);
            }
            checkId++;
        }
        return (true, -1, null);
    }

    public static bool CheckBoundVariables(Block block, [NotNullWhen(false)] out int? invalidRuleId)
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
}