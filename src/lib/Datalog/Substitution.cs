using System;
using System.Collections.Generic;
using System.Linq;

namespace biscuit_net.Datalog;

/// <summary>
/// A substitution (or environment) denotes a mapping from variables to terms.
/// </summary>
public class Substitution : Dictionary<Variable, Term>
{
    public Substitution() {}
    public Substitution(Substitution that) : base(that) {}
        
    // The condition v != t is just to avoid inserting
    // redundant mappings like "x -> x".
    public new bool TryAdd(Variable v, Term t) =>
        v != t && base.TryAdd(v, t);
        
    public override bool Equals(object? obj) => 
        obj switch
        {
            Substitution that => this.ToHashSet().SetEquals(that), 
            _ => false
        };

    public override int GetHashCode() => this.Aggregate(0, (acc, p) => HashCode.Combine(acc, p.Key, p.Value));
}