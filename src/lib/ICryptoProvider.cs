namespace biscuit_net;

public interface ICryptoProvider
{    
    IEphemeralSigningKey CreateEphemeral();
    IEphemeralSigningKey CreateEphemeral(byte[] secretKey);
    IVerificationKey CreateVerification(Proto.PublicKey publicKey);

    static ICryptoProvider Create(Algorithm algorithm)
    {
        if(algorithm != Algorithm.Ed25519) 
        {
            throw new ArgumentException($"Unknown algorithm {algorithm}", nameof(algorithm));
        }
        
        return new Ed25519();
    }
    
    static IVerificationKey CreateVerificationKey(Proto.PublicKey publicKey)
    {
        return new Ed25519.VerificationKey(publicKey);
    }
}

public interface ISigningKey 
{
    byte[] Sign(ReadOnlySpan<byte> data);
    PublicKey Public { get; }

    ICryptoProvider CreateProvider();
}

public interface IEphemeralSigningKey  : ISigningKey
{    
    public byte[] Private { get; }
}

public interface IVerificationKey
{
    bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature);
    PublicKey PublicKey { get; }
}