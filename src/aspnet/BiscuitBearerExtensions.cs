using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace biscuit_net.AspNet;

/// <summary>
/// Extension methods to configure Biscuit bearer authentication.
/// </summary>
public static class BiscuitBearerExtensions
{
    /// <summary>
    /// Enables Biscuit-bearer authentication using the default scheme <see cref="BiscuitBearerDefaults.AuthenticationScheme"/>.
    /// <para>
    /// Biscuit bearer authentication performs authentication by extracting and validating a Biscuit token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddBiscuitBearer(this AuthenticationBuilder builder)
        => builder.AddBiscuitBearer(BiscuitBearerDefaults.AuthenticationScheme, _ => { });

    /// <summary>
    /// Enables Biscuit-bearer authentication using a pre-defined scheme.
    /// <para>
    /// Biscuit bearer authentication performs authentication by extracting and validating a Biscuit token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddBiscuitBearer(this AuthenticationBuilder builder, string authenticationScheme)
        => builder.AddBiscuitBearer(authenticationScheme, _ => { });

    /// <summary>
    /// Enables Biscuit-bearer authentication using the default scheme <see cref="BiscuitBearerDefaults.AuthenticationScheme"/>.
    /// <para>
    /// Biscuit bearer authentication performs authentication by extracting and validating a Biscuit token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="BiscuitBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddBiscuitBearer(this AuthenticationBuilder builder, Action<BiscuitBearerOptions> configureOptions)
        => builder.AddBiscuitBearer(BiscuitBearerDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Enables Biscuit-bearer authentication using the specified scheme.
    /// <para>
    /// Biscuit bearer authentication performs authentication by extracting and validating a Biscuit token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="BiscuitBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddBiscuitBearer(this AuthenticationBuilder builder, string authenticationScheme, Action<BiscuitBearerOptions> configureOptions)
        => builder.AddBiscuitBearer(authenticationScheme, displayName: null, configureOptions: configureOptions);

    /// <summary>
    /// Enables Biscuit-bearer authentication using the specified scheme.
    /// <para>
    /// Biscuit bearer authentication performs authentication by extracting and validating a Biscuit token from the <c>Authorization</c> request header.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="displayName">The display name for the authentication handler.</param>
    /// <param name="configureOptions">A delegate that allows configuring <see cref="BiscuitBearerOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddBiscuitBearer(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<BiscuitBearerOptions> configureOptions)
    {
        //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<BiscuitBearerOptions>, BiscuitBearerConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<BiscuitBearerOptions>, BiscuitBearerPostConfigureOptions>());
        return builder.AddScheme<BiscuitBearerOptions, BiscuitBearerHandler>(authenticationScheme, displayName, configureOptions);
    }
}