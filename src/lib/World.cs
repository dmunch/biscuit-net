namespace biscuit_net;

using System.Diagnostics.CodeAnalysis;
using Datalog;

public class World
{
    readonly FactSet _facts = new();

    public IEnumerable<Fact> Facts => _facts.Values;
    
    public World()
    {
    }

    public void AddFacts(Block authority, IEnumerable<Block> blocks, AuthorizerBlock authorizerBlock)
    {
        //add facts
        _facts.Add(KnownOrigins.Authorizer, authorizerBlock.Facts.ToHashSet());
        _facts.Add(KnownOrigins.Authority, authority.Facts.ToHashSet());

        uint blockId = 1;
        foreach(var block in blocks)
        {
            _facts.Add(blockId, block.Facts.ToHashSet());
            blockId++;
        }
    }
    
    public bool RunRules(Block authority, IEnumerable<Block> blocks, AuthorizerBlock authorizer, TrustedOriginSet trustedOrigins, [NotNullWhen(false)] out Error? err) 
    {
        try
        {
            //run authority rules             
            RunRules(KnownOrigins.Authority, trustedOrigins, authority.Rules);

            //run block rules 
            uint blockId = 1;
            foreach(var block in blocks)
            {            
                RunRules(blockId++, trustedOrigins, block.Rules);
            }

            RunRules(KnownOrigins.Authorizer, trustedOrigins, authorizer.Rules);
        } catch(OverflowException)
        {
            err = new Error(new FailedExecution("Overflow"));
            return false;
        }

        err = null;
        return true;
    }

    void RunRules(uint blockId, TrustedOriginSet origins, IEnumerable<RuleScoped> rules) 
    {
        var executionFacts = new HashSet<Fact>();
        foreach(var rule in rules)
        {
            var facts = _facts.Filter(origins.Origins(blockId, rule.Scope));
            executionFacts.UnionWith(facts.Evaluate(rule));
        }
        _facts.UnionWith(blockId, executionFacts);
    }
            
    public bool RunChecks(Block authority, IEnumerable<Block> blocks, AuthorizerBlock authorizerBlock, TrustedOriginSet trustedOrigins, [NotNullWhen(false)] out Error? err) 
    {
        try
        {
            IEnumerable<Fact> FactProvider(RuleScoped rule, uint blockId) 
                => _facts.Filter(trustedOrigins.Origins(blockId, rule.Scope));

            //run checks
            //run authority checks
            if(!Checks.TryCheck(authority.Checks, rule => FactProvider(rule, KnownOrigins.Authority), out var failedCheckId, out var failedCheck))
            {
                err = new Error(new FailedBlockCheck(0, failedCheckId.Value/*, failedRule*/));
                return false;
            }

            //run block checks
            uint blockId = 1;
            foreach(var block in blocks)
            {
                if(!Checks.TryCheck(block.Checks, rule => FactProvider(rule, blockId), out var failedBlockCheckId, out var failedBlockCheck))
                {
                    err = new Error(new FailedBlockCheck(blockId, failedBlockCheckId.Value/*, failedRule*/));
                    return false;
                }
                blockId++;
            }

            //run authorizer checks
            if(!Checks.TryCheck(authorizerBlock.Checks, rule => FactProvider(rule, KnownOrigins.Authorizer), out var failedAuthorizerCheckId, out var failedAuthorizerCheck))
            {
                err = new Error(new FailedAuthorizerCheck(failedAuthorizerCheckId.Value/*, failedAuthorizerRule*/));
                return false;
            }
        }
        catch (OverflowException)
        {
            err = new Error(new FailedExecution("Overflow"));
            return false;
        }

        err = null;
        return true;
    }

    public bool ValidatePolicies(AuthorizerBlock authorizerBlock, TrustedOriginSet trustedOrigins, [NotNullWhen(false)] out Error err) 
    {
        try 
        {
            IEnumerable<Fact> FactProvider(RuleScoped rule, uint blockId) 
                => _facts.Filter(trustedOrigins.Origins(blockId, rule.Scope));
    
            //validate policies 
            foreach(var policy in authorizerBlock.Policies)
            {
                if(Checks.TryCheckOne(policy.Rules, rule => FactProvider(rule, KnownOrigins.Authorizer)))
                {
                    err = new Error(new FailedLogic(new Unauthorized(policy.Kind)));
                    return policy.Kind switch {
                        PolicyKind.Allow => true,
                        PolicyKind.Deny => false,
                        _ => throw new NotSupportedException("Unsupported policy kind")
                    };
                }
            }
        }
        catch (OverflowException)
        {
            err = new Error(new FailedExecution("Overflow"));
            return false;
        }

        err = new Error(new FailedLogic(new NoMatchingPolicy()));
        return false;
    }
}
