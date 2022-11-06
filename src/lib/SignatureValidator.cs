using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using NSec.Cryptography;

namespace biscuit_net;

public record FailedFormat(Signature? Signature, int? InvalidSignatureSize);
public record Signature(string InvalidSignature);

public class SignatureValidator
{
    SignatureAlgorithm _algorithm;
    PublicKey _key;
    
    public SignatureValidator(string publicKeyInHex) : this(Convert.FromHexString(publicKeyInHex))
    {
    }
    
    public SignatureValidator(byte[] publicKey)
    {
        _algorithm = SignatureAlgorithm.Ed25519;
        _key = PublicKey.Import(_algorithm, publicKey, KeyBlobFormat.RawPublicKey);
    }

    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        => _algorithm.Verify(_key, data, signature);

}

static class BlockSignatureVerification
{
    static bool VerifySignature(this Proto.SignedBlock signedBlock, SignatureValidator validator, [NotNullWhen(false)] out int? invalidSignatureSize)
    {
        if(signedBlock.Signature.Length != 64)
        {
            invalidSignatureSize = signedBlock.Signature.Length;
            return false;
        }
        invalidSignatureSize = null;

        //IMPROVE: could use an array pool here
        var buffer = new byte[signedBlock.Block.Length + sizeof(int) + signedBlock.nextKey.Key.Length];
        var bytes = (Span<byte>) buffer;        

        signedBlock.Block.CopyTo(buffer, 0);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(signedBlock.Block.Length, sizeof(int)), (int)signedBlock.nextKey.algorithm);
        signedBlock.nextKey.Key.CopyTo(buffer, signedBlock.Block.Length + 4);

        return validator.Verify(buffer, signedBlock.Signature);
    }

    static bool VerifySignatures(this Proto.Biscuit biscuitProto, SignatureValidator validator, [NotNullWhen(false)] out int? invalidSignatureSize)
    {
        if(!biscuitProto.Authority.VerifySignature(validator, out invalidSignatureSize))
        {
            return false;
        }

        var nextValidator = new SignatureValidator(biscuitProto.Authority.nextKey.Key);
        foreach(var block in biscuitProto.Blocks)
        {
            if(!block.VerifySignature(nextValidator, out invalidSignatureSize))
            {
                return false;
            }
            nextValidator = new SignatureValidator(block.nextKey.Key);
        }

        return true;
    }

    public static bool VerifySignatures(this Proto.Biscuit biscuitProto, SignatureValidator validator, [NotNullWhen(false)] out FailedFormat? err)
    {
        if(!biscuitProto.VerifySignatures(validator, out int? invalidSignatureSize))
        {
            err = invalidSignatureSize != null 
                ? new FailedFormat(null, invalidSignatureSize)
                : new FailedFormat(new Signature("signature error: Verification equation was not satisfied"), null);
            return false; 
        }
        err = null; return true;
    }
}