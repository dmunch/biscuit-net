using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public static class Verifier
{
    public static bool TryVerify(IBiscuit b, World world, IBlock authorizerBlock, [NotNullWhen(false)] out Error? err)
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


        world.Facts.Add(Origin.Authorizer, authorizerBlock.Facts.ToHashSet());
        //TODO: Evaluate authorizer rules

        world.Facts.Add(Origin.Authority, b.Authority.Facts.ToHashSet());

        var authorityTrustedOrigin = new TrustedOrigin(Origin.Authority, Origin.Authorizer);
        var authorityExecutionFacts = world.Facts.Filter(authorityTrustedOrigin).Evaluate(b.Authority.Rules);
        world.Facts.Merge(Origin.Authority, authorityExecutionFacts);
        
        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Facts.Add(new Origin(blockId), block.Facts.ToHashSet());

            var blockTrustedOrigin = new TrustedOrigin(Origin.Authority, (Origin)blockId, Origin.Authorizer);
            var blockExecutionFacts = world.Facts.Filter(blockTrustedOrigin).Evaluate(block.Rules);
            world.Facts.Merge(new Origin(blockId), blockExecutionFacts);

            blockId++;
        }

        //run authority checks
        if(!Checks.TryCheck(world.Facts.Filter(authorityTrustedOrigin), b.Authority.Checks, out var failedCheckId, out var failedCheck))
        {
            err = new Error(new FailedBlockCheck(0, failedCheckId.Value/*, failedRule*/));
            return false;
        }

        //run block checks
        blockId = 1;
        foreach(var block in b.Blocks)
        {
            var blockTrustedOrigin = new TrustedOrigin(Origin.Authority, (Origin)blockId, Origin.Authorizer);
            if(!Checks.TryCheck(world.Facts.Filter(blockTrustedOrigin), block.Checks, out var failedBlockCheckId, out var failedBlockCheck))
            {
                err = new Error(new FailedBlockCheck(blockId, failedBlockCheckId.Value/*, failedRule*/));
                return false;
            }
            blockId++;
        }
            
        //run authorizer checks
        var authorizerTrustedOrigin = new TrustedOrigin(Origin.Authority, Origin.Authorizer);
        for(uint i = 1; i < blockId; i++)
        {
            authorityTrustedOrigin.Add(i);
        }
        if(!Checks.TryCheck(world.Facts.Filter(authorizerTrustedOrigin), authorizerBlock.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }


        err = null;
        return true;
    }
}