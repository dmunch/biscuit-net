using biscuit_net.Proto;
using ProtoBuf;
using VeryNaiveDatalog;

namespace biscuit_net;


public record RuleExpressions(
        Atom Head, 
        IEnumerable<Atom> Body, 
        IEnumerable<ExpressionV2> Expressions) 
    : Rule(Head, Body);

//TODO Assuming the int is a RuleId - specification and examples are unclear here
public record InvalidBlockRule(int RuleId/*, RuleExpressions Rule*/);


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
    
    Biscuit(Proto.Biscuit biscuit, Block authority)
    {
        _biscuit = biscuit;
        Authority = authority;

        Symbols.AddRange(authority.Symbols.ToList());
        Blocks = LoadBlocks();
    }
    public IEnumerable<Block> Blocks { get; protected set; }
    
    IEnumerable<Block> LoadBlocks() 
    {
        foreach(var block in _biscuit.Blocks)
        {
            var blockBytes = (ReadOnlySpan<byte>) block.Block;
            var blockProto = Serializer.Deserialize<Proto.Block>(blockBytes);
            
            Symbols.AddRange(blockProto.Symbols);

            yield return new Block(blockProto, Symbols);
        }
    }

    public static Biscuit Deserialize(ReadOnlySpan<byte> bytes)
    {        
        var biscuit = Serializer.Deserialize<Proto.Biscuit>((ReadOnlySpan<byte>)bytes);
        var authorityProto = Serializer.Deserialize<Proto.Block>((ReadOnlySpan<byte>)biscuit.Authority.Block);
        
        var authority = new Block(authorityProto, authorityProto.Symbols);

        return new Biscuit(biscuit, authority);
    }

    public bool CheckBoundVariables(out InvalidBlockRule invalidBlockRule)
    {
        if(!CheckBoundVariables(Authority, out invalidBlockRule))
        {
            return false;
        }

        foreach(var block in Blocks)
        {
            if(!CheckBoundVariables(block, out invalidBlockRule))
            {
                return false;
            }
        }

        invalidBlockRule = null;
        return true;
    }

    bool CheckBoundVariables(Block block, out InvalidBlockRule invalidBlockRule)
    {
        int ruleId = 0;
        foreach(var rule in block.Rules)
        {
            var headVariables = rule.Head.Terms.OfType<Variable>();
            var bodyVariables = rule.Body.SelectMany(b => b.Terms).OfType<Variable>().ToHashSet();
            
            if(!headVariables.All(hv => bodyVariables.Contains(hv)))
            {
                invalidBlockRule = new InvalidBlockRule(ruleId);
                return false;
            }
            ruleId++;
        }

        invalidBlockRule = null;
        return true;
    }
}
