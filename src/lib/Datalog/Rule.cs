using System;
using System.Collections.Generic;
using System.Linq;

namespace biscuit_net.Datalog;

/// <summary>
/// A rule (or [Horn] clause) is an expression A_0 :- A_1, ..., A_n
/// composed of a head (A_0) and a body (A_1, ..., A_n), where A_i are atoms.
/// A rule without body is called a fact.
/// Examples:
/// parent(Homer,Lisa) -- a fact expressing that Homer is Lisa's parent
/// ancestor(x,z):-ancestor(x,y),parent(y,z) -- a rule for deducing ancestors from parents
/// </summary>
public record Rule(Atom Head, IEnumerable<Atom> Body)
{
    public Rule(Atom head, params Atom[] body) : this(head, body.AsEnumerable()) {}

    public virtual bool Equals(Rule? other) => Head == other?.Head && Body.SequenceEqual(other.Body);
    
    public override int GetHashCode() => HashCode.Combine(Head, Body.Aggregate(0, HashCode.Combine));

}