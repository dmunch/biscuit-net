namespace biscuit_net.Datalog;

public interface IBlock
{
    IEnumerable<Atom> Atoms { get; }
    IEnumerable<RuleExpressions> Rules { get; }
    IEnumerable<Check> Checks { get; }
    uint Version { get; }
}
