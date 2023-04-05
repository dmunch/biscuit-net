using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace biscuit_net.AspNet.Tests;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

public static class TestHost
{
    public static async Task<IHost> CreateHost(Action<IEndpointRouteBuilder> endpointRouteBuilder, Action<BiscuitBearerOptions>? options = null, Action<AuthorizationOptions>? authzOptions = null)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(builder =>
                builder.UseTestServer()                    
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseAuthentication();
                        if(authzOptions != null) 
                        {
                            app.UseAuthorization();
                        }
                        
                        
                        app.UseEndpoints(endpoints => {
                            endpointRouteBuilder(endpoints);
                        });
                    })
                    .ConfigureServices(services => {
                        if(options != null)
                        {                            
                            services
                                .AddAuthentication(BiscuitBearerDefaults.AuthenticationScheme)
                                .AddBiscuitBearer(o => {
                                    options?.Invoke(o);
                                });
                        } else 
                        {
                            services
                                .AddAuthentication("nullauthscheme")
                                .AddScheme<NullAuthOptions, NullAuthHandler>("nullauthscheme", o => {});
                        }

                        if(authzOptions != null)
                        {
                            services.AddAuthorization(options => {
                                authzOptions(options);
                            });
                        }
                        
                        services.AddSingleton<IAuthorizationHandler, BiscuitPolicyHandler>();
                        
                        services.AddSingleton<ISystemClock, TestClock>();
                        services.AddRouting();
                    }))
            .Build();

        await host.StartAsync();
        return host;
    }

    public static async Task<HttpResponseMessage> SendAsync(TestServer server, string uri, string? authorizationHeader = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            request.Headers.Add("Authorization", authorizationHeader);
        }

        return await server.CreateClient().SendAsync(request);
    }
}

public class NullAuthOptions : AuthenticationSchemeOptions
{

}

public class NullAuthValidatedContext : ResultContext<NullAuthOptions>
{  
    public NullAuthValidatedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        NullAuthOptions options)
        : base(context, scheme, options) { }   
}

public class NullAuthHandler : AuthenticationHandler<NullAuthOptions>
{
    public NullAuthHandler(IOptionsMonitor<NullAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new [] { new Claim("sub", "anyuser") };
        var identity = new ClaimsIdentity(claims, Scheme.Name, "sub", null);
        var principal = new ClaimsPrincipal(identity);
            
        var validatedContext = new NullAuthValidatedContext(Context, Scheme, Options)
        {
            Principal = principal
        };

        return Task.FromResult(validatedContext.Result);
    }
}