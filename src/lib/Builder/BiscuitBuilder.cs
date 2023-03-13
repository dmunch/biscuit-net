using biscuit_net.Proto;
using ProtoBuf;
using System.Buffers;

namespace biscuit_net.Builder;

public interface IBiscuitBuilder
{
    BlockBuilder AddBlock();
    Proto.Biscuit ToProto();    
}

public static class BiscuitBuilderExtensions
{
    public static ReadOnlySpan<byte> Serialize(this IBiscuitBuilder builder)
    {        
        return builder.ToProto().Serialize();        
    }

    public static ReadOnlySpan<byte> Serialize(this Proto.Biscuit proto)
    {        
        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, proto);

        return bufferWriter.WrittenSpan;
    }

    public static ReadOnlySpan<byte> Seal(this IBiscuitBuilder builder) 
    {
        var biscuit = builder.ToProto();
        var signer = new SignatureCreator(biscuit.Proof.nextSecret);

        var finalSignature = signer.Sign(SignatureHelper.MakeFinalSignatureBuffer(biscuit));
        biscuit.Proof = new Proto.Proof() { finalSignature = finalSignature };
        return biscuit.Serialize();
    }
}

public class BiscuitBuilder : IBiscuitBuilder
{
    BlockBuilder _authority;
    List<BlockBuilder> _blocks = new List<BlockBuilder>();

    SignatureCreator _signatureCreator;
            
    public BiscuitBuilder(SignatureCreator signatureCreator)
    {
        _authority = new BlockBuilder(this);
        _signatureCreator = signatureCreator;
    }

    public BlockBuilder AuthorityBlock() => _authority;
    
    public BlockBuilder AddBlock()
    {
        var block = new BlockBuilder(this);
        _blocks.Add(block);
        return block;
    }

    public Proto.Biscuit ToProto()
    {
        var nextKey = _signatureCreator.GetNextKey();
        var symbols = new SymbolTable();

        var biscuit = new Proto.Biscuit() 
        {
            Authority = SignBlock(_authority.ToProto(symbols), nextKey, _signatureCreator)
        };
        
        foreach(var block in _blocks)
        {
            var nextSigner = new SignatureCreator(nextKey);

            nextKey = _signatureCreator.GetNextKey();            
            biscuit.Blocks.Add(SignBlock(block.ToProto(symbols), nextKey, nextSigner));
        }

        biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return biscuit;    
    }

    public static Proto.SignedBlock SignBlock(Proto.Block block, SignatureCreator.NextKey nextKey, SignatureCreator signer)
    {
        var signedBlock = new SignedBlock();

        var bufferWriter = new ArrayBufferWriter<byte>();
        Serializer.Serialize(bufferWriter, block);
        
        signedBlock.Block = bufferWriter.WrittenMemory.ToArray();
        signedBlock.nextKey = new Proto.PublicKey() 
        {
            algorithm = Proto.PublicKey.Algorithm.Ed25519,
            Key = nextKey.Public
        };
        
        var buffer = SignatureHelper.MakeBuffer(signedBlock.Block, signedBlock.nextKey.algorithm, signedBlock.nextKey.Key);
        signedBlock.Signature = signer.Sign(new ReadOnlySpan<byte>(buffer));
        
        return signedBlock;    
    }
}