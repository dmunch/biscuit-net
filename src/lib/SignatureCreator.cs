using NSec.Cryptography;

namespace biscuit_net;

public class SignatureCreator
{
    public record NextKey(byte[] Public, byte[] Private);

    readonly SignatureAlgorithm _algorithm =  SignatureAlgorithm.Ed25519;   
    readonly NSec.Cryptography.Key _key;
    
    public byte[] PublicKey { get; private set; }

    public SignatureCreator()
    {
        _key = NSec.Cryptography.Key.Create(_algorithm);

        PublicKey = _key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
    }
    
    public SignatureCreator(NextKey nextKey)
    {
        _key = NSec.Cryptography.Key.Import(_algorithm, nextKey.Private, KeyBlobFormat.RawPrivateKey);
        
        PublicKey = nextKey.Public;
    }

    public NextKey GetNextKey() 
    {
        var creationParameters = new KeyCreationParameters() 
        {
            ExportPolicy = KeyExportPolicies.AllowPlaintextExport
        };

        var nextKey = NSec.Cryptography.Key.Create(_algorithm, creationParameters);

        return new NextKey(
            nextKey.PublicKey.Export(KeyBlobFormat.RawPublicKey),
            nextKey.Export(KeyBlobFormat.RawPrivateKey)
        );
    }

    public byte[] Sign(ReadOnlySpan<byte> data)
        => _algorithm.Sign(_key, data);
}
