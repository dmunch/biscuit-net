using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;

public static class Verifier
{
    public static bool TryVerify(Block authority, IEnumerable<Block> blocks, World world, AuthorizerBlock authorizerBlock, [NotNullWhen(false)] out Error? err)
    {
        if(!Checks.CheckBoundVariables(authority, blocks, out var invalidBlockRule))
        {
            err = new Error(new FailedLogic(invalidBlockRule));
            return false;
        }

        if(authority.Version < 3 || authority.Version > 4)
            throw new Exception($"Unsupported Authority Block Version {authority.Version}");

        foreach(var block in blocks)
        {
            if(block.Version < 3 || block.Version > 4)
                throw new Exception($"Unsupported Block Version {authority.Version}");
        }

        var trustedOrigins = TrustedOriginSet.Build(authority, blocks, authorizerBlock.Scope);

        world.AddFacts(authority, blocks, authorizerBlock);
        if(!world.RunRules(authority, blocks, trustedOrigins, out err))
        {
            return false;
        }
        
        //run authorizer rules 
        //var authorizerTrustedOrigin = trustedOrigins.Origins(uint.MaxValue, authorizerBlock.Scope);
        
        if(!world.RunChecks(authority, blocks, authorizerBlock, trustedOrigins, out err))
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