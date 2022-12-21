using ProtoBuf;

namespace biscuit_net;
using Datalog;

public class VerifiedBlock : IBlock
{
    public IEnumerable<Fact> Facts { get; protected set; }
    public IEnumerable<RuleConstrained> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }
    public uint Version { get; protected set; }
    public string RevocationId { get; protected set; }
    public Scope Scope { get; }
    public PublicKey? SignedBy { get; }
    
    VerifiedBlock(
        IEnumerable<Fact> facts, 
        IEnumerable<RuleConstrained> rules, 
        IEnumerable<Check> checks, 
        uint version, 
        string revocationId
    ) : this(facts, rules, checks, version, revocationId, Scope.DefaultBlockScope, null)
    {
        
    }

    VerifiedBlock(
        IEnumerable<Fact> facts, 
        IEnumerable<RuleConstrained> rules, 
        IEnumerable<Check> checks, 
        uint version, 
        string revocationId, 
        Scope scope,
        PublicKey? signedBy) 
    {
        Facts = facts;
        Rules = rules;
        Checks = checks;
        Version = version;
        RevocationId = revocationId;
        Scope = scope;
        SignedBy = signedBy;
    }

    public static VerifiedBlock FromProto(Proto.SignedBlock signedBlock, SymbolTable symbols, KeyTable keys)
    {
        var block = Serializer.Deserialize<Proto.Block>( (ReadOnlySpan<byte>) signedBlock.Block);
        
        symbols.AddSymbols(block.Symbols);
        keys.Add(block.publicKeys.Select(Converters.ToPublicKey));
        
        var scope = Converters.ToScope(block.Scopes, keys);
        scope = scope.IsEmpty ? Scope.DefaultBlockScope : scope; 

        return new VerifiedBlock(
            block.FactsV2s.ToFacts(symbols),
            block.RulesV2s.ToRules(symbols, keys),
            block.ChecksV2s.ToChecks(symbols, keys),
            block.Version,
            Convert.ToHexString(signedBlock.Signature).ToLowerInvariant(),
            scope,
            signedBlock.externalSignature != null ? Converters.ToPublicKey(signedBlock.externalSignature.publicKey) : null
        );
    }
}