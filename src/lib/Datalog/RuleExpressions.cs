namespace biscuit_net.Datalog;
using Expressions;

public record RuleExpressions(
        Fact Head, 
        IEnumerable<Fact> Body, 
        IEnumerable<Expression> Expressions) 
    : Rule(Head, Body)
    {
        public RuleExpressions(Fact head, params Fact[] body) : this(head, body.AsEnumerable(), Enumerable.Empty<Expression>()) {}
        public virtual bool Equals(RuleExpressions? other) => base.Equals(other) && Expressions.SequenceEqual(other.Expressions);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expressions.Aggregate(0, HashCode.Combine));
    }
