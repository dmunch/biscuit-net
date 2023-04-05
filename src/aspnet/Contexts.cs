using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace biscuit_net.AspNet;

public class TokenValidatedContext : ResultContext<BiscuitBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="TokenValidatedContext"/>.
    /// </summary>
    /// <inheritdoc />
    public TokenValidatedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        BiscuitBearerOptions options)
        : base(context, scheme, options) { }

    /// <summary>
    /// Gets or sets the validated security token.
    /// </summary>
    public Biscuit Biscuit { get; set; } = default!;
}

/// <summary>
/// A <see cref="ResultContext{TOptions}"/> when access to a resource is forbidden.
/// </summary>
public class ForbiddenContext : ResultContext<BiscuitBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ForbiddenContext"/>.
    /// </summary>
    /// <inheritdoc />
    public ForbiddenContext(
        HttpContext context,
        AuthenticationScheme scheme,
        BiscuitBearerOptions options)
        : base(context, scheme, options) { }
}

/// <summary>
/// A <see cref="PropertiesContext{TOptions}"/> when access to a resource authenticated using Biscuit bearer is challenged.
/// </summary>
public class ChallengeContext : PropertiesContext<BiscuitBearerOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChallengeContext"/>.
    /// </summary>
    /// <inheritdoc />
    public ChallengeContext(
        HttpContext context,
        AuthenticationScheme scheme,
        BiscuitBearerOptions options,
        AuthenticationProperties properties)
        : base(context, scheme, options, properties) { }

    /// <summary>
    /// Any failures encountered during the authentication process.
    /// </summary>
    public Exception? AuthenticateFailure { get; set; }

    /// <summary>
    /// Gets or sets the "error" value returned to the caller as part
    /// of the WWW-Authenticate header. This property may be null when
    /// <see cref="BiscuitBearerOptions.IncludeErrorDetails"/> is set to <c>false</c>.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the "error_description" value returned to the caller as part
    /// of the WWW-Authenticate header. This property may be null when
    /// <see cref="BiscuitBearerOptions.IncludeErrorDetails"/> is set to <c>false</c>.
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Gets or sets the "error_uri" value returned to the caller as part of the
    /// WWW-Authenticate header. This property is always null unless explicitly set.
    /// </summary>
    public string? ErrorUri { get; set; }

    /// <summary>
    /// If true, will skip any default logic for this challenge.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// Skips any default logic for this challenge.
    /// </summary>
    public void HandleResponse() => Handled = true;
}