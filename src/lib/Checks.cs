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

public static class Checks
{
    public static bool TryCheck(FactSet factSet, BlockTrustedOriginSet trustedOrigins, IEnumerable<Check> checks, [NotNullWhen(false)] out int? failedCheckId, [NotNullWhen(false)] out Check? failedCheck)
    {
        var result = true; 
        var checkId = 0;
        foreach(var check in checks)
        {
            result &= check.Kind switch 
            {
                Check.CheckKind.One => TryCheckOne(factSet, trustedOrigins, check.Rules),
                Check.CheckKind.All => TryCheckAll(factSet, trustedOrigins, check.Rules),
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

    public static bool TryCheckOne(FactSet factSet, BlockTrustedOriginSet trustedOrigins, IEnumerable<RuleConstrained> rules)
    {
        var ruleResult = false;

        foreach(var rule in rules)
        {
            var facts = factSet.Filter(trustedOrigins.Origins(rule.Scope));

            var eval = facts.Evaluate(rule, out var expressionResult);
            eval.UnionWith(facts);
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

    static bool TryCheckAll(FactSet factSet, BlockTrustedOriginSet trustedOrigins, IEnumerable<RuleConstrained> rules)
    {
        foreach(var rule in rules)
        {
            var facts = factSet.Filter(trustedOrigins.Origins(rule.Scope));

            var eval = facts.Evaluate(rule);
            eval.UnionWith(facts);
            
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