using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;

public class Biscuit
{
    public Block Authority { get; private set; }
    public IReadOnlyCollection<Block> Blocks { get; protected set; }
    
    Proto.Biscuit _biscuit;
    SymbolTable _symbols;
    KeyTable _keys;
    
    Biscuit(Proto.Biscuit biscuit, Block authority, SymbolTable symbols, KeyTable keys)
    {
        _biscuit = biscuit;
        Authority = authority;
        _symbols = symbols;
        _keys = keys;

        Blocks = BlockEnumerable().ToArray();
    }
    
    IEnumerable<Block> BlockEnumerable() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            if(block.externalSignature != null)
            {
                var externalSymbolTable = new SymbolTable();
                //var externalKeyTable = new KeyTable();
                yield return Block.FromProto(block, externalSymbolTable, _keys);    
            }
            else
            {
                yield return Block.FromProto(block, _symbols, _keys);
            }
        }
    }

    public IEnumerable<string> RevocationIds 
    {
        get
        {
            yield return Authority.RevocationId;
            foreach(var block in Blocks)
            {
                yield return block.RevocationId;
            }
        }
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> bytes, SignatureValidator validator, [NotNullWhen(true)] out Biscuit? biscuit, [NotNullWhen(false)] out FailedFormat? err)
    {        
        var biscuitProto = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(!biscuitProto.VerifySignatures(validator, out err))
        {
            biscuit = null; return false; 
        }

        var symbols = new SymbolTable();
        var keys = new KeyTable();
        var authority = Block.FromProto(biscuitProto.Authority, symbols, keys);

        biscuit = new Biscuit(biscuitProto, authority, symbols, keys);

        err = null; return true;
    }
}
