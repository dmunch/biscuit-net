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

        world.Facts.Add(Origin.Authority, b.Authority.Facts.ToHashSet());

        var authorityTrustedOrigin = new TrustedOrigin(Origin.Authority, Origin.Authorizer);
        var authorityExecutionFacts = world.Facts.Filter(authorityTrustedOrigin).Evaluate(b.Authority.Rules);
        world.Facts.Merge(Origin.Authority, authorityExecutionFacts);
        
        if(!Checks.TryCheckBlock(world, b.Authority, world.Facts.Filter(authorityTrustedOrigin), 0, out err))
            return false;

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Facts.Add(new Origin(blockId), block.Facts.ToHashSet());

            var blockTrustedOrigin = new TrustedOrigin(Origin.Authority, (Origin)blockId, Origin.Authorizer);
            var blockExecutionFacts = world.Facts.Filter(blockTrustedOrigin).Evaluate(block.Rules);
            world.Facts.Merge(new Origin(blockId), blockExecutionFacts);

            if(!Checks.TryCheckBlock(world, block, world.Facts.Filter(blockTrustedOrigin).Evaluate(block.Rules), blockId, out err))
                return false;

            blockId++;
        }

        err = null;
        return true;
    }
}