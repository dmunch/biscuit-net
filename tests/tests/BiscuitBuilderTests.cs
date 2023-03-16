using biscuit_net;
using biscuit_net.Datalog;
using biscuit_net.Parser;
using biscuit_net.Builder;

namespace tests;

public class BiscuitBuilderTests
{
    [Fact]
    public void TestBuilder()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var bytes = Biscuit.New(rootKey)
                .AuthorityBlock("""
                    right("/a/file1.txt", "read");
                    right("/a/file1.txt", "write");
                    right("/a/file2.txt", "read");
                    right("/a/file2.txt", "write");
                """)
                .EndBlock()
            .Serialize();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if (!Biscuit.TryDeserialize(bytes, verificationKey, out _, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        //249 taken from https://github.com/biscuit-auth/biscuit-rust/blob/e05e0db497f7f7039bdcd466a686bfb2c0913da6/biscuit-auth/src/lib.rs#L56
        //Assert.Equal(249, bytes.Length);
    }

    [Fact]
    public void TestBuilderRules()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var bytes = Biscuit.New(rootKey)
            .AuthorityBlock("""
                    resource("file1");
                    resource("file2");
                    operation("read");
                    operation("write");
                    opi($0, $1) <- resource($0), operation($1);
                """)
            .EndBlock()
            .Serialize();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(bytes, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        var authorizer = Parser.Authorizer("kkk($0, $1) <- opi($0, $1); allow if true;");
        var success = authorizer.TryAuthorize(biscuit, out _);

        Assert.True(success);

        //after authorization, world should contain the facts from the tokens authority facts, 
        //tokens authority rules and the authorizer rules
        var assertBlock = Parser.Block("""
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
        var rootKey = Ed25519.NewSigningKey();
        
        var bytes = Biscuit.New(rootKey)
            .AuthorityBlock("""check if resource("file4");""")
            .EndBlock()
            .Serialize();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(bytes, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.False(Parser.Authorizer("""resource("file3"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.True(Parser.Authorizer("""resource("file4"); allow if true;""").TryAuthorize(biscuit, out _));
    }

    [Fact]
    public void TestBuilderBlocks()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var bytes = Biscuit.New(rootKey)
            .AuthorityBlock("""resource("file4");""")
            .EndBlock()
            .AddBlock("""
                check if resource("file4");
                check if resource("file5");
            """).EndBlock()
            .Serialize();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(bytes, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.True(Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.False(Parser.Authorizer("""resource("file6"); allow if true;""").TryAuthorize(biscuit, out _));
    }

    [Fact]
    public void TestAttenuation()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")
            .EndBlock()            
            .Serialize();

        var token2 = Biscuit.Attenuate(token1)
            .AddBlock()
                .Add("""check if resource("file4")""")
                .Add("""check if resource("file5")""")
            .EndBlock()
            .Serialize();
        
        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(token2, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.True(Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.False(Parser.Authorizer("""resource("file6"); allow if true;""").TryAuthorize(biscuit, out _));
    }

    [Fact]
    public void Test_Sealed_Token_Without_Blocks_Should_Pass_Verification()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")
            .EndBlock()  
            .Seal();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.True(Parser.Authorizer("""check if resource("file4"); allow if true;""").TryAuthorize(biscuit, out _));        
    }

    [Fact]
    public void Test_Sealed_Token_With_Blocks_Should_Pass_Verification()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")
            .EndBlock()
            .AddBlock()
                .Add("""check if resource("file4")""")
                .Add("""check if resource("file5")""")
            .EndBlock()
            .Seal();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);
        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.True(Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.False(Parser.Authorizer("""resource("file6"); allow if true;""").TryAuthorize(biscuit, out _));
    }

    [Fact]
    public void Test_Sealed_Cant_be_attenuated()
    {
        var rootKey = Ed25519.NewSigningKey();
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")
            .EndBlock()
            .Seal()
            .ToArray();

        //todo better exception here 
        Assert.Throws<System.ArgumentException>(() => {Biscuit.Attenuate(token1);});
    }

    [Fact]
    public void Test_Third_Party_Block()
    {
        var rootKey = Ed25519.NewSigningKey();
        var thirdPartyKey = Ed25519.NewSigningKey();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);        
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")
            .EndBlock()
            .Serialize();

        var token2 = Biscuit.Attenuate(token1)
            .AddThirdPartyBlock(request => 
                //the request would usually be send to a third-party over the wire
                //the third party processes the requests, builds a third-party block, signs
                //it, it sends it back.
                //for the sake of the example, everything here happens in-process
                Biscuit.NewThirdParty()
                    .Add("""check if resource("file4")""")
                    .Add("""check if resource("file5")""")
                .Sign(thirdPartyKey, request)
            )
            .Serialize();

        /*
        var thirdPartySigner = new SignatureCreator();
        var thirdPartyBlock = new ThirdPartyBlockSigner(thirdPartyBlockRequest)
            .Add(parser.ParseCheck("""check if resource("file4")"""))
            .Add(parser.ParseCheck("""check if resource("file5")"""))
            .Sign(thirdPartySigner);

        var token2 = Biscuit.Attenuate(token1)
            .AddThirdPartyBlock(thirdPartyBlock)
            .Serialize();
        */
        if(!Biscuit.TryDeserialize(token2, verificationKey, out var biscuit, out var formatErr))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr}");
        }

        Assert.True(Parser.Authorizer("""resource("file5"); allow if true;""").TryAuthorize(biscuit, out _));
        Assert.False(Parser.Authorizer("""resource("file6"); allow if true;""").TryAuthorize(biscuit, out _));
    }

