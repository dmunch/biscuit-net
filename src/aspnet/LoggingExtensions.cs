using Microsoft.Extensions.Logging;

namespace biscuit_net.AspNet;

static partial class LoggingExtensions
{
    [LoggerMessage(1, LogLevel.Information, "Failed to validate the token.", EventName = "TokenValidationFailed")]
    public static partial void TokenValidationFailed(this ILogger logger, FailedFormat fmtError);
    [LoggerMessage(1, LogLevel.Information, "Failed to validate the token.", EventName = "TokenValidationFailed")]
    public static partial void TokenValidationFailed(this ILogger logger, Exception fmtError);

    [LoggerMessage(1, LogLevel.Information, "Failed to validate the token.", EventName = "TokenValidationFailed")]
    public static partial void TokenValidationFailed(this ILogger logger, Error error);

    [LoggerMessage(2, LogLevel.Debug, "Successfully validated the token.", EventName = "TokenValidationSucceeded")]
    public static partial void TokenValidationSucceeded(this ILogger logger);

    [LoggerMessage(3, LogLevel.Error, "Exception occurred while processing message.", EventName = "ProcessingMessageFailed")]
    public static partial void ErrorProcessingMessage(this ILogger logger, Exception ex);

    [LoggerMessage(4, LogLevel.Debug, "Unable to reject the response as forbidden, it has already started.", EventName = "ForbiddenResponseHasStarted")]
    public static partial void ForbiddenResponseHasStarted(this ILogger logger);
}