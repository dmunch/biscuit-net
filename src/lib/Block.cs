using ProtoBuf;

namespace biscuit_net;
using Datalog;

public record ThirdPartyBlock(byte[] Bytes, byte[] Signature, PublicKey PublicKey);

public class Block
{
    public IEnumerable<Fact> Facts { get; protected set; }
    public IEnumerable<RuleScoped> Rules { get; protected set; }
    public IEnumerable<Check> Checks { get; protected set; }
    public uint Version { get; protected set; }    
    public Scope Scope { get; }
    public PublicKey? SignedBy { get; }
    
    public Block(
        IEnumerable<Fact> facts, 
        IEnumerable<RuleScoped> rules, 
        IEnumerable<Check> checks, 
        uint version        
    ) : this(facts, rules, checks, version, Scope.DefaultBlockScope, null)
    {
        
    }

    public Block(
        IEnumerable<Fact> facts, 
        IEnumerable<RuleScoped> rules, 
        IEnumerable<Check> checks, 
        uint version,
        Scope scope,
        PublicKey? signedBy) 
    {
        Facts = facts;
        Rules = rules;
        Checks = checks;
        Version = version;        
        Scope = scope;
        SignedBy = signedBy;
    }
}