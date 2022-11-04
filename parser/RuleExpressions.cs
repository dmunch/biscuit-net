using VeryNaiveDatalog;

namespace parser;


public record Expression(List<Op> Ops);

public record RuleExpressions(
        Atom Head, 
        IEnumerable<Atom> Body, 
        IEnumerable<Expression> Expressions) 
    : Rule(Head, Body)
    {
        public virtual bool Equals(RuleExpressions? other) => base.Equals(other) && Expressions.SequenceEqual(other.Expressions);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Expressions.Aggregate(0, HashCode.Combine));
    }

public record Check(IEnumerable<RuleExpressions> Rules);
