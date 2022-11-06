using ProtoBuf;
using VeryNaiveDatalog;
using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class Biscuit
{
    public Block Authority { get; private set; }
    public IEnumerable<Block> Blocks { get; protected set; }
    Proto.Biscuit _biscuit;
    SymbolTable _symbols;
    
    Biscuit(Proto.Biscuit biscuit, Block authority, SymbolTable symbols)
    {
        _biscuit = biscuit;
        Authority = authority;
        _symbols = symbols;

        Blocks = BlockEnumerable();
    }
    
    IEnumerable<Block> BlockEnumerable() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            yield return Block.FromProto(block, _symbols);
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
        var authority = Block.FromProto(biscuitProto.Authority, symbols);

        biscuit = new Biscuit(biscuitProto, authority, symbols);

        err = null; return true;
    }
}

public class Block
{
    public IEnumerable<Atom> Atoms { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }
    public string RevocationId { get; protected set; }

    Block(IEnumerable<Atom> atoms, IEnumerable<RuleExpressions> rules, IEnumerable<Check> checks, string revocationId) 
    {
        Atoms = atoms;
        Rules = rules;
        Checks = checks;
        RevocationId = revocationId;
    }

    public static Block FromProto(Proto.SignedBlock signedBlock, SymbolTable symbols)
    {
        var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);
        
        symbols.AddSymbols(block.Symbols);

        return new Block(
            block.FactsV2s.ToAtoms(symbols),
            block.RulesV2s.ToRules(symbols),
            block.ChecksV2s.ToChecks(symbols),
            Convert.ToHexString(signedBlock.Signature).ToLowerInvariant()
        );
    }
}