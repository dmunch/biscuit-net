namespace biscuit_net;

public interface IBiscuit
{
    IBlock Authority { get; }
    IEnumerable<IBlock> Blocks { get; }
}