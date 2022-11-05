using VeryNaiveDatalog;
using parser;

namespace biscuit_net;

public record World(List<Atom> Atoms, List<string> Symbols, List<Check> Checks);
public record FailedBlockCheck(int BlockId, int CheckId/*, RuleExpressions Rule*/);
public record FailedAuthorizerCheck(int CheckId/*, RuleExpressions Rule*/);

public record Error(
    FailedBlockCheck Block, 
    FailedAuthorizerCheck Authorizer, 
    InvalidBlockRule InvalidBlockRule);

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

    public bool TryAuthorize(Biscuit b, out Error err)
    {
        if(!b.CheckBoundVariables(out var invalidBlockRule))
        {
            err = new Error(null, null, invalidBlockRule);
            return false;
        }
        
        var world = new World(_authorizerAtoms.ToList(), b.Symbols, _authorizerChecks);
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

    bool TryCheckBlock(World world, Block block, IEnumerable<Atom> blockAtoms, int blockId, out Error err)
    {
        var (blockCheck, failedCheckId, failedRule) = Check(blockAtoms, block.Checks, world);
        
        if(!blockCheck) 
        {
            err = new Error(new FailedBlockCheck(blockId, failedCheckId/*, failedRule*/), null, null);
            return false;
        }

        var (blockAuthorizerCheck, failedAuthorizerCheckId, failedAuthorizerRule) = Check(blockAtoms, world.Checks, world);
        if(!blockAuthorizerCheck) 
        {
            err = new Error(null, new FailedAuthorizerCheck(failedAuthorizerCheckId/*, failedAuthorizerRule*/), null);
            return false;
        }

        err = null;
        return true;
    }

    IEnumerable<Atom> EvaluateBlockRules(World world, Block block, IEnumerable<Atom> authorityAtoms)
    {
        var rulesAtoms = world.Atoms.Evaluate(block.Rules, world.Symbols);

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
                var eval = blockAtoms.Evaluate(rule, world.Symbols, out var expressionResult);

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
}
