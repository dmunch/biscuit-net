using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace biscuit_net.AspNet;

public class BiscuitPolicyRequirement : IAuthorizationRequirement
{
    public BiscuitPolicyRequirement(string policyCode) =>
        PolicyCode = policyCode;

    public string PolicyCode { get; }
}

public class BiscuitPolicyHandler : AuthorizationHandler<BiscuitPolicyRequirement>
{
    readonly ILogger _logger;
    
    public BiscuitPolicyHandler(ILoggerFactory logger)
    {
        _logger = logger.CreateLogger<BiscuitBearerHandler>();
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, BiscuitPolicyRequirement requirement)
    {
        if(!(context.Resource is HttpContext httpContext))
        {            
            _logger.LogCritical(1, "Couldn't retrieve HttpContext");
            return Task.CompletedTask;
        }
        
        if (httpContext.Items[BiscuitBearerDefaults.BiscuitAuthorizerHttpContextItemsKey] is not Authorizer authorizer)
        {            
            _logger.LogCritical(2, "Couldn't retrieve Biscuit Authorizer. Make sure that the Biscuit Authentication Handler is properly configured.");
            return Task.CompletedTask;
        }
        if (httpContext.Items[BiscuitBearerDefaults.BiscuitHttpContextItemsKey] is not Biscuit biscuit)
        {            
            _logger.LogCritical(3, "Couldn't retrieve Biscuit. Make sure that the Biscuit Authentication Handler is properly configured.");
            return Task.CompletedTask;
        }

        var parser = new Parser.Parser();
        var requirementAuthorizerBlock = parser.ParseAuthorizer(requirement.PolicyCode);
        authorizer.Add(requirementAuthorizerBlock);

        if(!authorizer.TryAuthorize(biscuit, out _))
        {
            _logger.LogWarning(4, "Biscuit authorizer denied access");
            context.Fail();
        } else 
        {
            context.Succeed(requirement);
        }

        var endpoint = httpContext.GetEndpoint();
        var biscuitAttribute = endpoint?.Metadata?.GetMetadata<BiscuitPolicyAttribute>();
        if(biscuitAttribute != null)
        {
            var attributeAuthorizerblock = parser.ParseAuthorizer(biscuitAttribute.PolicyCode);
            authorizer.Add(attributeAuthorizerblock);
        }

        /* TODO we could use route-data to map to facts. */
        /*
        var routeData = httpContext.GetRouteData();
        var routeEndpoint = endpoint as RouteEndpoint;
        foreach(var parameter in routeEndpoint.RoutePattern.Parameters)
        {
            var contents = parameter.ParameterPolicies.Select(pp => pp.Content);
            // contents is on of these route constraints, i.e. int, bool, datetime 
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#route-constraints
            
            if(routeData.Values.TryGetValue(parameter.Name, out var value))
            {
                // parse the route data value according to the format constraint
                // and add it as fact
            }
        }
        */
        

        return Task.CompletedTask;
    }
}