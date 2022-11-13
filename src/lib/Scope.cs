namespace biscuit_net;

public record Scope(IEnumerable<ScopeType> Types, IEnumerable<PublicKey> Keys)
{
    public virtual bool Equals(Scope? other) => other != null && Types.SequenceEqual(other.Types) && Keys.SequenceEqual(other.Keys);
    
    public override int GetHashCode() => HashCode.Combine(Types, Keys);

    public bool IsEmpty => !Types.Any() && !Keys.Any();
    public static Scope DefaultRuleScope = new Scope(Enumerable.Empty<ScopeType>(), Enumerable.Empty<PublicKey>());
    public static Scope DefaultBlockScope = new Scope(new [] { ScopeType.Authority }, Enumerable.Empty<PublicKey>());
}

public enum ScopeType
{
    Authority = 0,
    Previous = 1,
}