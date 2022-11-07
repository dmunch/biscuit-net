using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class VerifiedBiscuit : IBiscuit
{
    IBlock IBiscuit.Authority { get { return Authority; }}
    IEnumerable<IBlock> IBiscuit.Blocks { get { return Blocks; }}

    public VerifiedBlock Authority { get; private set; }
    public IEnumerable<VerifiedBlock> Blocks { get; protected set; }
    
    Proto.Biscuit _biscuit;
    SymbolTable _symbols;
    
    VerifiedBiscuit(Proto.Biscuit biscuit, VerifiedBlock authority, SymbolTable symbols)
    {
        _biscuit = biscuit;
        Authority = authority;
        _symbols = symbols;

        Blocks = BlockEnumerable();
    }
    
    IEnumerable<VerifiedBlock> BlockEnumerable() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            yield return VerifiedBlock.FromProto(block, _symbols);
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
        var authority = VerifiedBlock.FromProto(biscuitProto.Authority, symbols);

        biscuit = new VerifiedBiscuit(biscuitProto, authority, symbols);

        err = null; return true;
    }
}
