namespace biscuit_net;
using Datalog;

public record Policy(PolicyKind Kind, IReadOnlyCollection<Rule> Rules)
{
    static Expressions.Expression True = new Expressions.Expression(new List<Expressions.Op>{ new Expressions.Op(new Boolean(true))});

    public static Policy AllowPolicy = new Policy(PolicyKind.Allow, new [] { new Rule(new Fact("query"), True) });
    public static Policy DenyPolicy = new Policy(PolicyKind.Deny, new [] { new Rule(new Fact("query"), True) });
}

public enum PolicyKind
{
    Allow,
    Deny
}
