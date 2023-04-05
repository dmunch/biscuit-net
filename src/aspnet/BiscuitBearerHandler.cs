using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using biscuit_net.Datalog;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace biscuit_net.AspNet;

public class BiscuitBearerHandler : AuthenticationHandler<BiscuitBearerOptions>
{
    public BiscuitBearerHandler(IOptionsMonitor<BiscuitBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(HandleAuthenticate());
    }

    protected AuthenticateResult HandleAuthenticate()
    {
        string? token = null;
        string authorization = Request.Headers.Authorization.ToString();

        if(Options.PublicKey == null)
        {
            throw new InvalidOperationException("A public key for token verification needs to be set");
        }

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization))
        {
            return AuthenticateResult.NoResult();
        }

        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorization["Bearer ".Length..].Trim();
        }

        // If no token found, no further work possible
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        var bytes = Base64UrlEncoder.DecodeBytes(token);        
        try
        {
            if (!Biscuit.TryDeserialize(bytes, Options.PublicKey, out var biscuit, out var formatErr))
            {            
                Logger.TokenValidationFailed(formatErr);
                return AuthenticateResult.Fail("invalid signature");
            }
            Logger.TokenValidationSucceeded();

            var authorizerBlock = new AuthorizerBlock();
            if(Options.AuthorizerBlock != null)
            {
                authorizerBlock.Add(Options.AuthorizerBlock);
            }

            var authorizer = new Authorizer(authorizerBlock);

            foreach(var factProvider in Options.FactProviders) 
            {
                authorizer.Add(factProvider(Context, Clock));
            }
            
            var success = authorizer.TryAuthorize(biscuit, out var err);
            
            if(!success)
            {
                Logger.TokenValidationFailed(err!);
                return AuthenticateResult.Fail("policy check failures");
            }

            var claimsFacts = authorizer.World.Facts.Where(f => f.Name == Options.ClaimsFactName);
            var claims = claimsFacts.Select(fact => {
                var terms = fact.Terms.ToList();
                var claimName = ((Datalog.String) terms.ToList()[0]).Value;
                return terms[1] switch {
                    Datalog.String s => new Claim(claimName, s.Value, ClaimValueTypes.String),
                    Date d => new Claim(claimName, Date.FromTAI64(d.Value).ToString("o"), ClaimValueTypes.DateTime),
                    Integer d => new Claim(claimName, d.Value.ToString(), ClaimValueTypes.Integer64),
                    _ => throw new Exception($"Datalog type {terms[1].GetType()} not support as claim")
                };
            });

            var identity = new ClaimsIdentity(claims, Scheme.Name, "sub", null);
            var principal = new ClaimsPrincipal(identity);
            
            var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
            {
                Principal = principal,
                Biscuit = biscuit!
            };
            
            tokenValidatedContext.Success();

            Context.Items[BiscuitBearerDefaults.BiscuitAuthorizerHttpContextItemsKey] = authorizer;
            Context.Items[BiscuitBearerDefaults.BiscuitHttpContextItemsKey] = biscuit;

            return tokenValidatedContext.Result;
        } 
        catch(EndOfStreamException e)
        {
            //TODO this exception should probably be catched in Biscuit.TryDeserialize
            Logger.TokenValidationFailed(e);
            return AuthenticateResult.Fail(e);
        }
    }
}