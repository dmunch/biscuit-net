using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public record Policy(PolicyKind Kind, IReadOnlyCollection<RuleConstrained> Rules)
{
    static Expressions.Expression True = new Expressions.Expression(new List<Expressions.Op>{ new Expressions.Op(new Boolean(true))});

    public static Policy AllowPolicy = new Policy(PolicyKind.Allow, new [] { new RuleConstrained(new Fact("query"), True) });
    public static Policy DenyPolicy = new Policy(PolicyKind.Deny, new [] { new RuleConstrained(new Fact("query"), True) });
}

public enum PolicyKind
{
    Allow,
    Deny
}

public class AuthorizerBlock : IBlock
{
    List<Fact> _facts = new List<Fact>();
    List<RuleConstrained> _rules = new List<RuleConstrained>();
    List<Check> _checks = new List<Check>();
    List<Policy> _policies = new List<Policy>();    

    public IEnumerable<Fact> Facts { get => _facts; }
    public IEnumerable<RuleConstrained> Rules { get => _rules; }
    public IEnumerable<Check> Checks { get => _checks; }
    public IEnumerable<Policy> Policies { get => _policies; }
        

    public Scope Scope { get => Scope.DefaultBlockScope; }
    public PublicKey? SignedBy { get => null; }
    public uint Version { get => 4; }

    public AuthorizerBlock Add(Fact fact) { _facts.Add(fact); return this; }
    public AuthorizerBlock Add(RuleConstrained rule) { _rules.Add(rule); return this; }
    public AuthorizerBlock Add(Check check) { _checks.Add(check); return this; }
    public AuthorizerBlock Add(Policy policy) { _policies.Add(policy); return this; }
}

public class Authorizer
{
    AuthorizerBlock _authorizerBlock = new AuthorizerBlock();

    public void Add(Fact fact) => _authorizerBlock.Add(fact);
    public void Add(RuleConstrained rule) => _authorizerBlock.Add(rule);
    public void Add(Check check) => _authorizerBlock.Add(check);
    public void Add(Policy policy) => _authorizerBlock.Add(policy);
    
    public void Allow()
    {
        Add(Policy.AllowPolicy);
    }

    public void Deny()
    {
        Add(Policy.DenyPolicy);
    }

    public bool TryAuthorize(VerifiedBiscuit b, [NotNullWhen(false)] out Error? err)
    {
        var factSet = new FactSet();
        var ruleSet = new RuleSet();
        var world = new World(factSet, ruleSet/*, _authorizerChecks*/);

        return Verifier.TryVerify(b, world, _authorizerBlock, out err);
    }
}
