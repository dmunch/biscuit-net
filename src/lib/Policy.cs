namespace biscuit_net;
using Datalog;

public record Policy(PolicyKind Kind, IReadOnlyCollection<RuleConstrained> Rules)
{
    static Expressions.Expression True = new Expressions.Expression(new List<Expressions.Op>{ new Expressions.Op(new Boolean(true))});

    public static Policy AllowPolicy = new Policy(PolicyKind.Allow, new [] { new RuleConstrained(new Fact("query"), True) });
    public static Policy DenyPolicy = new Policy(PolicyKind.Deny, new [] { new RuleConstrained(new Fact("query"), True) });
}

public enum PolicyKind
{
    Allow,
    Deny
}
