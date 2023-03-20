namespace biscuit_net.Datalog;
using Expressions;

/// <summary>
/// A rule (or [Horn] clause) is an expression A_0 :- A_1, ..., A_n
/// composed of a head (A_0) and a body (A_1, ..., A_n), where A_i are Facts.
/// A rule without body is called a fact.
/// Examples:
/// parent(Homer,Lisa) -- a fact expressing that Homer is Lisa's parent
/// ancestor(x,z):-ancestor(x,y),parent(y,z) -- a rule for deducing ancestors from parents
/// </summary>

public record Rule(
        Fact Head, 
        IEnumerable<Fact> Body, 
        IEnumerable<Expression> Constraints)
    {
        public Rule(Fact head, params Fact[] body) : this(head, body.AsEnumerable(), Enumerable.Empty<Expression>()) {}
        public Rule(Fact head, params Expression[] expressions) : this(head, Enumerable.Empty<Fact>(), expressions) {}
        public virtual bool Equals(Rule? other) => Head == other?.Head && Body.SequenceEqual(other.Body) && Constraints.SequenceEqual(other.Constraints);
        public override int GetHashCode() => HashCode.Combine(Head, Body.Aggregate(0, HashCode.Combine), Constraints.Aggregate(0, HashCode.Combine));
    }
