namespace biscuit_net.Datalog;

public abstract record Term
{
    public abstract Term Apply(Substitution env);

    public static implicit operator Term(string s) => 
        s.StartsWith('$') 
            ? new Variable(s.TrimStart('$')) 
            : new Symbol(s);
    public static implicit operator Term(bool b) => new Boolean(b);
    public static implicit operator Term(long b) => new Integer(b);
    public static implicit operator Term(byte[] b) => new Bytes(b);
    public static implicit operator Term(DateTime d) => new Date(Date.ToTAI64(d));
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