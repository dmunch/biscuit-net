using biscuit_net;
using biscuit_net.Datalog;
using F = biscuit_net.Datalog.Fact;
using R = biscuit_net.RuleConstrained;

namespace tests;
public class VerifierTests
{
    record Biscuit(Block Authority, IReadOnlyCollection<Block> Blocks);

    [Fact]
    public void Test()
    {
        var authority = new Block(
            new [] {
                new F("owner", "alice", "file1"),
                new F("owner", "alice", "file2"),
                new F("reader", "alice", "file3"),
                new F("reader", "bob", "file4"),
            },
            new [] {
                new R(
                    new F("right", "$1", "read"),
                    new F("user_id", "$0"),
                    new F("resource", "$1"),
                    new F("reader", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "read"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "read"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "write"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                )
            },
            new Check[] {
            },
            3            
        );

        var biscuit = new Biscuit(authority, Array.Empty<Block>());

        bool Verify(string user, string resource, string operation)
        {
            var authorizerBlock = new AuthorizerBlock()
                .Add(new F("resource", resource))
                .Add(new F("user_id", user))
                .Add(new F("operation", operation))
                .Add(new Check(
                        new R(
                            new F("check1"), 
                            new F("resource", "$0"),
                            new F("operation", "$1"),
                            new F("right", "$0", "$1")
                        )
                    )
                )
                .Add(Policy.AllowPolicy);

            var factSet = new FactSet();
            var ruleSet = new RuleSet();
            var world = new World( 
                factSet,
                ruleSet
            );
            return Verifier.TryVerify(biscuit.Authority, biscuit.Blocks, world, authorizerBlock, out var error);

        }
        Assert.True(Verify("alice", "file1", "write"));
        Assert.True(Verify("alice", "file2", "write"));
        Assert.False(Verify("alice", "file3", "write"));

        Assert.True(Verify("alice", "file1", "read"));
        Assert.True(Verify("alice", "file2", "read"));
        Assert.True(Verify("alice", "file3", "read"));

        Assert.False(Verify("bob", "file1", "write"));
        Assert.False(Verify("bob", "file2", "write"));
        Assert.False(Verify("bob", "file3", "write"));

        Assert.False(Verify("bob", "file1", "read"));
        Assert.False(Verify("bob", "file2", "read"));
        Assert.False(Verify("bob", "file3", "read"));

        Assert.True(Verify("bob", "file4", "read"));
    }

    [Fact]
    public void Test_Deny_And_Allow_Policies()
    {
        var authority = new Block(
            new [] {
                new F("owner", "alice", "file1"),
                new F("owner", "alice", "file2"),
                new F("reader", "alice", "file3"),
                new F("reader", "bob", "file4"),
            },
            new [] {
                new R(
                    new F("right", "$1", "read"),
                    new F("user_id", "$0"),
                    new F("resource", "$1"),
                    new F("reader", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "read"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "read"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                ),
                new R(
                    new F("right", "$1", "write"), 
                    new F("user_id", "$0"), 
                    new F("resource", "$1"), 
                    new F("owner", "$0", "$1")
                )
            },
            new Check[] {
            },
            3
        );

        var biscuit = new Biscuit(authority, Array.Empty<Block>());

        
        var authorizerBlockAllow = new AuthorizerBlock()
            .Add(new F("resource", "file1"))
            .Add(new F("user_id", "alice"))
            .Add(new F("operation", "write"))
            .Add(new Policy(PolicyKind.Allow, new [] { 
                new R(
                        new F("check1"), 
                        new F("resource", "$0"),
                        new F("operation", "$1"),
                        new F("right", "$0", "$1")
                    )
            }));

        var authorizerBlockDeny = new AuthorizerBlock()
            .Add(new F("resource", "file1"))
            .Add(new F("user_id", "alice"))
            .Add(new F("operation", "write"))
            .Add(new Policy(PolicyKind.Deny, new [] { 
                new R(
                        new F("check1"), 
                        new F("resource", "$0"),
                        new F("operation", "$1"),
                        new F("right", "$0", "$1")
                    )
            }));

        var authorizerBlockNoMatchingPolicy = new AuthorizerBlock()
            .Add(new F("resource", "file666"))
            .Add(new F("user_id", "alice"))
            .Add(new F("operation", "write"))
            .Add(new Policy(PolicyKind.Deny, new [] { 
                new R(
                        new F("check1"), 
                        new F("resource", "$0"),
                        new F("operation", "$1"),
                        new F("right", "$0", "$1")
                    )
            }));
        
        var worldAllow = new World();
        var worldDeny = new World();
        var worldNoMatchingPolicy = new World();

        Assert.True(Verifier.TryVerify(biscuit.Authority, biscuit.Blocks, worldAllow, authorizerBlockAllow, out var _));
        Assert.False(Verifier.TryVerify(biscuit.Authority, biscuit.Blocks, worldDeny, authorizerBlockDeny, out var _));
        Assert.False(Verifier.TryVerify(biscuit.Authority, biscuit.Blocks, worldNoMatchingPolicy, authorizerBlockNoMatchingPolicy, out var errorNoMatchingPolicy));

        Assert.Equal(new Error(new FailedLogic(new NoMatchingPolicy())), errorNoMatchingPolicy);
    }
}