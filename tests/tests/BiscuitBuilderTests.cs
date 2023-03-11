using biscuit_net;
using biscuit_net.Datalog;
using biscuit_net.Parser;
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
        //Assert.Equal(249, bytes.Length);
    }

    [Fact]
    public void TestBuilderRules()
    {
        var signer = new SignatureCreator();
        var builder = new BiscuitBuilder(signer);
        var parser = new Parser();
        
        builder
            .AddAuthority(new F("resource", "file1"))
            .AddAuthority(new F("resource", "file2"))
            .AddAuthority(new F("operation", "read"))
            .AddAuthority(new F("operation", "write"))
            .AddAuthority(parser.ParseRule("opi($0, $1) <- resource($0), operation($1)"));

        var bytes = builder.Serialize();

        var validator = new SignatureValidator(signer.PublicKey);
        if(!Biscuit.TryDeserialize(bytes, validator, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        var authorizer = Parser.Authorizer("kkk($0, $1) <- opi($0, $1); allow if true;");        
        var success = authorizer.TryAuthorize(biscuit, out var err);

        Assert.True(success);

        //after authorization, world should contain the facts from the tokens authority facts, 
        //tokens authority rules and the authorizer rules
        var assertBlock = parser.ParseAuthorizer("""
            resource("file1");
            resource("file2");
            operation("read");
            operation("write");
            opi("file1", "read");
            opi("file1", "write");
            opi("file2", "read");
            opi("file2", "write");
            kkk("file1", "read");
            kkk("file1", "write");
            kkk("file2", "read");
            kkk("file2", "write");
        """);
        
        Assert.Equal(assertBlock.Facts.ToHashSet(), authorizer.World.Facts.ToHashSet());
        
    }

    [Fact]
    public void TestBuilderChecks()
    {
        var signer = new SignatureCreator();
        var builder = new BiscuitBuilder(signer);
        var parser = new Parser();
        
        builder
            .AddAuthority(parser.ParseCheck("""check if resource("file4")"""));

        var bytes = builder.Serialize();

        var validator = new SignatureValidator(signer.PublicKey);
        if(!Biscuit.TryDeserialize(bytes, validator, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.False(Parser.Authorizer("""resource("file3"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.True(Parser.Authorizer("""resource("file4"); allow if true;""").TryAuthorize(biscuit, out _));
    }
}