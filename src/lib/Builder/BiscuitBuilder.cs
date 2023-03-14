using biscuit_net.Proto;
using ProtoBuf;
using System.Buffers;

namespace biscuit_net.Builder;

public interface IBiscuitBuilder
{
    BlockBuilder AddBlock();
    IBiscuitBuilder AddThirdPartyBlock(ThirdPartyBlock thirdPartyBlock);
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
    List<IBlockBuilder> _blocks = new List<IBlockBuilder>();

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

    public IBiscuitBuilder AddThirdPartyBlock(ThirdPartyBlock thirdPartyBlock)
    {
        var block = new ThirdPartyBlockBuilder(thirdPartyBlock);
        _blocks.Add(block);
        return this;
    }

    public Proto.Biscuit ToProto()
    {
        var nextKey = _signatureCreator.GetNextKey();
        var symbols = new SymbolTable();

        var biscuit = new Proto.Biscuit() 
        {
            Authority = _authority.Sign(symbols, nextKey, _signatureCreator)
        };
        
        foreach(var block in _blocks)
        {
            var nextSigner = new SignatureCreator(nextKey);

            nextKey = _signatureCreator.GetNextKey();      

            
            biscuit.Blocks.Add(block.Sign(symbols, nextKey, nextSigner));
        }

        biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return biscuit;    
    }
}