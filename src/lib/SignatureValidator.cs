using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using NSec.Cryptography;

namespace biscuit_net;

public record FailedFormat(Signature? Signature, int? InvalidSignatureSize);
public record Signature(string InvalidSignature);

public class SignatureValidator
{
    SignatureAlgorithm _algorithm;
    NSec.Cryptography.PublicKey _key;

    public byte[] Key { get; private set; }
    
    public SignatureValidator(string publicKeyInHex) : this(Convert.FromHexString(publicKeyInHex))
    {
    }
    
    public SignatureValidator(byte[] publicKey)
    {
        _algorithm = SignatureAlgorithm.Ed25519;
        Key = publicKey;
        _key = NSec.Cryptography.PublicKey.Import(_algorithm, publicKey, KeyBlobFormat.RawPublicKey);
    }

    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        => _algorithm.Verify(_key, data, signature);

}

static class SignatureHelper
{
    public static byte[] MakeBuffer(byte[] block, Proto.PublicKey.Algorithm alg, byte[] key, byte[]? additionalBytes = null)
    {
        var buffer = new byte[block.Length + sizeof(int) + key.Length + (additionalBytes?.Length ?? 0)];
        var bytes = (Span<byte>) buffer;
        
        block.CopyTo(buffer, 0);

        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(block.Length, sizeof(int)), (int)alg);
        key.CopyTo(buffer, block.Length + sizeof(int));

        if(additionalBytes != null)
        {
            additionalBytes.CopyTo(buffer, block.Length + sizeof(int) + key.Length);
        }
        
        return buffer;
    }

    public static byte[] MakeBuffer(byte[] block, byte[] externalSignature, Proto.PublicKey.Algorithm alg, byte[] key)
    {
        var buffer = new byte[block.Length + externalSignature.Length + sizeof(int) + key.Length];
        var bytes = (Span<byte>) buffer;
        
        block.CopyTo(buffer, 0);
        externalSignature.CopyTo(buffer, block.Length);

        var keyPosition = block.Length + externalSignature.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(keyPosition, sizeof(int)), (int)alg);
        key.CopyTo(buffer, keyPosition + sizeof(int));
        
        return buffer;
    }

    static bool VerifySignature(this Proto.SignedBlock signedBlock, SignatureValidator validator, [NotNullWhen(false)] out int? invalidSignatureSize)
    {
        if(signedBlock.Signature.Length != 64)
        {
            invalidSignatureSize = signedBlock.Signature.Length;
            return false;
        }
        invalidSignatureSize = null;

        //IMPROVE: could use an array pool here

        var buffer = signedBlock.externalSignature != null 
            ? MakeBuffer(signedBlock.Block, signedBlock.externalSignature.Signature, signedBlock.nextKey.algorithm, signedBlock.nextKey.Key)
            : MakeBuffer(signedBlock.Block,signedBlock.nextKey.algorithm, signedBlock.nextKey.Key);

        if(signedBlock.externalSignature == null)
        {
            return validator.Verify(buffer, signedBlock.Signature);
        }
        
        var externalBuffer = MakeBuffer(signedBlock.Block, Proto.PublicKey.Algorithm.Ed25519, validator.Key);
        var externalValidator = new SignatureValidator(signedBlock.externalSignature.publicKey.Key);

        return    validator.Verify(buffer, signedBlock.Signature)
               && externalValidator.Verify(externalBuffer, signedBlock.externalSignature.Signature);

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

    static bool VerifyProof(Proto.Biscuit biscuitProto)
    {
        //verify proof 
        var publicKey = (biscuitProto.Blocks.LastOrDefault() ?? biscuitProto.Authority).nextKey.Key;
        var validator = new SignatureValidator(publicKey);
        
        if(biscuitProto.Proof.finalSignature != null)
        {
            //token is sealed                
            var buffer = MakeFinalSignatureBuffer(biscuitProto);
            
            return validator.Verify(buffer, biscuitProto.Proof.finalSignature);            
        } 
        else if(biscuitProto.Proof.nextSecret != null) 
        {
            var key = NSec.Cryptography.Key.Import(SignatureAlgorithm.Ed25519, biscuitProto.Proof.nextSecret, KeyBlobFormat.RawPrivateKey);
            return validator.Key.SequenceEqual(key.PublicKey.Export(KeyBlobFormat.RawPublicKey));
        } 

        //proof field has been tampered with          
        return false;        
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

        if(!VerifyProof(biscuitProto))
        {
            err =  new FailedFormat(new Signature("signature error: Proof verification equation was not satisfied"), null);
            return false;
        }

        err = null; return true;
    }

    public static ReadOnlySpan<byte> MakeFinalSignatureBuffer(Proto.Biscuit biscuit)
    {
        var lastBlock = biscuit.Blocks.LastOrDefault() ?? biscuit.Authority;
        
        var buffer = SignatureHelper.MakeBuffer(lastBlock.Block, lastBlock.nextKey.algorithm, lastBlock.nextKey.Key, lastBlock.Signature);
        
        return (ReadOnlySpan<byte>)buffer;
    }
}