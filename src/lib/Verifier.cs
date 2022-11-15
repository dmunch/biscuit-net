using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public static class Verifier
{
    public static bool TryVerify(IBiscuit b, World world, AuthorizerBlock authorizerBlock, [NotNullWhen(false)] out Error? err)
    {
        if(!Checks.CheckBoundVariables(b, out var invalidBlockRule))
        {
            err = new Error(new FailedLogic(invalidBlockRule));
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
        world.Facts.Add(Origins.Authorizer, authorizerBlock.Facts.ToHashSet());
        world.Facts.Add(Origins.Authority, b.Authority.Facts.ToHashSet());

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Facts.Add(blockId, block.Facts.ToHashSet());
            blockId++;
        }

        
        var trustedOrigins = TrustedOriginSet.Build(b, authorizerBlock);

        //run rules
        //run authority rules 
        var authorityExecutionFacts = world.Facts.Filter(trustedOrigins.For(0, b.Authority.Scope)).Evaluate(b.Authority.Rules);
        world.Facts.Merge(Origins.Authority, authorityExecutionFacts);
        
        //run block rules 
        blockId = 1;
        foreach(var block in b.Blocks)
        {
            var blockExecutionFacts = world.Facts.Filter(trustedOrigins.For(blockId, block.Scope)).Evaluate(block.Rules);
            world.Facts.Merge(blockId, blockExecutionFacts);

            blockId++;
        }

        //run authorizer rules 
        var authorizerTrustedOrigin = trustedOrigins.For(uint.MaxValue, authorizerBlock.Scope);
        
        //run checks
        //run authority checks
        if(!Checks.TryCheck(world.Facts, trustedOrigins, Origins.Authority, b.Authority.Checks, out var failedCheckId, out var failedCheck))
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
        if(!Checks.TryCheck(world.Facts, trustedOrigins, Origins.Authorizer, authorizerBlock.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }


        //validate policies 
        foreach(var policy in authorizerBlock.Policies)
        {
            if(Checks.TryCheckOne(world.Facts, trustedOrigins, Origins.Authorizer, policy.Rules))
            {
                err = new Error(new FailedLogic(new Unauthorized(policy.Kind)));
                return policy.Kind switch {
                    PolicyKind.Allow => true,
                    PolicyKind.Deny => false,
                    _ => throw new NotSupportedException("Unsupported policy kind")
                };
            }
        }

        err = new Error(new FailedLogic(new NoMatchingPolicy()));
        return false;
    }
}