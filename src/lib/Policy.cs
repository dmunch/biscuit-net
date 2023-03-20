namespace biscuit_net;
using Datalog;

public record Policy(PolicyKind Kind, IReadOnlyCollection<RuleScoped> Rules)
{
    readonly static Expressions.Expression True = new(new List<Expressions.Op>{ new Expressions.Op(new Boolean(true))});
    readonly static RuleScoped TrueRule = new(new Fact("query"), Enumerable.Empty<Fact>(), new []{ True }, Scope.DefaultRuleScope);

    public readonly static Policy AllowPolicy = new(PolicyKind.Allow, new [] { TrueRule });
    public readonly static Policy DenyPolicy = new(PolicyKind.Deny, new [] { TrueRule });
}

public enum PolicyKind
{
    Allow,
    Deny
}
