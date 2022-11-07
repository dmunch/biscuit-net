namespace biscuit_net.Datalog;

public interface IBiscuit
{
    IBlock Authority { get; }
    IEnumerable<IBlock> Blocks { get; }
}