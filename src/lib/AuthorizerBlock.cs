namespace biscuit_net;
using Datalog;

public class AuthorizerBlock
{
    readonly List<Fact> _facts = new();
    readonly List<RuleScoped> _rules = new();
    readonly List<Check> _checks = new();
    readonly List<Policy> _policies = new();

    public IEnumerable<Fact> Facts { get => _facts; }
    public IEnumerable<RuleScoped> Rules { get => _rules; }
    public IEnumerable<Check> Checks { get => _checks; }
    public IEnumerable<Policy> Policies { get => _policies; }
    
    public AuthorizerBlock Add(Fact fact) { _facts.Add(fact); return this; }
    public AuthorizerBlock Add(RuleScoped rule) { _rules.Add(rule); return this; }
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
