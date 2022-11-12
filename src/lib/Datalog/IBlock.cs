namespace biscuit_net.Datalog;

public interface IBlock
{
    IEnumerable<Fact> Facts { get; }
    IEnumerable<IRuleConstrained> Rules { get; }
    IEnumerable<Check> Checks { get; }
    uint Version { get; }
}
