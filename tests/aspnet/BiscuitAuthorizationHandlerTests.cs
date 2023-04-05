using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Routing;

using biscuit_net.Builder;
using biscuit_net.Parser;
using biscuit_net.Datalog;

namespace biscuit_net.AspNet.Tests;

public class BiscuitAuthorizationHandlerTests
{
    protected ISigningKey rootKey = Ed25519.NewSigningKey();

    private void ConfigureDefaults(BiscuitBearerOptions o)
    {
        o.PublicKey = new Ed25519.VerificationKey(rootKey.Public);
        o.ClaimsFactName = "claims";
                
        o.FactProviders.Add(
            (context, clock) => new Fact("now", clock.UtcNow.DateTime)
        );

        o.AuthorizerCode = """
            check if now($now), claims("exp", $exp), $exp > $now; //token expiry time isn't reached
            check all now($now), claims("nbf", $nbf), $nbf < $now; //token is already valid - check all will skip if there's no claims("nbf")
            
            allow if true;
        """;
    }

    [Fact]
    public async Task Token_Containing_Facts_Statisfying_Datalog_Policy_Should_Pass()
    {
        var token = WebEncoders.Base64UrlEncode(Biscuit.New(rootKey)
                .AuthorityBlock("""
                    claims("sub", "biscuitName");
                    claims("iss", "my-issuer");
                    claims("exp", 2023-03-26T21:52:00Z);
                    claims("iat", 2023-03-26T19:52:00Z);
                    claims("nbf", 2023-03-26T18:52:00Z);
                    operation("hello");
                """)
                .EndBlock()
                .Serialize()
        );

        using var host = await TestHost.CreateHost(endpoints => 
            {
                endpoints
                    .MapGet("/hello/", () => "hi!")                
                    .RequireAuthorization("biscuit");
            }, 
            ConfigureDefaults, 
            authzOptions => {
                authzOptions.AddPolicy("biscuit", policy=> 
                    policy.Requirements.Add(new BiscuitPolicyRequirement("""
                        check if operation("hello");
                    """))
                );
        });
        
        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello", "Bearer "+ token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Token_Not_Containing_Facts_And_Not_Statisfying_Datalog_Policy_Should_Fail()
    {
        var token = WebEncoders.Base64UrlEncode(Biscuit.New(rootKey)
                .AuthorityBlock("""
                    claims("sub", "biscuitName");
                    claims("iss", "my-issuer");
                    claims("exp", 2023-03-26T21:52:00Z);
                    claims("iat", 2023-03-26T19:52:00Z);
                    claims("nbf", 2023-03-26T18:52:00Z);
                """)
                .EndBlock()
                .Serialize()
        );

        using var host = await TestHost.CreateHost(endpoints => 
            {
                endpoints
                    .MapGet("/hello/", () => "hi!")
                    .RequireAuthorization("biscuit");
            }, 
            ConfigureDefaults, 
            authzOptions => {
                authzOptions.AddPolicy("biscuit", policy=> 
                    policy.Requirements.Add(new BiscuitPolicyRequirement("""
                        check if operation("hello");
                    """))
                );
        });

        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello",  "Bearer " + token);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task When_Biscuit_Auth_Not_Configured_Should_Fail()
    {
        using var host = await TestHost.CreateHost(endpoints => 
            {
                endpoints
                    .MapGet("/hello/", () => "hi!")
                    .RequireAuthorization("biscuit");
            }, 
            null, //don't configure biscuit auth 
            authzOptions => {
                authzOptions.AddPolicy("biscuit", policy=> 
                    policy.Requirements.Add(new BiscuitPolicyRequirement("""
                        check if operation("hello");
                    """))
                );
        });

        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello",  "Bearer whatever");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}