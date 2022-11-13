using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;

public class VerifiedBiscuit : IBiscuit
{
    IBlock IBiscuit.Authority { get { return Authority; }}
    IReadOnlyCollection<IBlock> IBiscuit.Blocks { get { return Blocks; }}

    public VerifiedBlock Authority { get; private set; }
    public IReadOnlyCollection<VerifiedBlock> Blocks { get; protected set; }
    
    Proto.Biscuit _biscuit;
    SymbolTable _symbols;
    KeyTable _keys;
    
    VerifiedBiscuit(Proto.Biscuit biscuit, VerifiedBlock authority, SymbolTable symbols, KeyTable keys)
    {
        _biscuit = biscuit;
        Authority = authority;
        _symbols = symbols;
        _keys = keys;

        Blocks = BlockEnumerable().ToArray();
    }
    
    IEnumerable<VerifiedBlock> BlockEnumerable() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            yield return VerifiedBlock.FromProto(block, _symbols, _keys);
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

    public static bool TryDeserialize(ReadOnlySpan<byte> bytes, SignatureValidator validator, [NotNullWhen(true)] out VerifiedBiscuit? biscuit, [NotNullWhen(false)] out FailedFormat? err)
    {        
        var biscuitProto = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(!biscuitProto.VerifySignatures(validator, out err))
        {
            biscuit = null; return false; 
        }

        var symbols = new SymbolTable();
        var keys = new KeyTable();
        var authority = VerifiedBlock.FromProto(biscuitProto.Authority, symbols, keys);

        biscuit = new VerifiedBiscuit(biscuitProto, authority, symbols, keys);

        err = null; return true;
    }
}
