using Microsoft.Extensions.Options;

namespace biscuit_net.AspNet;

/// <summary>
/// Used to setup defaults for all <see cref="BiscuitBearerOptions"/>.
/// </summary>
public class BiscuitBearerPostConfigureOptions : IPostConfigureOptions<BiscuitBearerOptions>
{
    /// <summary>
    /// Invoked to post configure a BiscuitBearerOptions instance.
    /// </summary>
    /// <param name="name">The name of the options instance being configured.</param>
    /// <param name="options">The options instance to configure.</param>
    public void PostConfigure(string? name, BiscuitBearerOptions options)
    {
        if(options.PublicKey == null)
        {
            throw new InvalidOperationException("A public key for token verification needs to be set");
        }
        
        if(string.IsNullOrEmpty(options.AuthorizerCode))
        {
            options.AuthorizerBlock = new AuthorizerBlock();
            return;
        }

        var parser = new Parser.Parser();                
        options.AuthorizerBlock =  parser.ParseAuthorizer(options.AuthorizerCode);
    }
}