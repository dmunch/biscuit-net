using System.Diagnostics.CodeAnalysis;

namespace biscuit_net.Datalog;

public static class Verifier
{
    public static bool TryVerify(IBiscuit b, World world, [NotNullWhen(false)] out Error? err)
    {
        if(!Checks.CheckBoundVariables(b, out var invalidBlockRule))
        {
            err = new Error(invalidBlockRule);
            return false;
        }
        
        world.Atoms.UnionWith(b.Authority.Atoms);

        var authorityExecutionAtoms = Checks.EvaluateBlockRules(world, b.Authority, world.Atoms);
        if(!Checks.TryCheckBlock(world, b.Authority, authorityExecutionAtoms, 0, out err))
            return false;

        var blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Atoms.UnionWith(block.Atoms);
            var blockExecutionAtoms = Checks.EvaluateBlockRules(world, block, authorityExecutionAtoms);
            if(!Checks.TryCheckBlock(world, block, blockExecutionAtoms, blockId, out err))
                return false;

            blockId++;
        }

        err = null;
        return true;
    }
}