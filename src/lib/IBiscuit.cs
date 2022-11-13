namespace biscuit_net;

public interface IBiscuit
{
    IBlock Authority { get; }
    IReadOnlyCollection<IBlock> Blocks { get; }
}