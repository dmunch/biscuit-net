using System;
using System.Collections.Generic;
using System.Linq;

namespace biscuit_net.Datalog;

/// <summary>
/// An Fact (Atom) is an expression p(t_0, t_1, ..., t_n) composed of
/// a predicate name (p) and a finite list of terms (t_0, ..., t_n).
///
/// Examples:
/// parent(Homer, Lisa) -- parent is the predicate and Homer/Lisa are symbols.
/// parent(x, Lisa)     -- x is a variable.
/// </summary>
public sealed record Fact(string Name, IEnumerable<Term> Terms)
{
    public Fact(string name, params Term[] terms) : this(name, new List<Term>(terms))
    {
    }
    
    public int Arity => Terms.Count();

    public Fact Apply(Substitution env) => this with { Terms = Terms.Select(t => t.Apply(env)) };

    public bool Equals(Fact? other) => Name == other?.Name && Terms.SequenceEqual(other.Terms);
    
    public override int GetHashCode() => HashCode.Combine(Name, Terms.Aggregate(0, HashCode.Combine));

    public override string ToString() => $"{Name}({string.Join(", ", Terms.Select(t => t.ToString()))})";
}