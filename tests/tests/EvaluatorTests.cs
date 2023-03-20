using biscuit_net.Datalog;
using biscuit_net.Parser;

namespace tests;

public class EvaluatorTests
{
    [Fact]
    public void TestRulesWithMultipleResults()
    {
        var parser = new Parser();
        
        var code = """
            resource("file1");
            resource("file2");
            operation("read");
            operation("write");
            opi($0, $1) <- resource($0), operation($1);
        """;

        var block = parser.ParseAuthorizer(code);
        
        var facts = block.Facts.Evaluate(block.Rules.First());

        var assertCode = """
            opi("file1", "read");
            opi("file1", "write");
            opi("file2", "read");
            opi("file2", "write");
        """;
        var assertBlock = parser.ParseAuthorizer(assertCode);
        Assert.Equal(assertBlock.Facts.ToHashSet(), facts);
    }

    [Fact]
    public void TestMethodOnVariable()
    {
        var parser = new Parser();
        
        var code = """
            role("role1", ["permission1_1", "permissions1_2"]);
            role("role2", ["permission2_1", "permissions2_2"]);
            
            role_with_permissions2_2($role) <- role($role, $permissions), $permissions.contains("permissions2_2");
        """;

        var block = parser.ParseAuthorizer(code);        
        var facts = block.Facts.Evaluate(block.Rules.First());

        var assertCode = """
            role_with_permissions2_2("role2");
        """;
        var assertBlock = parser.ParseAuthorizer(assertCode);
        Assert.Equal(assertBlock.Facts.ToHashSet(), facts);
    }
}