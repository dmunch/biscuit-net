using ProtoBuf;

namespace biscuit_net.Builder;

public class BiscuitAttenuator : IBiscuitBuilder
{
    Proto.Biscuit _biscuit; 
    SymbolTable _symbolTable;

    List<BlockBuilder> _blocks = new List<BlockBuilder>();

    SignatureCreator _signatureCreator;
            
    
    BiscuitAttenuator(Proto.Biscuit biscuit, SymbolTable symbolTable)
    {
        _biscuit = biscuit;
        _symbolTable = symbolTable;
        _signatureCreator = new SignatureCreator(biscuit.Proof.nextSecret);
    }

    public static BiscuitAttenuator Attenuate(ReadOnlySpan<byte> bytes)    
    {        
        var biscuit = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        var symbols = new SymbolTable();

        var authority = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuit.Authority.Block);
        symbols.AddSymbols(authority.Symbols);

        foreach(var signedBlock in biscuit.Blocks)
        {
            var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);        
            symbols.AddSymbols(block.Symbols);
        }

        return new BiscuitAttenuator(biscuit, symbols);
    }

    public BlockBuilder AddBlock()
    {
        var block = new BlockBuilder(this);
        _blocks.Add(block);
        
        return block;
    }

    public Proto.Biscuit ToProto()
    {                
        var nextSigner = _signatureCreator;
        var nextKey = new SignatureCreator.NextKey(nextSigner.PublicKey, _biscuit.Proof.nextSecret);
        foreach(var block in _blocks)
        {   
            nextKey = _signatureCreator.GetNextKey();                     
            _biscuit.Blocks.Add(BiscuitBuilder.SignBlock(block.ToProto(_symbolTable), nextKey, nextSigner));

            nextSigner = new SignatureCreator(nextKey);        
        }

        _biscuit.Proof = new Proto.Proof() { nextSecret = nextKey.Private };
        //biscuit.rootKeyId = 1;

        return _biscuit;    
    }
}
