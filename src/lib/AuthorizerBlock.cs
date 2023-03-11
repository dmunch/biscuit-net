namespace biscuit_net;
using Datalog;

public class AuthorizerBlock
{
    List<Fact> _facts = new List<Fact>();
    List<Rule> _rules = new List<Rule>();
    List<Check> _checks = new List<Check>();
    List<Policy> _policies = new List<Policy>();    

    public IEnumerable<Fact> Facts { get => _facts; }
    public IEnumerable<Rule> Rules { get => _rules; }
    public IEnumerable<Check> Checks { get => _checks; }
    public IEnumerable<Policy> Policies { get => _policies; }
        

    public Scope Scope { get => Scope.DefaultBlockScope; }
    public PublicKey? SignedBy { get => null; }
    public uint Version { get => 4; }

    public AuthorizerBlock Add(Fact fact) { _facts.Add(fact); return this; }
    public AuthorizerBlock Add(Rule rule) { _rules.Add(rule); return this; }
    public AuthorizerBlock Add(Check check) { _checks.Add(check); return this; }
    public AuthorizerBlock Add(Policy policy) { _policies.Add(policy); return this; }

    public AuthorizerBlock Add(AuthorizerBlock other) 
    {
        foreach(var fact in other.Facts)
        {
            Add(fact);
        }

        foreach(var rule in other.Rules)
        {
            Add(rule);
        }

        foreach(var policy in other.Policies)
        {
            Add(policy);
        }

        foreach(var chck in other.Checks)
        {
            Add(chck);
        }
        
        return this;
    }
}
