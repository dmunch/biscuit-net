using System.Diagnostics.CodeAnalysis;

namespace biscuit_net;
using Datalog;

public class Authorizer
{
    List<Atom> _authorizerAtoms = new List<Atom>();
    List<Check> _authorizerChecks = new List<Check>();
    
    public void AddAtom(Atom atom)
    {
        _authorizerAtoms.Add(atom);
    }

    public void AddCheck(Check check)
    {
        _authorizerChecks.Add(check);
    }

    public bool TryAuthorize(VerifiedBiscuit b, [NotNullWhen(false)] out Error? err)
    {
        //var world = new World(_authorizerAtoms.ToHashSet(), _authorizerChecks);

        var atomSet = new AtomSet();
        var ruleSet = new RuleSet();
        var world = new World(atomSet, ruleSet, _authorizerChecks);

        atomSet.Add(Origin.Authorizer, _authorizerAtoms.ToHashSet());
        //ruleSet.Add(new Origin(0), _aut)
        return Verifier.TryVerify(b, world, out err);
    }
}
