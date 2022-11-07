using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

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

        var authorityExecutionAtoms = Checks.EvaluateBlockRules(world, b.Authority, world.Atoms);
        if(!Checks.TryCheckBlock(world, b.Authority, authorityExecutionAtoms, 0, out err))
            return false;

        var blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Atoms.AddRange(block.Atoms);
            var blockExecutionAtoms = Checks.EvaluateBlockRules(world, block, authorityExecutionAtoms);
            if(!Checks.TryCheckBlock(world, block, blockExecutionAtoms, blockId, out err))
                return false;

            blockId++;
        }

        err = null;
        return true;
    }

    static bool CheckBoundVariables(Biscuit b, [NotNullWhen(false)] out InvalidBlockRule? invalidBlockRule)
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
