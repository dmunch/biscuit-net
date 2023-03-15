namespace biscuit_net;

public record Error
{
    public Error(FailedBlockCheck block) => Block = block;
    public Error(FailedAuthorizerCheck authorizer) => Authorizer = authorizer;
    public Error(FailedLogic failedLogic) => FailedLogic = failedLogic;
    public Error(FailedExecution failedExecution) => Execution = failedExecution;

    public FailedBlockCheck? Block { get; } = null;
    public FailedAuthorizerCheck? Authorizer { get; } = null;
    public FailedLogic? FailedLogic { get; } = null;
    public FailedExecution? Execution { get; } = null;
}

public record FailedBlockCheck(uint BlockId, int CheckId/*, RuleExpressions Rule*/);
public record FailedAuthorizerCheck(int CheckId/*, RuleExpressions Rule*/);
//TODO Assuming the int is a RuleId - specification and examples are unclear here
public record InvalidBlockRule(int RuleId/*, RuleExpressions Rule*/);

public record FailedLogic(
    InvalidBlockRule? InvalidBlockRule,
    NoMatchingPolicy? NoMatchingPolicy,
    Unauthorized? Unauthorized)
{
    public FailedLogic(InvalidBlockRule invalidBlockRule) : this(invalidBlockRule, null, null) {}
    public FailedLogic(NoMatchingPolicy noMatchingPolicy) : this(null, noMatchingPolicy, null) {}
    public FailedLogic(Unauthorized unauthorized) : this(null, null, unauthorized) {}
}

public record NoMatchingPolicy();
public record Unauthorized(PolicyKind Policy);

public record FailedExecution(string Reason);