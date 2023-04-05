using Microsoft.AspNetCore.Authentication;

namespace biscuit_net.AspNet.Tests.Shared;

public class TestClock : ISystemClock
{
    public TestClock()
    {
        UtcNow = new DateTimeOffset(2023, 3, 26, 21, 53, 00, 000, TimeSpan.Zero);
    }

    public DateTimeOffset UtcNow { get; set; }

    public void Add(TimeSpan timeSpan)
    {
        UtcNow = UtcNow + timeSpan;
    }
}