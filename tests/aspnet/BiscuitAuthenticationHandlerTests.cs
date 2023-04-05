using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Routing;

using biscuit_net.Builder;
using biscuit_net.Parser;
using biscuit_net.Datalog;

namespace biscuit_net.AspNet.Tests;

using Shared;

public class BiscuitAuthenticationHandlerTests : SharedAuthenticationTests<BiscuitBearerOptions>
{
    protected override string DefaultScheme => BiscuitBearerDefaults.AuthenticationScheme;
    protected override Type HandlerType => typeof(BiscuitBearerHandler);
    protected override bool SupportsSignIn { get => false; }
    protected override bool SupportsSignOut { get => false; }

    protected ISigningKey rootKey = Ed25519.NewSigningKey();

    protected readonly Func<HttpContext, Task> CheckAuthenticated = (context) => {context.Response.StatusCode = context.User?.Identity?.IsAuthenticated ?? false ? 200 : 401; return Task.CompletedTask;};

    protected override void RegisterAuth(AuthenticationBuilder services, Action<BiscuitBearerOptions> configure)
    {
        services.AddBiscuitBearer(o => 
        {
            ConfigureDefaults(o);
            configure.Invoke(o);
        });
    }

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
    public async Task Token_Signed_By_Authority_And_Validated_With_Right_Verification_Key_Should_Pass()
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
            endpoints.MapGet("/hello", [BiscuitPolicy("""
                check if operation("hello");
            """)]
            (context) => CheckAuthenticated(context));
        }, ConfigureDefaults);

        using var server = host.GetTestServer();

        var response = await TestHost.SendAsync(server, "http://example.com/hello", "Bearer " + token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task Token_Signed_By_Authority_And_Validated_With_Another_Verification_Key_Should_Not_Pass()
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

        var newKey = Ed25519.NewSigningKey();
        var newVerificationKey = new Ed25519.VerificationKey(newKey.Public);

        using var host = await TestHost.CreateHost(endpoints => 
        {
            endpoints.MapGet("/hello", [BiscuitPolicy("""
                check if operation("hello");
            """)]
            (context) => CheckAuthenticated(context));
        },
        o =>
        {
            o.PublicKey = newVerificationKey;
        });

        var newBearerToken = "Bearer " + token;
        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello", newBearerToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task SignInThrows()
    {
        using var host = await TestHost.CreateHost(endpoints => {
            endpoints.MapGet("/signIn", async context => await Assert.ThrowsAsync<InvalidOperationException>(() 
                => context.SignInAsync(BiscuitBearerDefaults.AuthenticationScheme, new ClaimsPrincipal())));
        }, ConfigureDefaults);

        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "https://example.com/signIn");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SignOutThrows()
    {
        using var host = await TestHost.CreateHost(endpoints => {
            endpoints.MapGet("/signOut", async context => await Assert.ThrowsAsync<InvalidOperationException>(() 
                => context.SignOutAsync(BiscuitBearerDefaults.AuthenticationScheme)));
        }, ConfigureDefaults);

        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "https://example.com/signOut");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HeaderWithoutBearerReceived_Should_Be_Unauthorized()
    {
        using var host = await TestHost.CreateHost(endpoints => {
            endpoints.MapGet("/hello", [BiscuitPolicy("""
                check if operation("hello");
            """)]
            (context) => CheckAuthenticated(context));
        }, ConfigureDefaults);
        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello", "Token");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnrecognizedTokenReceived_Should_Be_Unauthorized()
    {
        using var host = await TestHost.CreateHost(endpoints => {
            endpoints.MapGet("/hello", [BiscuitPolicy("""
                check if operation("hello");
            """)]
            (context) => CheckAuthenticated(context));
        }, ConfigureDefaults);

        using var server = host.GetTestServer();
        var response = await TestHost.SendAsync(server, "http://example.com/hello", "Bearer someblob");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}