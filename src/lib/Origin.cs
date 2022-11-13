
global using Origin = System.UInt32;

namespace biscuit_net;
using Datalog;

public static class Origins
{
    public const Origin Authority = 0;
    public const Origin Authorizer = Origin.MaxValue;
}

public class TrustedOrigin : SortedSet<Origin> 
{
    public TrustedOrigin(Origin idx) : this(new []{ idx })   
    {
    }

    public TrustedOrigin(IEnumerable<Origin> set) : base(set)   
    {
    }

    public TrustedOrigin(params Origin[] origins) : base(origins)   
    {
    }

    public TrustedOrigin(params IEnumerable<Origin>[] sets) : base(sets.SelectMany(s => s))   
    {
    }
}

public class TrustedOrigins
{
    Dictionary<PublicKey, Origin> _publicKeyBlockIdx = new Dictionary<PublicKey, Origin>();
    Dictionary<Origin, TrustedOrigin> _blockIdxTrustedOrigins = new Dictionary<Origin, TrustedOrigin>();

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

    public TrustedOrigin For(Origin blockId, Scope scope)
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

    TrustedOrigin InternalFor(Origin blockId, Scope scope)
    {
        var trustedOrigin = scope.Types.Any(type => type == ScopeType.Previous)
            ? Previous(blockId)
            : new TrustedOrigin(Origins.Authority, blockId, Origins.Authorizer);

        foreach(var key in scope.Keys)
        {
            var trustedBlock = _publicKeyBlockIdx[key];
            trustedOrigin.Add(trustedBlock);
        }

        return trustedOrigin;
    }

    static TrustedOrigin Previous(Origin blockId)
    {
        var trustedOrigin = new TrustedOrigin(Origins.Authority, Origins.Authorizer);
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
            .Where(kvp => origin.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value);
    }
}