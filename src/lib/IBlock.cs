namespace biscuit_net;
using Datalog;

public interface IBlock
{
    IEnumerable<Fact> Facts { get; }
    IEnumerable<RuleConstrained> Rules { get; }
    IEnumerable<Check> Checks { get; }
    uint Version { get; }
    public Scope Scope { get; }
    public PublicKey? SignedBy { get; }
}
