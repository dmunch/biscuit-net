using biscuit_net.Datalog;
using A = biscuit_net.Datalog.Atom;
using R = biscuit_net.Datalog.RuleExpressions;

namespace tests;
public class VerifierTests
{
    record Block
    (
        IEnumerable<Atom> Atoms,
        IEnumerable<RuleExpressions> Rules,
        IEnumerable<Check> Checks,
        uint Version
    )  : IBlock;

    record Biscuit(IBlock Authority, IEnumerable<IBlock> Blocks) : IBiscuit;

    [Fact]
    public void Test()
    {
        var authority = new Block(
            new [] {
                new A("owner", "alice", "file1"),
                new A("owner", "alice", "file2"),
                new A("reader", "alice", "file3"),
                new A("reader", "bob", "file4"),
            },
            new [] {
                new R(
                    new A("right", "$1", "read"),
                    new A("user_id", "$0"),
                    new A("resource", "$1"),
                    new A("reader", "$0", "$1")
                ),
                new R(
                    new A("right", "$1", "read"), 
                    new A("user_id", "$0"), 
                    new A("resource", "$1"), 
                    new A("owner", "$0", "$1")
                ),
                new R(
                    new A("right", "$1", "read"), 
                    new A("user_id", "$0"), 
                    new A("resource", "$1"), 
                    new A("owner", "$0", "$1")
                ),
                new R(
                    new A("right", "$1", "write"), 
                    new A("user_id", "$0"), 
                    new A("resource", "$1"), 
                    new A("owner", "$0", "$1")
                )
            },
            new Check[] {
            },
            3
        );

        var biscuit = new Biscuit(authority, Enumerable.Empty<IBlock>());

        bool Verify(string user, string resource, string operation)
        {
            var world = new World(new HashSet<A>() {
            new A("resource", resource),
            new A("user_id", user),
            new A("operation", operation)
            }, new List<Check>(){
                new Check(
                    new R(
                        new A("check1"), 
                        new A("resource", "$0"),
                        new A("operation", "$1"),
                        new A("right", "$0", "$1")
                    )
                )
            });
            return Verifier.TryVerify(biscuit, world, out var error);

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