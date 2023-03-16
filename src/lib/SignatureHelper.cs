using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Builder;

public record FailedFormat(Signature? Signature, int? InvalidSignatureSize);
public record Signature(string InvalidSignature);

static class SignatureHelper
{
    public static byte[] MakeBuffer(byte[] block, Proto.PublicKey publicKey, byte[]? additionalBytes = null)
    {
        var buffer = new byte[block.Length + sizeof(int) + publicKey.Key.Length + (additionalBytes?.Length ?? 0)];
        var bytes = (Span<byte>) buffer;
        
        block.CopyTo(buffer, 0);

        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(block.Length, sizeof(int)), (int)publicKey.algorithm);
        publicKey.Key.CopyTo(buffer, block.Length + sizeof(int));

        additionalBytes?.CopyTo(buffer, block.Length + sizeof(int) + publicKey.Key.Length);
        
        return buffer;
    }

    public static byte[] MakeBuffer(byte[] block, byte[] externalSignature, Proto.PublicKey publicKey)
    {
        var buffer = new byte[block.Length + externalSignature.Length + sizeof(int) + publicKey.Key.Length];
        var bytes = (Span<byte>) buffer;
        
        block.CopyTo(buffer, 0);
        externalSignature.CopyTo(buffer, block.Length);

        var keyPosition = block.Length + externalSignature.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(keyPosition, sizeof(int)), (int)publicKey.algorithm);
        publicKey.Key.CopyTo(buffer, keyPosition + sizeof(int));
        
        return buffer;
    }

    static bool VerifySignature(this Proto.SignedBlock signedBlock, IVerificationKey verificationKey, [NotNullWhen(false)] out int? invalidSignatureSize)
    {
        if(signedBlock.Signature.Length != 64)
        {
            invalidSignatureSize = signedBlock.Signature.Length;
            return false;
        }
        invalidSignatureSize = null;

        //IMPROVE: could use an array pool here

        var buffer = signedBlock.externalSignature != null 
            ? MakeBuffer(signedBlock.Block, signedBlock.externalSignature.Signature, signedBlock.nextKey)
            : MakeBuffer(signedBlock.Block,signedBlock.nextKey);

        if(signedBlock.externalSignature == null)
        {
            return verificationKey.Verify(buffer, signedBlock.Signature);
        }
        
        var externalBuffer = MakeBuffer(signedBlock.Block, ProtoConverters.ToPublicKey(verificationKey.PublicKey));
        var externalverificationKey = ICryptoProvider.CreateVerificationKey(signedBlock.externalSignature.publicKey);

        return    verificationKey.Verify(buffer, signedBlock.Signature)
               && externalverificationKey.Verify(externalBuffer, signedBlock.externalSignature.Signature);

    }

    static bool VerifySignatures(this Proto.Biscuit biscuitProto, IVerificationKey verificationKey, [NotNullWhen(false)] out int? invalidSignatureSize)
    {
        if(!biscuitProto.Authority.VerifySignature(verificationKey, out invalidSignatureSize))
        {
            return false;
        }

        var nextverificationKey = ICryptoProvider.CreateVerificationKey(biscuitProto.Authority.nextKey);
        foreach(var block in biscuitProto.Blocks)
        {
            if(!block.VerifySignature(nextverificationKey, out invalidSignatureSize))
            {
                return false;
            }
            nextverificationKey = ICryptoProvider.CreateVerificationKey(block.nextKey);
        }

        return true;
    }

    static bool VerifyProof(Proto.Biscuit biscuitProto)
    {
        //verify proof 
        var publicKey = (biscuitProto.Blocks.LastOrDefault() ?? biscuitProto.Authority).nextKey;
        var verificationKey = ICryptoProvider.CreateVerificationKey(publicKey);
        
        if(biscuitProto.Proof.finalSignature != null)
        {
            //token is sealed                
            var buffer = MakeFinalSignatureBuffer(biscuitProto);
            
            return verificationKey.Verify(buffer, biscuitProto.Proof.finalSignature);            
        } 
        else if(biscuitProto.Proof.nextSecret != null) 
        {
            //TODO why is the nextSecret algorithm not specified??
            var cryptoProvider = ICryptoProvider.Create(Algorithm.Ed25519);
            var key = cryptoProvider.CreateEphemeral(biscuitProto.Proof.nextSecret);
            return verificationKey.PublicKey == key.Public;
        } 

        //proof field has been tampered with
        return false;        
    }

    public static bool VerifySignatures(this Proto.Biscuit biscuitProto, IVerificationKey verificationKey, [NotNullWhen(false)] out FailedFormat? err)
    {
        if(!biscuitProto.VerifySignatures(verificationKey, out int? invalidSignatureSize))
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
        
        var buffer = SignatureHelper.MakeBuffer(lastBlock.Block, lastBlock.nextKey, lastBlock.Signature);
        
        return (ReadOnlySpan<byte>)buffer;
    }
}