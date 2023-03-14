using NSec.Cryptography;

namespace biscuit_net;

public class VerificationKey
{
    SignatureAlgorithm _algorithm;
    NSec.Cryptography.PublicKey _key;
    
    public PublicKey PublicKey => new PublicKey(Algorithm.Ed25519, _publicKey);
    readonly byte[] _publicKey;

    public VerificationKey(byte[] key) : this(new PublicKey(Algorithm.Ed25519, key))
    {
    }

    public VerificationKey(Proto.PublicKey publicKey)
    {
        _algorithm = SignatureAlgorithm.Ed25519;
        _publicKey = publicKey.Key;
        _key = NSec.Cryptography.PublicKey.Import(_algorithm, publicKey.Key, KeyBlobFormat.RawPublicKey);
    }


    public VerificationKey(PublicKey publicKey)
    {
        _algorithm = SignatureAlgorithm.Ed25519;
        _publicKey = publicKey.Key;
        _key = NSec.Cryptography.PublicKey.Import(_algorithm, publicKey.Key, KeyBlobFormat.RawPublicKey);
    }

    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        => _algorithm.Verify(_key, data, signature);

    public static VerificationKey FromHexKey(string publicKeyInHex)
    {
        var key = Convert.FromHexString(publicKeyInHex);
        return new VerificationKey(key);
    }
}
