using System;
using System.Collections.Generic;
using System.Linq;

namespace biscuit_net.Datalog;

/// <summary>
/// A rule (or [Horn] clause) is an expression A_0 :- A_1, ..., A_n
/// composed of a head (A_0) and a body (A_1, ..., A_n), where A_i are Facts.
/// A rule without body is called a fact.
/// Examples:
/// parent(Homer,Lisa) -- a fact expressing that Homer is Lisa's parent
/// ancestor(x,z):-ancestor(x,y),parent(y,z) -- a rule for deducing ancestors from parents
/// </summary>
public interface IRule
{
    Fact Head { get; }
    IEnumerable<Fact> Body { get; }
}

public interface IRuleConstrained : IRule
{
    IEnumerable<Expressions.Expression> Constraints { get; }
}