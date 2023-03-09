using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;

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

        var trustedOrigins = TrustedOriginSet.Build(b, authorizerBlock);

        world.AddFacts(b, authorizerBlock);
        world.RunRules(b, trustedOrigins);
        
        //run authorizer rules 
        //var authorizerTrustedOrigin = trustedOrigins.Origins(uint.MaxValue, authorizerBlock.Scope);
        
        if(!world.RunChecks(b, authorizerBlock, trustedOrigins, out err))
        {
            return false;
        }
    
        if(!world.ValidatePolicies(authorizerBlock, trustedOrigins, out err))
        {
            return false;
        }

        return true;
    }
}