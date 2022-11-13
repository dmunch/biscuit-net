using biscuit_net;
using biscuit_net.Datalog;
using F = biscuit_net.Datalog.Fact;
using R = biscuit_net.RuleConstrained;

namespace tests;
public class VerifierTests
{
    record Block
    (
        IEnumerable<Fact> Facts,
        IEnumerable<IRuleConstrained> Rules,
        IEnumerable<Check> Checks,
        uint Version,
        Scope Scope,
        PublicKey? SignedBy
    )  : IBlock;

    record Biscuit(IBlock Authority, IReadOnlyCollection<IBlock> Blocks) : IBiscuit;

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
            3,
            Scope.DefaultBlockScope,
            null
        );

        var biscuit = new Biscuit(authority, Array.Empty<IBlock>());

        bool Verify(string user, string resource, string operation)
        {
            var authorizerBlock = new AuthorizerBlock();
            authorizerBlock.Add(new F("resource", resource));
            authorizerBlock.Add(new F("user_id", user));
            authorizerBlock.Add(new F("operation", operation));

            authorizerBlock.Add(new Check(
                new R(
                    new F("check1"), 
                    new F("resource", "$0"),
                    new F("operation", "$1"),
                    new F("right", "$0", "$1")
                )
            )
            );

            var factSet = new FactSet();
            var ruleSet = new RuleSet();
            var world = new World( 
                factSet,
                ruleSet
            );
            return Verifier.TryVerify(biscuit, world, authorizerBlock, out var error);

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
}