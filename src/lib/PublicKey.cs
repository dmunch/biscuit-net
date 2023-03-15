namespace biscuit_net;

public record PublicKey(Algorithm Algorithm, byte[] Key)
{
    public virtual bool Equals(PublicKey? other) => Algorithm == other?.Algorithm && Key.SequenceEqual(other.Key);
    
    public override int GetHashCode() 
    {
        var hashCode = new HashCode();
        hashCode.Add(Algorithm);
        hashCode.AddBytes(Key);
        return hashCode.ToHashCode();
    }
}

public enum Algorithm
{
    Ed25519 = 0
}

public class KeyTable
{
    List<PublicKey> _keys = new List<PublicKey>();
    public IReadOnlyList<PublicKey> Keys { get => _keys.AsReadOnly(); }
    public KeyTable()
    {
    }

    public KeyTable(IEnumerable<PublicKey> initialKeys)
    {
        Add(initialKeys);
    }

    public void Add(IEnumerable<PublicKey> keys)
    {
        foreach(var key in keys)
        {
            if(!_keys.Contains(key))
            {
                _keys.Add(key);
            }            
        }
    }

    public PublicKey Lookup(long pos)
    {
        return _keys[(int)pos];
    }

     public uint LookupOrAdd(PublicKey key)
    {        
        if(_keys.Contains(key))
        {
            return (uint) _keys.IndexOf(key);
        }

        //we add the key
        _keys.Add(key);
        return (uint) _keys.Count - 1;
    }
}