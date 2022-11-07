using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public record World(List<Atom> Atoms, List<Check> Checks);
public record FailedBlockCheck(int BlockId, int CheckId/*, RuleExpressions Rule*/);
public record FailedAuthorizerCheck(int CheckId/*, RuleExpressions Rule*/);

//TODO Assuming the int is a RuleId - specification and examples are unclear here
public record InvalidBlockRule(int RuleId/*, RuleExpressions Rule*/);

public record Error
{
    public Error(FailedBlockCheck block) => Block = block;
    public Error(FailedAuthorizerCheck authorizer) => Authorizer = authorizer;
    public Error(InvalidBlockRule invalidBlockRule) => InvalidBlockRule = invalidBlockRule;

    FailedBlockCheck? Block { get; } = null;
    FailedAuthorizerCheck? Authorizer { get; } = null;
    InvalidBlockRule? InvalidBlockRule { get; } = null;
}

public class Authorizer
{
    List<Atom> _authorizerAtoms = new List<Atom>();
    List<Check> _authorizerChecks = new List<Check>();
    
    public void AddAtom(Atom atom)
    {
        _authorizerAtoms.Add(atom);
    }

    public void AddCheck(Check check)
    {
        _authorizerChecks.Add(check);
    }

    public bool TryAuthorize(Biscuit b, [NotNullWhen(false)] out Error? err)
    {
        if(!CheckBoundVariables(b, out var invalidBlockRule))
        {
            err = new Error(invalidBlockRule);
            return false;
        }
        
        var world = new World(_authorizerAtoms.ToList(), _authorizerChecks);
        world.Atoms.AddRange(b.Authority.Atoms);

        var authorityExecutionAtoms = EvaluateBlockRules(world, b.Authority, world.Atoms);
        if(!TryCheckBlock(world, b.Authority, authorityExecutionAtoms, 0, out err))
            return false;

        var blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Atoms.AddRange(block.Atoms);
            var blockExecutionAtoms = EvaluateBlockRules(world, block, authorityExecutionAtoms);
            if(!TryCheckBlock(world, block, blockExecutionAtoms, blockId, out err))
                return false;

            blockId++;
        }

        err = null;
        return true;
    }

    bool TryCheckBlock(World world, Block block, IEnumerable<Atom> blockAtoms, int blockId, [NotNullWhen(false)] out Error? err)
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

    IEnumerable<Atom> EvaluateBlockRules(World world, Block block, IEnumerable<Atom> authorityAtoms)
    {
        var rulesAtoms = world.Atoms.Evaluate(block.Rules);

        var blockScopedAtoms = authorityAtoms.ToList();
        blockScopedAtoms.AddRange(rulesAtoms);

        return blockScopedAtoms;
    }

    (bool, int, Check?) Check(IEnumerable<Atom> blockAtoms, IEnumerable<Check> checks, World world)
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

    static bool CheckBoundVariables(Biscuit b, [NotNullWhen(false)] out InvalidBlockRule? invalidBlockRule)
    {
        if(!CheckBoundVariables(b.Authority, out invalidBlockRule))
        {
            return false;
        }

        foreach(var block in b.Blocks)
        {
            if(!CheckBoundVariables(block, out invalidBlockRule))
            {
                return false;
            }
        }

        invalidBlockRule = null;
        return true;
    }

    static bool CheckBoundVariables(Block block, [NotNullWhen(false)] out InvalidBlockRule? invalidBlockRule)
    {
        int ruleId = 0;
        foreach(var rule in block.Rules)
        {
            var headVariables = rule.Head.Terms.OfType<Variable>();
            var bodyVariables = rule.Body.SelectMany(b => b.Terms).OfType<Variable>().ToHashSet();
            
            if(!headVariables.All(hv => bodyVariables.Contains(hv)))
            {
                invalidBlockRule = new InvalidBlockRule(ruleId);
                return false;
            }
            ruleId++;
        }

        invalidBlockRule = null;
        return true;
    }
}
