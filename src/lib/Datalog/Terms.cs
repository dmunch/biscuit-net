namespace biscuit_net.Datalog;

public abstract record Term
{
    public abstract Term Apply(Substitution env);
}

public sealed record Variable(string Name) : Term
{
    public override Term Apply(Substitution env) => env.GetValueOrDefault(this, this);
    public override string ToString() => $"<{Name}>";
}

public sealed record Symbol(string Name) : Term
{
    public override Term Apply(Substitution env) => this;
    public override string ToString() => Name;
}

public record Constant : Term
{
    public override Term Apply(Substitution env) => this;
}