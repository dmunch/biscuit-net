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
        var world = new World(_authorizerAtoms.ToList(), _authorizerChecks);
        return Verifier.TryVerify(b, world, out err);
    }
}
