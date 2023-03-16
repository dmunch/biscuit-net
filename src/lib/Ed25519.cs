using NSec.Cryptography;

namespace biscuit_net;

public class Ed25519 : ICryptoProvider
{
    public Ed25519() {}
    
    public IEphemeralSigningKey CreateEphemeral()
    {
        return new EphemeralSigningKey();
    }

    public IEphemeralSigningKey CreateEphemeral(byte[] secretKey)
    {
        return new EphemeralSigningKey(secretKey);
    }

    public IVerificationKey CreateVerification(Proto.PublicKey publicKey)
    {
        return new VerificationKey(publicKey);
    }

    public static ISigningKey NewSigningKey()
    {
        return new SigningKey();
    }
    
    class SigningKey : ISigningKey
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

        public ICryptoProvider CreateProvider() => new Ed25519();
    }

    class EphemeralSigningKey : IEphemeralSigningKey
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

        public ICryptoProvider CreateProvider() => new Ed25519();
    }

    public class VerificationKey : IVerificationKey
    {
        readonly SignatureAlgorithm _algorithm;
        readonly NSec.Cryptography.PublicKey _key;
        
        public PublicKey PublicKey => new(Algorithm.Ed25519, _publicKey);
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
    }
}