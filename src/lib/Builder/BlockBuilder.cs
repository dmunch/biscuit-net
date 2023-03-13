using biscuit_net.Datalog;

namespace biscuit_net.Builder;

public class BlockBuilder
{
    public List<Fact> Facts { get; } = new List<Fact>();
    public List<Rule> Rules { get; } = new List<Rule>();
    public List<Check> Checks { get; } = new List<Check>();

    IBiscuitBuilder _topLevelBuilder;
    public BlockBuilder(IBiscuitBuilder topLevelBuilder)
    {
        _topLevelBuilder = topLevelBuilder;
    }

    public BlockBuilder Add(Fact fact) { Facts.Add(fact); return this; }
    public BlockBuilder Add(Rule rule) { Rules.Add(rule); return this; }
    public BlockBuilder Add(Check check) { Checks.Add(check); return this; } 

    public IBiscuitBuilder EndBlock() => _topLevelBuilder;

    public Proto.Block ToProto(SymbolTable symbols)
    {
        var blockV2 = new Proto.Block();

        var symbolsBefore = symbols.Symbols.ToList(); //deep copy 
        blockV2.FactsV2s.AddRange(ProtoConverters.ToFactsV2(Facts, symbols));
        blockV2.RulesV2s.AddRange(ProtoConverters.ToRulesV2(Rules, symbols));
        blockV2.ChecksV2s.AddRange(ProtoConverters.ToChecksV2(Checks, symbols));
        
        blockV2.Symbols.AddRange(symbols.Symbols.Except(symbolsBefore)); //add symbol delta, not all symbols

        blockV2.Version = 3;

        blockV2.Scopes.Add(new Proto.Scope() { scopeType = Proto.Scope.ScopeType.Authority });

        return blockV2;
    }
}
