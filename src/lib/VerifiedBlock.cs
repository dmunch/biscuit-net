using ProtoBuf;

namespace biscuit_net;
using Datalog;

public class VerifiedBlock : IBlock
{
    public IEnumerable<Atom> Atoms { get; protected set; }
    public IEnumerable<RuleExpressions> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }
    public string RevocationId { get; protected set; }

    VerifiedBlock(IEnumerable<Atom> atoms, IEnumerable<RuleExpressions> rules, IEnumerable<Check> checks, string revocationId) 
    {
        Atoms = atoms;
        Rules = rules;
        Checks = checks;
        RevocationId = revocationId;
    }

    public static VerifiedBlock FromProto(Proto.SignedBlock signedBlock, SymbolTable symbols)
    {
        var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);
        
        symbols.AddSymbols(block.Symbols);

        return new VerifiedBlock(
            block.FactsV2s.ToAtoms(symbols),
            block.RulesV2s.ToRules(symbols),
            block.ChecksV2s.ToChecks(symbols),
            Convert.ToHexString(signedBlock.Signature).ToLowerInvariant()
        );
    }
}