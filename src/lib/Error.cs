namespace biscuit_net;

public record Error
{
    public Error(FailedBlockCheck block) => Block = block;
    public Error(FailedAuthorizerCheck authorizer) => Authorizer = authorizer;
    public Error(InvalidBlockRule invalidBlockRule) => InvalidBlockRule = invalidBlockRule;

    FailedBlockCheck? Block { get; } = null;
    FailedAuthorizerCheck? Authorizer { get; } = null;
    InvalidBlockRule? InvalidBlockRule { get; } = null;
}

public record FailedBlockCheck(uint BlockId, int CheckId/*, RuleExpressions Rule*/);
public record FailedAuthorizerCheck(int CheckId/*, RuleExpressions Rule*/);
//TODO Assuming the int is a RuleId - specification and examples are unclear here
public record InvalidBlockRule(int RuleId/*, RuleExpressions Rule*/);
