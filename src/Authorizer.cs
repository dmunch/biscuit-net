using VeryNaiveDatalog;

namespace biscuit_net;

public record World(List<Atom> Atoms);

public class Authorizer
{
    List<Atom> _authorizerAtoms = new List<Atom>();

    public void AddAtom(Atom atom)
    {
        _authorizerAtoms.Add(atom);
    }

    public (bool, int, int, Rule?) Authorize(Biscuit b)
    {
        var world = new World(b.Authority.Atoms.ToList());
        
        world.Atoms.AddRange(_authorizerAtoms);

        var (check, authorityFailedCheckId, authorityFailedRule) = Check(world, b.Authority);

        if(!check)
            return (false, 0, authorityFailedCheckId, authorityFailedRule);

        var blockId = 1;
        foreach(var block in b.Blocks)
        {
            world.Atoms.AddRange(block.Atoms);

            var (blockCheck, failedCheckId, failedRule) = Check(world, block);
            check &= blockCheck;

            if(!check) return (false, blockId, failedCheckId, failedRule);
            blockId++;
        }

        return (true, -1, -1, null);
    }

    (bool, int, Rule?) Check(World world, Block block)
    {
        var rulesAtoms = block.Atoms.EvaluateWithExpressions(block.Rules);

        var blockScopedAtoms = world.Atoms.ToList();
        blockScopedAtoms.AddRange(rulesAtoms);

        
        var result = true;
        var i = 0;
        foreach(var query in block.CheckQueries)
        {
            var eval = world.Atoms.EvaluateWithExpressions(new []{query});

            var checkScopedAtoms = blockScopedAtoms.ToList();
            checkScopedAtoms.AddRange(eval);
            var subs = query.Head.UnifyWith(checkScopedAtoms, new Substitution());

            result &= subs.Any();
            if(!result) 
            {
                //check failed? we return false, alongside
                //the check index and the actual rule used for the check
                return (false, i, query);
            }
            i++;
        }

        return (true, -1, null);
    }
}
