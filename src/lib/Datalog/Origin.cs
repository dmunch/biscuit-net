
namespace biscuit_net.Datalog;

public class Origin : SortedSet<uint>
{
    public static Origin Authority = new Origin(0);
    public static Origin Authorizer = new Origin(uint.MaxValue);

    public Origin(uint idx) : this(new []{ idx })   
    {
    }

    public Origin(IEnumerable<uint> set) : base(set)   
    {
    }

    public static implicit operator Origin(uint value) => new Origin(value);
}


public class TrustedOrigin : Origin 
{
    public TrustedOrigin(uint idx) : this(new []{ idx })   
    {
    }

    public TrustedOrigin(IEnumerable<uint> set) : base(set)   
    {
    }

    public TrustedOrigin(params IEnumerable<uint>[] sets) : base(sets.SelectMany(s => s))   
    {
    }
}

public class AtomSet : OriginSet<HashSet<Atom>, Atom>
{   
    public AtomSet() 
    {
    }

    public AtomSet(Origin origin, HashSet<Atom> intialValue) : base(origin, intialValue)
    {
    }
}

public class RuleSet : OriginSet<List<(int, RuleExpressions)>, (int, RuleExpressions)>
{
    public RuleSet() 
    {
    }

    public RuleSet(Origin origin, List<(int, RuleExpressions)> intialValue) : base(origin, intialValue)
    {
    }
}

public class OriginSet<T, I> where T : ICollection<I>
{
    Dictionary<Origin, T> _values = new Dictionary<Origin, T>();

    public OriginSet()
    {
    }

    public OriginSet(Origin origin, T intialValue)
    {
        Add(origin, intialValue);
    }

    public void Add(Origin origin, T value)
    {
        _values.Add(origin, value);
    }

    public void Add(Origin origin, ICollection<T> values)
    {
        foreach(var value in values)
        {
            _values.Add(origin, value);
        }
    }

    public void Merge(OriginSet<T, I> other)
    {
        foreach(var kvp in other._values)
        {
            Merge(kvp.Key, kvp.Value);
        }
    }

    public void Merge(Origin origin, T other)
    {
        if(_values.TryGetValue(origin, out var set))
        {
            foreach(var item in other)
            {
                set.Add(item);
            }
        }
        else
        {
            _values.Add(origin, other);
        }
    }

    public IEnumerable<I> Filter(TrustedOrigin origin)
    {
        return _values
            .Where(kvp => origin.IsProperSupersetOf(kvp.Key))
            .SelectMany(kvp => kvp.Value);
    }
}