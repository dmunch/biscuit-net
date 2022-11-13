
namespace biscuit_net;
using Datalog;

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

public class TrustedOrigins
{
    Dictionary<PublicKey, uint> _publicKeyBlockIdx = new Dictionary<PublicKey, uint>();
    Dictionary<uint, TrustedOrigin> _blockIdxTrustedOrigins = new Dictionary<uint, TrustedOrigin>();

    public static TrustedOrigins Build(IBiscuit b)
    {
        var trustedOrigins = new TrustedOrigins();

        uint blockId = 1;
        foreach(var block in b.Blocks)
        {
            if(block.SignedBy != null)
            {
                trustedOrigins._publicKeyBlockIdx[block.SignedBy] = blockId++;
            }
        }

        return trustedOrigins;
    }

    public TrustedOrigin For(uint blockId, Scope scope)
    {
        if(_blockIdxTrustedOrigins.TryGetValue(blockId, out var trustedOrigin))
        {
            if(scope.IsEmpty)
            {
                //A rule scope can be empty, in that case it returns the default 
                //block scope
                return trustedOrigin;
            } 
            else
            {
                //a non-empty rule-scope overwrites the block scope
                return InternalFor(blockId, scope);
            }
        }

        if(scope.IsEmpty)
        {
            throw new ArgumentException("Need non-empty scope when building trusted origin for the first time", nameof(scope)); 
        }

        
        trustedOrigin = InternalFor(blockId, scope);
        _blockIdxTrustedOrigins.Add(blockId, trustedOrigin);
        return trustedOrigin;
    }

    TrustedOrigin InternalFor(uint blockId, Scope scope)
    {
        var trustedOrigin = scope.Types.Any(type => type == ScopeType.Previous)
            ? Previous(blockId)
            : new TrustedOrigin(Origin.Authority, (Origin) blockId, Origin.Authorizer);

        foreach(var key in scope.Keys)
        {
            var trustedBlock = _publicKeyBlockIdx[key];
            trustedOrigin.Add(trustedBlock);
        }

        return trustedOrigin;
    }

    static TrustedOrigin Previous(uint blockId)
    {
        var trustedOrigin = new TrustedOrigin(Origin.Authority, Origin.Authorizer);
        for(uint blockIdx = 1; blockIdx <= blockId; blockIdx++)
        {
            trustedOrigin.Add(blockIdx);
        }

        return trustedOrigin;
    }
}

public class FactSet : OriginSet<HashSet<Fact>, Fact>
{   
    public FactSet() 
    {
    }

    public FactSet(Origin origin, HashSet<Fact> intialValue) : base(origin, intialValue)
    {
    }
}

public class RuleSet : OriginSet<List<(int, IRuleConstrained)>, (int, IRuleConstrained)>
{
    public RuleSet() 
    {
    }

    public RuleSet(Origin origin, List<(int, IRuleConstrained)> intialValue) : base(origin, intialValue)
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