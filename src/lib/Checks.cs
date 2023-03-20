using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public record Check(IEnumerable<RuleScoped> Rules, Check.CheckKind Kind)
{
    public Check(params RuleScoped[] rules) : this(rules.AsEnumerable(), CheckKind.One) {}

    public enum CheckKind
    {
        One,
        All
    }

    public virtual bool Equals(Check? other) => Kind == other?.Kind && Rules.SequenceEqual(other.Rules);
    public override int GetHashCode() => HashCode.Combine(Kind, Rules.Aggregate(0, HashCode.Combine));
}

public static class Checks
{
    public static bool TryCheck(IEnumerable<Check> checks, Func<RuleScoped, IEnumerable<Fact>> factProvider, [NotNullWhen(false)] out int? failedCheckId, [NotNullWhen(false)] out Check? failedCheck)
    {
        var result = true; 
        var checkId = 0;
        foreach(var check in checks)
        {
            result &= check.Kind switch 
            {
                Check.CheckKind.One => TryCheckOne(check.Rules, factProvider),
                Check.CheckKind.All => TryCheckAll(check.Rules, factProvider),
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

    public static bool TryCheckOne(IEnumerable<RuleScoped> rules, Func<RuleScoped, IEnumerable<Fact>> factProvider)
    {
        var ruleResult = false;

        foreach(var rule in rules)
        {
            var facts = factProvider(rule);

            var eval = facts.Evaluate(rule);
            ruleResult |= eval.Any();

            eval.UnionWith(facts);
        }

        return ruleResult;   
    }

    static bool TryCheckAll(IEnumerable<RuleScoped> rules, Func<RuleScoped, IEnumerable<Fact>> factProvider)
    {
        foreach(var rule in rules)
        {
            var facts = factProvider(rule);

            //for check all, we want to evaluate the expressions once we found all matches
            //so we evaluate the body without taking into account the expressions first
            //and run the expressions only later ontop of the matches
            var ruleWithoutExpressions = rule with {
                Constraints = Enumerable.Empty<Expressions.Expression>()
            };
            var eval = facts.Evaluate(ruleWithoutExpressions);
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

    public static bool TryCheckBoundVariables(IEnumerable<Rule> rules, [NotNullWhen(false)] out int? invalidRuleId)
    {
        int ruleId = 0;
        foreach(var rule in rules)
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

    public static bool CheckBoundVariables(Block authority, IEnumerable<Block> blocks, [NotNullWhen(false)] out InvalidBlockRule? invalidBlockRule)
    {
        if(!Checks.TryCheckBoundVariables(authority.Rules, out var invalidRuleId))
        {
            invalidBlockRule = new InvalidBlockRule(invalidRuleId.Value);
            return false;
        }

        foreach(var block in blocks)
        {
            if(!Checks.TryCheckBoundVariables(block.Rules, out var invalidBlockRuleId))
            {
                invalidBlockRule = new InvalidBlockRule(invalidBlockRuleId.Value);
                return false;
            }
        }

        invalidBlockRule = null;
        return true;
    }
}