using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using biscuit_net.Datalog;
using A = biscuit_net.Datalog.Fact;
using R = biscuit_net.Datalog.RuleExpressions;


var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

public static class Utils
{
    public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] List)
    {
        foreach (IEnumerable<T> element in List)
        {
            foreach (T subelement in element)
            {
                yield return subelement;
            }
        }
    }
}

[MemoryDiagnoser]
public class VerifierTests

{
    record Block
    (
        IEnumerable<Fact> Facts,
        IEnumerable<RuleExpressions> Rules,
        IEnumerable<Check> Checks
    )  : IBlock;

    record Biscuit(IBlock Authority, IEnumerable<IBlock> Blocks) : IBiscuit;

    Biscuit _biscuit;

    public VerifierTests()
    {
        var authority = new Block(
            Utils.Concatenate(
                Enumerable.Range(0, 1000).Select(i => new Fact("owner", "alice", $"file{i}")),
                Enumerable.Range(1001, 2000).Select(i => new Fact("reader", "alice", $"file{i}")),
                Enumerable.Range(0, 100).Select(i => new Fact("reader", "bob", $"file{i}"))
            ),
            /*
            new [] {
                new A("owner", "alice", "file1"),
                new A("owner", "alice", "file2"),
                new A("reader", "alice", "file3"),
                new A("reader", "bob", "file4"),
            },*/
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
            }
        );

        _biscuit = new Biscuit(authority, Enumerable.Empty<IBlock>());
    }
    
    [Benchmark]
    public void Test()
    {
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
            return Verifier.TryVerify(_biscuit, world, out var error);
        }
        
        var r = new Random();
        for(int i = 0; i < 10; i++)
        {
            var f = r.NextInt64(2000);
            Verify("alice", $"file{f}", "write");
            Verify("alice", $"file{f}", "read");
            Verify("bob", $"file{f}", "write");
            Verify("bob", $"file{f}", "read");
        }
    }
}