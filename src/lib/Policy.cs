namespace biscuit_net;
using Datalog;

public record Policy(PolicyKind Kind, IReadOnlyCollection<Rule> Rules)
{
    readonly static Expressions.Expression True = new(new List<Expressions.Op>{ new Expressions.Op(new Boolean(true))});

    public readonly static Policy AllowPolicy = new(PolicyKind.Allow, new [] { new Rule(new Fact("query"), True) });
    public readonly static Policy DenyPolicy = new(PolicyKind.Deny, new [] { new Rule(new Fact("query"), True) });
}

public enum PolicyKind
{
    Allow,
    Deny
}
