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
        
        if(b.Authority.Version < 3 || b.Authority.Version > 4)
            throw new Exception($"Unsupported Authority Block Version {b.Authority.Version}");

        foreach(var block in b.Blocks)
        {
            if(block.Version < 3 || block.Version > 4)
                throw new Exception($"Unsupported Block Version {b.Authority.Version}");
        }

        world.Atoms.Add(Origin.Authority, b.Authority.Atoms.ToHashSet());
        
        var authorityTrustedOrigin = new TrustedOrigin(Origin.Authority, Origin.Authorizer);
        var authorityExecutionAtoms = world.Atoms.Filter(authorityTrustedOrigin).Evaluate(b.Authority.Rules);
        world.Atoms.Merge(Origin.Authority, authorityExecutionAtoms);
        
        if(!Checks.TryCheckBlock(world, b.Authority, world.Atoms.Filter(authorityTrustedOrigin), 0, out err))
            return false;

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Atoms.Add(new Origin(blockId), block.Atoms.ToHashSet());

            var blockTrustedOrigin = new TrustedOrigin(Origin.Authority, (Origin)blockId, Origin.Authorizer);
            var blockExecutionAtoms = world.Atoms.Filter(blockTrustedOrigin).Evaluate(block.Rules);
            world.Atoms.Merge(new Origin(blockId), blockExecutionAtoms);

            if(!Checks.TryCheckBlock(world, block, world.Atoms.Filter(blockTrustedOrigin).Evaluate(block.Rules), blockId, out err))
                return false;

            blockId++;
        }

        err = null;
        return true;
    }
}