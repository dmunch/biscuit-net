using NSec.Cryptography;

namespace biscuit_net.Builder;

public interface ISigningKey 
{
    byte[] Sign(ReadOnlySpan<byte> data);
    PublicKey Public { get; }
}

public class SigningKey : ISigningKey
{
    readonly SignatureAlgorithm _algorithm =  SignatureAlgorithm.Ed25519;   
    readonly NSec.Cryptography.Key _key;
    public PublicKey Public => new(Algorithm.Ed25519, _key.PublicKey.Export(KeyBlobFormat.RawPublicKey));
    public SigningKey()
    {
        _key = NSec.Cryptography.Key.Create(_algorithm);        
    }

    public byte[] Sign(ReadOnlySpan<byte> data)
        => _algorithm.Sign(_key, data);
}

public class EphemeralSigningKey : ISigningKey
{
    readonly SignatureAlgorithm _algorithm =  SignatureAlgorithm.Ed25519;   
    readonly NSec.Cryptography.Key _key;
    
    public byte[] Private => _key.Export(KeyBlobFormat.RawPrivateKey);
    public PublicKey Public => new(Algorithm.Ed25519, _key.PublicKey.Export(KeyBlobFormat.RawPublicKey));

    public EphemeralSigningKey()
    {
        var creationParameters = new KeyCreationParameters() 
        {
            ExportPolicy = KeyExportPolicies.AllowPlaintextExport
        };

        _key = NSec.Cryptography.Key.Create(_algorithm, creationParameters);
    }

    public EphemeralSigningKey(byte[] secretKey)
    {
        _key = NSec.Cryptography.Key.Import(_algorithm, secretKey, KeyBlobFormat.RawPrivateKey);        
    }


    public byte[] Sign(ReadOnlySpan<byte> data)
        => _algorithm.Sign(_key, data);
}