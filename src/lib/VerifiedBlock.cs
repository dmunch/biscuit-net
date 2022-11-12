using ProtoBuf;

namespace biscuit_net;
using Datalog;

public class VerifiedBlock : IBlock
{
    public IEnumerable<Fact> Facts { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }
    public uint Version { get; protected set; }
    public string RevocationId { get; protected set; }

    VerifiedBlock(IEnumerable<Fact> facts, IEnumerable<RuleExpressions> rules, IEnumerable<Check> checks, uint version, string revocationId) 
    {
        Facts = facts;
        Rules = rules;
        Checks = checks;
        Version = version;
        RevocationId = revocationId;
    }

    public static VerifiedBlock FromProto(Proto.SignedBlock signedBlock, SymbolTable symbols)
    {
        var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);
        
        symbols.AddSymbols(block.Symbols);

        return new VerifiedBlock(
            block.FactsV2s.ToFacts(symbols),
            block.RulesV2s.ToRules(symbols),
            block.ChecksV2s.ToChecks(symbols),
            block.Version,
            Convert.ToHexString(signedBlock.Signature).ToLowerInvariant()
        );
    }
}