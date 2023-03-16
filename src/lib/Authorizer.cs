using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class VersionException : Exception
{
    public VersionException(string message) : base(message)
    {
    }   
}

public class Authorizer
{
    public World World { get; } = new World();
    readonly AuthorizerBlock _authorizerBlock = new();
    
    public void Add(Fact fact) => _authorizerBlock.Add(fact);
    public void Add(Rule rule) => _authorizerBlock.Add(rule);
    public void Add(Check check) => _authorizerBlock.Add(check);
    public void Add(Policy policy) => _authorizerBlock.Add(policy);
    public void Add(AuthorizerBlock authorizer) => _authorizerBlock.Add(authorizer);
    
    public Authorizer() {}
    public Authorizer(AuthorizerBlock authorizerBlock)
    {
        Add(authorizerBlock);
    }
    
    public bool TryAuthorize(Biscuit b, [NotNullWhen(false)] out Error? err)
    {        
        
        return TryAuthorize(b.Authority, b.Blocks, World, _authorizerBlock, out err);
    }

    public static bool TryAuthorize(Block authority, IEnumerable<Block> blocks, World world, AuthorizerBlock authorizerBlock, [NotNullWhen(false)] out Error? err)
    {
        if(!Checks.CheckBoundVariables(authority, blocks, out var invalidBlockRule))
        {
            err = new Error(new FailedLogic(invalidBlockRule));
            return false;
        }

        if(authority.Version < 3 || authority.Version > 4)
            throw new VersionException($"Unsupported Authority Block Version {authority.Version}");

        foreach(var version in blocks.Select(b => b.Version))
        {
            if(version < 3 || version > 4)
                throw new VersionException($"Unsupported Block Version {version}");
        }

        var trustedOrigins = TrustedOriginSet.Build(authority, blocks, Scope.DefaultBlockScope);

        world.AddFacts(authority, blocks, authorizerBlock);
        if(!world.RunRules(authority, blocks, authorizerBlock, trustedOrigins, out err))
        {
            return false;
        }
        
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
