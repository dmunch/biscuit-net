using System;
using System.Collections.Generic;
using System.Linq;

namespace biscuit_net;
using Datalog;
using Expressions;

public record Rule(Fact Head, IEnumerable<Fact> Body) : IRule
{
    public Rule(Fact head, params Fact[] body) : this(head, body.AsEnumerable()) {}

    public virtual bool Equals(Rule? other) => Head == other?.Head && Body.SequenceEqual(other.Body);
    
    public override int GetHashCode() => HashCode.Combine(Head, Body.Aggregate(0, HashCode.Combine));
}


public record RuleConstrained(
        Fact Head, 
        IEnumerable<Fact> Body, 
        IEnumerable<Expression> Constraints) 
    : Rule(Head, Body), IRuleConstrained
    {
        public RuleConstrained(Fact head, params Fact[] body) : this(head, body.AsEnumerable(), Enumerable.Empty<Expression>()) {}
        public virtual bool Equals(RuleConstrained? other) => base.Equals(other) && Constraints.SequenceEqual(other.Constraints);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Constraints.Aggregate(0, HashCode.Combine));
    }
