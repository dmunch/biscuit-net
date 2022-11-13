
global using Origin = System.UInt32;

namespace biscuit_net;
using Datalog;

public static class Origins
{
    public const Origin Authority = 0;
    public const Origin Authorizer = Origin.MaxValue;
}

public class TrustedOrigins : SortedSet<Origin> 
{
    public TrustedOrigins(params Origin[] origins) : base(origins)   
    {
    }
}

public class TrustedOriginSet
{
    IReadOnlyDictionary<PublicKey, Origin> _publicKeys;
    IReadOnlyDictionary<Origin, TrustedOrigins> _trustedOrigins;

    TrustedOriginSet(IReadOnlyDictionary<PublicKey, Origin> publicKeys,  IReadOnlyDictionary<Origin, TrustedOrigins> blockTrustedOrigins)
    {
        _publicKeys = publicKeys;
        _trustedOrigins = blockTrustedOrigins;
    }

    public static TrustedOriginSet Build(IBiscuit b, IBlock authorizerBlock)
    {
        var publicKeys = new Dictionary<PublicKey, Origin>();  
        var trustedOrigins = new Dictionary<Origin, TrustedOrigins>();

        uint blockId = 1;
        //populate public key lookup table
        foreach(var block in b.Blocks)
        {
            if(block.SignedBy != null)
            {
                publicKeys[block.SignedBy] = blockId++;
            }
        }

        //populate block scopes 
        trustedOrigins[Origins.Authority] = For(Origins.Authority, b.Authority.Scope, publicKeys);
        blockId = 1;
        foreach(var block in b.Blocks)
        {
            trustedOrigins[blockId] = For(blockId, block.Scope, publicKeys);
            blockId++;
        }
        trustedOrigins[Origins.Authorizer] = For(Origins.Authorizer, authorizerBlock.Scope, publicKeys);

        return new TrustedOriginSet(publicKeys, trustedOrigins);
    }

    public TrustedOrigins For(Origin blockId, Scope scope)
    {
        if(!_trustedOrigins.TryGetValue(blockId, out var trustedOrigin))
        {
            throw new ArgumentException("Unknown blockId", nameof(blockId));
        }

        if(scope.IsEmpty)
        {
            //A rule scope can be empty, in that case it returns the default 
            //block scope
            return trustedOrigin;
        } 
        else
        {
            //a non-empty rule-scope overwrites the block scope
            return For(blockId, scope, _publicKeys);
        }
    }

    static TrustedOrigins For(Origin blockId, Scope scope, IReadOnlyDictionary<PublicKey, Origin> publicKeys)
    {
        var trustedOrigin = scope.Types.Any(type => type == ScopeType.Previous)
            ? Previous(blockId)
            : new TrustedOrigins(Origins.Authority, blockId, Origins.Authorizer);

        foreach(var key in scope.Keys)
        {
            var trustedBlock = publicKeys[key];
            trustedOrigin.Add(trustedBlock);
        }

        return trustedOrigin;
    }

    static TrustedOrigins Previous(Origin blockId)
    {
        var trustedOrigin = new TrustedOrigins(Origins.Authority, Origins.Authorizer);
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

    public IEnumerable<I> Filter(TrustedOrigins origin)
    {
        return _values
            .Where(kvp => origin.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value);
    }
}