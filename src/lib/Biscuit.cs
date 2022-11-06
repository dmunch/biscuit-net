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
            var blockBytes = (ReadOnlySpan<byte>) block.Block;
            var blockProto = Serializer.Deserialize<Proto.Block>(blockBytes);
            
            _symbols.AddSymbols(blockProto.Symbols);
            yield return Block.FromProto(blockProto, _symbols);
        }
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> bytes, SignatureValidator validator, [NotNullWhen(true)] out Biscuit? biscuit, [NotNullWhen(false)] out FailedFormat? err)
    {        
        var biscuitProto = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);

        if(!biscuitProto.VerifySignatures(validator, out err))
        {
            biscuit = null; return false; 
        }

        var authorityProto = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuitProto.Authority.Block);
        var symbols = new SymbolTable(authorityProto.Symbols);
        var authority = Block.FromProto(authorityProto, symbols);

        biscuit = new Biscuit(biscuitProto, authority, symbols);

        err = null; return true;
    }
}

public class Block
{
    public IEnumerable<Atom> Atoms { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }

    Block(IEnumerable<Atom> atoms, IEnumerable<RuleExpressions> rules, IEnumerable<Check> checks) 
    {
        Atoms = atoms;
        Rules = rules;
        Checks = checks;
    }

    public static Block FromProto(Proto.Block block, SymbolTable symbols)
    {
        return new Block(
            block.FactsV2s.ToAtoms(symbols),
            block.RulesV2s.ToRules(symbols),
            block.ChecksV2s.ToChecks(symbols)
        );
    }
}