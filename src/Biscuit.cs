using biscuit_net.Proto;
using ProtoBuf;
using VeryNaiveDatalog;

namespace biscuit_net;


public record RuleExpressions(
        Atom Head, 
        IEnumerable<Atom> Body, 
        IEnumerable<ExpressionV2> Expressions) 
    : Rule(Head, Body);


public class Block
{
    Proto.Block block;

    public IEnumerable<Atom> Atoms { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<RuleExpressions> CheckQueries { get; protected set; }

    public List<string> Symbols { get; private set; }

    public Block(Proto.Block block, List<string> symbols)
    {
        Atoms = block.FactsV2s.ToAtoms(symbols);
        Rules = block.RulesV2s.ToRules(symbols);
        CheckQueries = block.ChecksV2s.ToQueries(symbols);

        Symbols = symbols;
    }
}



public class Biscuit
{
    Proto.Biscuit _biscuit;
    public Block Authority { get; private set; }

    public List<string> Symbols { get; protected set; }= new List<string>();
    Block[] _blocks;
    Biscuit(Proto.Biscuit biscuit, Block authority)
    {
        _biscuit = biscuit;
        Authority = authority;

        Symbols.AddRange(authority.Symbols.ToList());
        _blocks = new Block[_biscuit.Blocks.Count];
    }

    public IEnumerable<Block> Blocks 
    {
        get
        {
            for(int blockId = 0; blockId < _biscuit.Blocks.Count; blockId++)
            {
                if(_blocks[blockId] != null) 
                {
                    yield return _blocks[blockId];
                }
                
                var blockBytes = (ReadOnlySpan<byte>)_biscuit.Blocks[blockId].Block;
                var blockProto = Serializer.Deserialize<Proto.Block>(blockBytes);

                Symbols.AddRange(blockProto.Symbols);

                _blocks[blockId] = new Block(blockProto, Symbols);
                yield return _blocks[blockId];
            }
        }
    }

    public static Biscuit Deserialize(ReadOnlySpan<byte> bytes)
    {        
        var biscuit = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);
        var authorityProto = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuit.Authority.Block);
        
        var authority = new Block(authorityProto, authorityProto.Symbols);

        return new Biscuit(biscuit, authority);
    }
}
