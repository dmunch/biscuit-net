namespace biscuit_net;
using Datalog;

public interface IBlock
{
    IEnumerable<Fact> Facts { get; }
    IEnumerable<IRuleConstrained> Rules { get; }
    IEnumerable<Check> Checks { get; }
    uint Version { get; }
}
