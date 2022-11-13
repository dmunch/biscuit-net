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
    
    //=> HashCode.Combine(Algorithm, ComputeHash(Key));

    public static int ComputeHash(byte[] data)
    {
        unchecked
        {
            const int p = 16777619;
            int hash = (int)2166136261;

            for (int i = 0; i < data.Length; i++)
                hash = (hash ^ data[i]) * p;

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }
    }
}

public enum Algorithm
{
    Ed25519 = 0
}

public class KeyTable
{
    List<PublicKey> _keys;
    
    public KeyTable()
    {
        _keys = new List<PublicKey>();
    }

    public KeyTable(IEnumerable<PublicKey> initialKeys)
    {
        _keys = initialKeys.ToList();
    }

    public void Add(IEnumerable<PublicKey> keys)
    {
        _keys.AddRange(keys);
    }

    public PublicKey Lookup(long pos)
    {
        return _keys[(int)pos];
    }
}