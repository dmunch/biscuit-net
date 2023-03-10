using biscuit_net;
using biscuit_net.Datalog;
using F = biscuit_net.Datalog.Fact;

namespace tests;

public class BiscuitBuilderTests
{
    [Fact]
    public void TestBuilder()
    {
        var signer = new SignatureCreator();
        var builder = new BiscuitBuilder(signer);

        builder
            .AddAuthority(new F("right", "/a/file1.txt", "read"))
            .AddAuthority(new F("right", "/a/file1.txt", "write"))
            .AddAuthority(new F("right", "/a/file2.txt", "read"))
            .AddAuthority(new F("right", "/b/file2.txt", "write"));

        var bytes = builder.Serialize();

        var validator = new SignatureValidator(signer.PublicKey);
        if(!Biscuit.TryDeserialize(bytes, validator, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        //249 taken from https://github.com/biscuit-auth/biscuit-rust/blob/e05e0db497f7f7039bdcd466a686bfb2c0913da6/biscuit-auth/src/lib.rs#L56
        Assert.Equal(249, bytes.Length);
    }
}