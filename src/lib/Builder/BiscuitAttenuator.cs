using ProtoBuf;

namespace biscuit_net.Builder;

public class BiscuitAttenuator : IBiscuitBuilder
{
    Proto.Biscuit _biscuit; 
    SymbolTable _symbolTable;
    KeyTable _keyTable;
    Proto.PublicKey _nextKey;
    
    List<IBlockSigner> _blocks = new List<IBlockSigner>();

    BiscuitAttenuator(Proto.Biscuit biscuit, SymbolTable symbolTable, KeyTable keyTable, Proto.PublicKey nextKey)
    {
        _biscuit = biscuit;
        _symbolTable = symbolTable;
        _keyTable = keyTable;
        _nextKey = nextKey;
    }

    public static BiscuitAttenuator Attenuate(ReadOnlySpan<byte> bytes)    
    {        
        var biscuit = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(biscuit.Proof.finalSignature != null)
        {
            throw new ArgumentException("Biscuit is sealed");
        }

        var symbols = new SymbolTable();
        var keys = new KeyTable();
        

        var authority = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuit.Authority.Block);
        symbols.AddSymbols(authority.Symbols);
        keys.Add(authority.publicKeys.Select(Converters.ToPublicKey));
        var nextKey = biscuit.Authority.nextKey;
        

        foreach(var signedBlock in biscuit.Blocks)
        {
            var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);        
            symbols.AddSymbols(block.Symbols);
            keys.Add(block.publicKeys.Select(Converters.ToPublicKey));
            nextKey = signedBlock.nextKey;
        }

        return new BiscuitAttenuator(biscuit, symbols, keys, nextKey);
    }

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

    public IBiscuitBuilder AddThirdPartyBlock(Func<ThirdPartyBlockRequest, ThirdPartyBlock> thirdPartyBlockConfigurator)
    {
        var thirdPartyBlock = thirdPartyBlockConfigurator(BuildThirdPartyBlockRequest());
        return AddThirdPartyBlock(thirdPartyBlock);
    }

    public ThirdPartyBlockRequest BuildThirdPartyBlockRequest()
    {
        return new ThirdPartyBlockRequest(new PublicKey((Algorithm)_nextKey.algorithm, _nextKey.Key), Enumerable.Empty<PublicKey>());
    }

    public Proto.Biscuit ToProto()
    {                
        var currentKey = new EphemeralSigningKey(_biscuit.Proof.nextSecret);        
        
        foreach(var block in _blocks)
        {   
            var nextKey = new EphemeralSigningKey();
            _biscuit.Blocks.Add(block.Sign(_symbolTable, _keyTable, nextKey.Public, currentKey));

            currentKey = nextKey;
        }

        _biscuit.Proof = new Proto.Proof() { nextSecret = currentKey.Private };
        //biscuit.rootKeyId = 1;

        return _biscuit;    
    }
}
