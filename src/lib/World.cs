namespace biscuit_net;

using System.Diagnostics.CodeAnalysis;
using Datalog;

public record World(FactSet Facts, RuleSet Rules)
{
    public World() : this(new FactSet(), new RuleSet())
    {
    }

    public void AddFacts(IBiscuit b, AuthorizerBlock authorizerBlock)
    {
        //add facts
        Facts.Add(Origins.Authorizer, authorizerBlock.Facts.ToHashSet());
        Facts.Add(Origins.Authority, b.Authority.Facts.ToHashSet());

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            Facts.Add(blockId, block.Facts.ToHashSet());
            blockId++;
        }
    }
    
    public void RunRules(IBiscuit b, TrustedOriginSet trustedOrigins) 
    {
        //run authority rules             
        RunRules(trustedOrigins.With(Origins.Authority), b.Authority);

        //run block rules 
        uint blockId = 1;
        foreach(var block in b.Blocks)
        {            
            RunRules(trustedOrigins.With(blockId++), block);
        }
    }

    void RunRules(BlockTrustedOriginSet origins, IBlock block) 
    {
        //var origins = trustedOrigins.With(blockId);
        var executionFacts = Facts
            .Filter(origins.Origins())
            .Evaluate(block.Rules, Facts, origins);

        Facts.UnionWith(origins.BlockId, executionFacts);
    }

    public bool RunChecks(IBiscuit b, AuthorizerBlock authorizerBlock, TrustedOriginSet trustedOrigins, [NotNullWhen(false)] out Error? err) 
    {
        //run checks
        //run authority checks
        if(!Checks.TryCheck(Facts, trustedOrigins.With(Origins.Authority), b.Authority.Checks, out var failedCheckId, out var failedCheck))
        {
            err = new Error(new FailedBlockCheck(0, failedCheckId.Value/*, failedRule*/));
            return false;
        }

        //run block checks
        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            if(!Checks.TryCheck(Facts, trustedOrigins.With(blockId), block.Checks, out var failedBlockCheckId, out var failedBlockCheck))
            {
                err = new Error(new FailedBlockCheck(blockId, failedBlockCheckId.Value/*, failedRule*/));
                return false;
            }
            blockId++;
        }

        //run authorizer checks
        if(!Checks.TryCheck(Facts, trustedOrigins.With(Origins.Authorizer), authorizerBlock.Checks, out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
        {
            err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
            return false;
        }

        err = null;
        return true;
    }

    public bool ValidatePolicies(AuthorizerBlock authorizerBlock, TrustedOriginSet trustedOrigins, [NotNullWhen(false)] out Error err) 
    {
         //validate policies 
        foreach(var policy in authorizerBlock.Policies)
        {
            if(Checks.TryCheckOne(Facts, trustedOrigins.With(Origins.Authorizer), policy.Rules))
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
