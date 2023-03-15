
global using Origin = System.UInt32;

namespace biscuit_net.Datalog;

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

public class BlockTrustedOriginSet
{
    public Origin BlockId { get; }
    public TrustedOriginSet OriginSet { get; }

    public BlockTrustedOriginSet(Origin blockId, TrustedOriginSet originSet) 
    {
        BlockId = blockId;
        OriginSet = originSet;
    }

    public TrustedOrigins Origins() => OriginSet.Origins(BlockId);
    public TrustedOrigins Origins(Scope scope) => OriginSet.Origins(BlockId, scope);
}


public class TrustedOriginSet
{
    ILookup<PublicKey, Origin> _publicKeys;
    IReadOnlyDictionary<Origin, TrustedOrigins> _trustedOrigins;

    TrustedOriginSet(ILookup<PublicKey, Origin> publicKeys,  IReadOnlyDictionary<Origin, TrustedOrigins> blockTrustedOrigins)
    {
        _publicKeys = publicKeys;
        _trustedOrigins = blockTrustedOrigins;
    }

    public static TrustedOriginSet Build(Block authority, IEnumerable<Block> blocks, Scope scope)
    {
        var trustedOrigins = new Dictionary<Origin, TrustedOrigins>();

        var publicKeys = blocks
            .Select((block, idx) => new {block, idx = idx + 1})
            .Where(b => b.block.SignedBy is not null)
            .ToLookup(b => b.block.SignedBy!, b => (uint) b.idx);

        //populate block scopes 
        trustedOrigins[Datalog.Origins.Authority] = Origins(Datalog.Origins.Authority, authority.Scope, publicKeys);

        uint blockId = 1;
        foreach(var block in blocks)
        {
            trustedOrigins[blockId] = Origins(blockId, block.Scope, publicKeys);
            blockId++;
        }
        trustedOrigins[Datalog.Origins.Authorizer] = Origins(Datalog.Origins.Authorizer, scope, publicKeys);

        return new TrustedOriginSet(publicKeys, trustedOrigins);
    }

    public BlockTrustedOriginSet With(Origin blockId) => new BlockTrustedOriginSet(blockId, this);

    public TrustedOrigins Origins(Origin blockId)
    {
        if(!_trustedOrigins.TryGetValue(blockId, out var trustedOrigin))
        {
            throw new ArgumentException("Unknown blockId", nameof(blockId));
        }

        return trustedOrigin;
    }

    public TrustedOrigins Origins(Origin blockId, Scope scope)
    {
        var trustedOrigin = Origins(blockId);

        if(scope.IsEmpty)
        {
            //A rule scope can be empty, in that case it returns the default 
            //block scope
            return trustedOrigin;
        } 
        else
        {
            //a non-empty rule-scope overwrites the block scope
            return Origins(blockId, scope, _publicKeys);
        }
    }

    static TrustedOrigins Origins(Origin blockId, Scope scope, ILookup<PublicKey, Origin> publicKeys)
    {
        
        var trustedOrigin = scope.Types.Any(type => type == ScopeType.Previous)
            ? Previous(blockId)
            : new TrustedOrigins(blockId, Datalog.Origins.Authorizer);
        

        if(scope.Types.Any(type => type == ScopeType.Authority))
        {
            trustedOrigin.Add(Datalog.Origins.Authority);
        }

        foreach(var key in scope.Keys)
        {
            var trustedBlocks = publicKeys[key];

            foreach(var trustedBlock in trustedBlocks)
            {
                trustedOrigin.Add(trustedBlock);
            }
        }

        return trustedOrigin;
    }

    static TrustedOrigins Previous(Origin blockId)
    {
        var trustedOrigin = new TrustedOrigins(Datalog.Origins.Authority, Datalog.Origins.Authorizer);
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

    public void UnionWith(Origin origin, HashSet<Fact> other)
    {
        Get(origin).UnionWith(other);
    }
}

public class RuleSet : OriginSet<List<(int, Rule)>, (int, Rule)>
{
    public RuleSet() 
    {
    }

    public RuleSet(Origin origin, List<(int, Rule)> intialValue) : base(origin, intialValue)
    {
    }
}

public class OriginSet<T, I> where T : ICollection<I>
{    
    public IEnumerable<I> Values => _dict.Values.SelectMany(v => v);

    Dictionary<Origin, T> _dict = new Dictionary<Origin, T>();

    public OriginSet()
    {
    }

    public OriginSet(Origin origin, T intialValue)
    {
        Add(origin, intialValue);
    }

    public void Add(Origin origin, T value)
    {
        _dict.Add(origin, value);
    }

    public void Add(Origin origin, ICollection<T> values)
    {
        foreach(var value in values)
        {
            _dict.Add(origin, value);
        }
    }

    protected T Get(Origin origin) => _dict[origin];

    public IEnumerable<I> Filter(TrustedOrigins origin)
    {
        return _dict
            .Where(kvp => origin.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value);
    }
}