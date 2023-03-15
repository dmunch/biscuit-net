namespace biscuit_net.Datalog;

public record Scope(IEnumerable<ScopeType> Types, IEnumerable<PublicKey> Keys)
{
    public virtual bool Equals(Scope? other) => other != null && Types.SequenceEqual(other.Types) && Keys.SequenceEqual(other.Keys);
    
    public override int GetHashCode() => HashCode.Combine(Types, Keys);

    public bool IsEmpty => !Types.Any() && !Keys.Any();
    public readonly static Scope DefaultRuleScope = new(Enumerable.Empty<ScopeType>(), Enumerable.Empty<PublicKey>());
    public readonly static Scope DefaultBlockScope = new(new [] { ScopeType.Authority }, Enumerable.Empty<PublicKey>());
}

public enum ScopeType
{
    Authority = 0,
    Previous = 1,
}