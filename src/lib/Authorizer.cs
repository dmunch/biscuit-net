using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class Authorizer
{
    List<Fact> _authorizerFacts = new List<Fact>();
    List<Check> _authorizerChecks = new List<Check>();
    
    public void AddFact(Fact fact)
    {
        _authorizerFacts.Add(fact);
    }

    public void AddCheck(Check check)
    {
        _authorizerChecks.Add(check);
    }

    public bool TryAuthorize(VerifiedBiscuit b, [NotNullWhen(false)] out Error? err)
    {
        //var world = new World(_authorizerFacts.ToHashSet(), _authorizerChecks);

        var FactSet = new FactSet();
        var ruleSet = new RuleSet();
        var world = new World(FactSet, ruleSet, _authorizerChecks);

        FactSet.Add(Origin.Authorizer, _authorizerFacts.ToHashSet());
        //ruleSet.Add(new Origin(0), _aut)
        return Verifier.TryVerify(b, world, out err);
    }
}
