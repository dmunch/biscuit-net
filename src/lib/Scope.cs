namespace biscuit_net;

using biscuit_net.Expressions;
using Datalog;

public record RuleScoped(Fact Head, 
        IEnumerable<Fact> Body, 
        IEnumerable<Expression> Constraints,
        Scope Scope) : Rule(Head, Body, Constraints)
{    
    public RuleScoped(Fact head, params Fact[] body) : this(head, body.AsEnumerable(), Enumerable.Empty<Expression>(), Scope.DefaultRuleScope) {}    

    public virtual bool Equals(RuleScoped? other) => base.Equals(other) && Scope == other.Scope;
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Scope.GetHashCode());
}

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