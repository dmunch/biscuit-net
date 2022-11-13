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


        //add facts
        world.Facts.Add(Origin.Authorizer, authorizerBlock.Facts.ToHashSet());
        world.Facts.Add(Origin.Authority, b.Authority.Facts.ToHashSet());

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Facts.Add(new Origin(blockId), block.Facts.ToHashSet());
            blockId++;
        }

        
        var trustedOrigins = TrustedOrigins.Build(b);

        //run rules
        //run authority rules 
        var authorityExecutionFacts = world.Facts.Filter(trustedOrigins.For(0, b.Authority.Scope)).Evaluate(b.Authority.Rules);
        world.Facts.Merge(Origin.Authority, authorityExecutionFacts);
        
        //run block rules 
        blockId = 1;
        foreach(var block in b.Blocks)
        {
            var blockExecutionFacts = world.Facts.Filter(trustedOrigins.For(blockId, block.Scope)).Evaluate(block.Rules);
            world.Facts.Merge(new Origin(blockId), blockExecutionFacts);

            blockId++;
        }

        //run authorizer rules 
        var authorizerTrustedOrigin = trustedOrigins.For(uint.MaxValue, authorizerBlock.Scope);
        
        //run checks
        //run authority checks
        if(!Checks.TryCheck(world.Facts, trustedOrigins, 0, b.Authority.Checks, out var failedCheckId, out var failedCheck))
        {
            err = new Error(new FailedBlockCheck(0, failedCheckId.Value/*, failedRule*/));
            return false;
        }

        //run block checks
        blockId = 1;
        foreach(var block in b.Blocks)
        {
            if(!Checks.TryCheck(world.Facts, trustedOrigins, blockId, block.Checks, out var failedBlockCheckId, out var failedBlockCheck))
            {
                err = new Error(new FailedBlockCheck(blockId, failedBlockCheckId.Value/*, failedRule*/));
                return false;
            }
            blockId++;
        }
            
        //run authorizer checks
        if(!Checks.TryCheck(world.Facts, trustedOrigins, uint.MaxValue, authorizerBlock.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }


        err = null;
        return true;
    }
}