    [Fact]
    public void Test_Block_Trusting_Previous_Block()
    {
        var rootKey = Ed25519.NewSigningKey();       
        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);        
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
            .EndBlock()
            .AddBlock()
                .Add("""resource("file5");""")
            .EndBlock()
            .AddBlock()
                .Trusts(ScopeType.Previous)
                .Add("""check if resource("file5");""")
            .EndBlock()
            .Serialize();

        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit1, out var formatErr1))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr1}");
        }

        Assert.True(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit1, out _));
    }

    [Fact]
    public void Test_Block_Trusting_Third_Party_Block()
    {
        var rootKey = Ed25519.NewSigningKey();
        var thirdPartyKey = Ed25519.NewSigningKey();

        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
                .Add("resource", "file4")                
            .EndBlock()
            .AddBlock()
                //this block trusts any blocks signed by the thirdPartyKey
                //even if these blocks have been appended only later-on 
                .Trusts(thirdPartyKey.Public)
                .Add("""check if resource("file5");""")
            .EndBlock()
            .Serialize();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);                
        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit1, out var formatErr1))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr1}");
        }

        //check in block 1 should fail, since fact resource("file5") isn't there yet
        Assert.False(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit1, out _));

        //add the third party block, which block 1 trusts
        var token2 = Biscuit.Attenuate(token1)
            .AddThirdPartyBlock(request => 
                Biscuit.NewThirdParty()
                    .Add("""resource("file5");""")
                .Sign(thirdPartyKey, request)
            )
            .Serialize();
        
        if(!Biscuit.TryDeserialize(token2, verificationKey, out var biscuit2, out var formatErr2))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr2}");
        }

        //check in block 1 passes now, since fact is in third party block 
        Assert.True(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit2, out _));
    }

    [Fact]
    public void Test_Rule_Trusting_Previous_Block()
    {
        var rootKey = Ed25519.NewSigningKey();
        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);        
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
            .EndBlock()
            .AddBlock()
                .Add("""resource("file5");""")
            .EndBlock()
            .AddBlock()                
                .AddCheck(Check.CheckKind.One)
                    .AddRule("""resource("file5")""")
                        .Trusts(ScopeType.Previous)
                    .EndRule()
                .EndCheck()
            .EndBlock()
            .Serialize();

        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit1, out var formatErr1))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr1}");
        }

        Assert.True(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit1, out _));
    }

    [Fact]
    public void Test_Rule_Trusting_Third_Party_Block()
    {
        var rootKey = Ed25519.NewSigningKey();
        var thirdPartyKey = Ed25519.NewSigningKey();

        var verificationKey = new Ed25519.VerificationKey(rootKey.Public);        
        
        var token1 = Biscuit.New(rootKey)
            .AuthorityBlock()
            .EndBlock()
            .AddBlock()                
                .AddCheck(Check.CheckKind.One)
                    .AddRule("""resource("file5")""")
                        //while the overall block only trusts authority and the authorizer
                        //this rule also trusts blocks signed by the thirdPartyKey
                        .Trusts(thirdPartyKey.Public)
                    .EndRule()
                .EndCheck()
            .EndBlock()
            .Serialize();

        if(!Biscuit.TryDeserialize(token1, verificationKey, out var biscuit1, out var formatErr1))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr1}");
        }

        //check in block 1 should fail, since fact resource("file5") isn't there yet
        Assert.False(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit1, out _));

        //add the third party block, which block 1 trusts
        var token2 = Biscuit.Attenuate(token1)
            .AddThirdPartyBlock(request => 
                Biscuit.NewThirdParty()
                    .Add("""resource("file5");""")
                .Sign(thirdPartyKey, request)
            )
            .Serialize();
        
        if(!Biscuit.TryDeserialize(token2, verificationKey, out var biscuit2, out var formatErr2))
        {
            throw new Exception($"Couldn't round-trip biscuit: {formatErr2}");
        }

        //check in block 1 passes now, since fact is in third party block 
        Assert.True(Parser.Authorizer("""allow if true;""").TryAuthorize(biscuit2, out _));
    }
}