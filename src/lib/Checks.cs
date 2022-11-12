using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public record Check(IEnumerable<RuleConstrained> Rules, Check.CheckKind Kind)
{
    public Check(params RuleConstrained[] rules) : this(rules.AsEnumerable(), CheckKind.One) {}

    public enum CheckKind
    {
        One,
        All
    }
}

public record World(FactSet Facts, RuleSet Rules, List<Check> Checks);

public static class Checks
{
    public static bool TryCheckBlock(World world, IBlock block, IEnumerable<Fact> blockFacts, uint blockId, [NotNullWhen(false)] out Error? err)
    {
        if(!TryCheck(blockFacts, block.Checks, out var failedCheckId, out var failedCheck))
        {
            err = new Error(new FailedBlockCheck(blockId, failedCheckId.Value/*, failedRule*/));
            return false;
        }

        if(!TryCheck(blockFacts, world.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }

        err = null;
        return true;
    }

    static bool TryCheck(IEnumerable<Fact> blockFacts, IEnumerable<Check> checks, [NotNullWhen(false)] out int? failedCheckId, [NotNullWhen(false)] out Check? failedCheck)
    {
        var result = true; 
        var checkId = 0;
        foreach(var check in checks)
        {
            result &= check.Kind switch 
            {
                Check.CheckKind.One => TryCheckOne(blockFacts, check.Rules),
                Check.CheckKind.All => TryCheckAll(blockFacts, check.Rules),
                _ => throw new NotSupportedException()
            };
            
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

    static bool TryCheckOne(IEnumerable<Fact> blockFacts, IEnumerable<IRuleConstrained> rules)
    {
        var ruleResult = false;

        foreach(var rule in rules)
        {
            var eval = blockFacts.Evaluate(rule, out var expressionResult);
            eval.UnionWith(blockFacts);
            var subs = rule.Head.UnifyWith(eval, new Substitution());

            if(rule.Body.Any())
            {
                ruleResult |= subs.Any();
            }
            if(rule.Constraints.Any())
            {
                ruleResult |= expressionResult;
            }
        }

        return ruleResult;   
    }

    static bool TryCheckAll(IEnumerable<Fact> blockFacts, IEnumerable<IRuleConstrained> rules)
    {
        foreach(var rule in rules)
        {
            var eval = blockFacts.Evaluate(rule);
            eval.UnionWith(blockFacts);
            
            var matches = rule.Body.Match(eval);

            var result = matches.All(match => 
                rule.Constraints.All(ex => 
                    Expressions.Evaluator.Evaluate(ex.Ops, v => v.Apply(match))
                )
            );
            
            if(!result) 
            {
                return false;
            }
        }

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