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
        var ephemeralKey = new EphemeralSigningKey(biscuit.Proof.nextSecret);

        var finalSignature = ephemeralKey.Sign(SignatureHelper.MakeFinalSignatureBuffer(biscuit));
        biscuit.Proof = new Proto.Proof() { finalSignature = finalSignature };
        return biscuit.Serialize();
    }
}

public class BiscuitBuilder : IBiscuitBuilder
{
    BlockBuilder _authority;
    List<IBlockSigner> _blocks = new List<IBlockSigner>();

    ISigningKey _rootKey;
            
    public BiscuitBuilder(ISigningKey rootKey)
    {
        _authority = new BlockBuilder(this);
        _rootKey = rootKey;
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
        var block = new ThirdPartyBlockSigner(thirdPartyBlock);
        _blocks.Add(block);
        return this;
    }

    public Proto.Biscuit ToProto()
    {
        var nextKey = new EphemeralSigningKey();
        var symbols = new SymbolTable();
        var keys = new KeyTable();

        var biscuit = new Proto.Biscuit() 
        {
            Authority = _authority.Sign(symbols, keys, nextKey.Public, _rootKey)
        };
        
        var currentKey = nextKey;
        foreach(var block in _blocks)
        {
            nextKey = new EphemeralSigningKey();
            biscuit.Blocks.Add(block.Sign(symbols, keys, nextKey.Public, currentKey));
            currentKey = nextKey;
        }

        biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return biscuit;    
    }
}