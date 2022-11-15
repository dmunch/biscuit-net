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
        IEnumerable<Expression> Constraints,
        Scope Scope)
    : Rule(Head, Body), IRuleConstrained
    {
        public RuleConstrained(Fact head, params Fact[] body) : this(head, body.AsEnumerable(), Enumerable.Empty<Expression>(), Scope.DefaultRuleScope) {}
        public RuleConstrained(Fact head, params Expression[] expressions) : this(head, Enumerable.Empty<Fact>(), expressions, Scope.DefaultRuleScope) {}
        public virtual bool Equals(RuleConstrained? other) => base.Equals(other) && Constraints.SequenceEqual(other.Constraints);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Constraints.Aggregate(0, HashCode.Combine));
    }
