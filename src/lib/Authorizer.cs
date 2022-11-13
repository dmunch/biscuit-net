using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class AuthorizerBlock : IBlock
{
    List<Fact> _facts = new List<Fact>();
    List<RuleConstrained> _rules = new List<RuleConstrained>();
    List<Check> _checks = new List<Check>();
    

    public IEnumerable<Fact> Facts { get => _facts; }
    public IEnumerable<IRuleConstrained> Rules { get => _rules; }
    public IEnumerable<Check> Checks { get => _checks; }
    

    public Scope Scope { get => Scope.DefaultBlockScope; }
    public PublicKey? SignedBy { get => null; }
    public uint Version { get => 4; }

    public void Add(Fact fact) => _facts.Add(fact);
    public void Add(RuleConstrained rule) => _rules.Add(rule);
    public void Add(Check check) => _checks.Add(check);
}

public class Authorizer
{
    AuthorizerBlock _authorizerBlock = new AuthorizerBlock();

    public void Add(Fact fact) => _authorizerBlock.Add(fact);
    public void Add(RuleConstrained rule) => _authorizerBlock.Add(rule);
    public void Add(Check check) => _authorizerBlock.Add(check);
    
    public bool TryAuthorize(VerifiedBiscuit b, [NotNullWhen(false)] out Error? err)
    {
        var factSet = new FactSet();
        var ruleSet = new RuleSet();
        var world = new World(factSet, ruleSet/*, _authorizerChecks*/);

        return Verifier.TryVerify(b, world, _authorizerBlock, out err);
    }
}
