using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using biscuit_net.Datalog;
using Microsoft.AspNetCore.Http;

namespace biscuit_net.AspNet;

public class BiscuitBearerOptions : AuthenticationSchemeOptions
{
    [AllowNull]
    public IVerificationKey? PublicKey { get; set; }
    public string? ClaimsFactName { get; set; }
    public string? AuthorizerCode { get; set; }

    public List<Func<HttpContext, ISystemClock, Fact>> FactProviders { get; } = new ();

    public AuthorizerBlock? AuthorizerBlock { get; set; }

    /// <summary>
    /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
    /// </summary>
    public string Challenge { get; set; } = BiscuitBearerDefaults.AuthenticationScheme;
}

public static class BiscuitBearerDefaults 
{
    public const string AuthenticationScheme = "BiscuitBearer";

    public const string BiscuitHttpContextItemsKey = "Biscuit";
    public const string BiscuitAuthorizerHttpContextItemsKey = "BiscuitAuthorizer";
}
