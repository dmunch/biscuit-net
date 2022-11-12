using System.Text.RegularExpressions;

using biscuit_net;
using biscuit_net.Expressions;
using biscuit_net.Parser;
using biscuit_net.Datalog;

namespace tests;
public class ParserTests
{
    [Theory]
    [InlineData("check if !false;")]
    [InlineData("check if !false && true;")]
    [InlineData("check if false or true;")]
    [InlineData("check if (true || false) && true;")]
    [InlineData("check if 1 < 2;")]
    [InlineData("check if 2 > 1;")]
    [InlineData("check if 1 <= 2;")]
    [InlineData("check if 1 <= 1;")]
    [InlineData("check if 2 >= 1;")]
    [InlineData("check if 2 >= 2;")]
    [InlineData("check if 3 == 3;")]
    [InlineData("check if 1 + 2 * 3 - 4 / 2 == 5;")]    
    [InlineData("check if \"hello world\".starts_with(\"hello\") && \"hello world\".ends_with(\"world\");")]    
    [InlineData("check if \"aaabde\".matches(\"a*c?.e\");")]
    [InlineData("check if \"aaabde\".contains(\"abd\");")]    
    [InlineData("check if \"aaabde\" == \"aaa\" + \"b\" + \"de\";")]
    [InlineData("check if \"abcD12\" == \"abcD12\";")]
    [InlineData("check if 2019-12-04T09:46:41Z < 2020-12-04T09:46:41Z;")]
    [InlineData("check if 2020-12-04T09:46:41Z > 2019-12-04T09:46:41Z;")]
    [InlineData("check if 2019-12-04T09:46:41Z <= 2020-12-04T09:46:41Z;")]
    [InlineData("check if 2020-12-04T09:46:41Z >= 2020-12-04T09:46:41Z;")]
    [InlineData("check if 2020-12-04T09:46:41Z >= 2019-12-04T09:46:41Z;")]
    [InlineData("check if 2020-12-04T09:46:41Z >= 2020-12-04T09:46:41Z;")]
    [InlineData("check if 2020-12-04T09:46:41Z == 2020-12-04T09:46:41Z;")]
    [InlineData("check if hex:12ab == hex:12ab;")]
    [InlineData("check if [1, 2].contains(2);")]    
    [InlineData("check if [2019-12-04T09:46:41Z, 2020-12-04T09:46:41Z].contains(2020-12-04T09:46:41Z);")]
    [InlineData("check if [false, true].contains(true);")]
    [InlineData("check if [\"abc\", \"def\"].contains(\"abc\");")]
    [InlineData("check if [hex:12ab, hex:34de].contains(hex:34de);")]
    public void Should_Parse(string expression)
    {
        var parser = new Parser();
        var ops = parser.Parse(expression);

        Assert.True(biscuit_net.Expressions.Evaluator.Evaluate(ops, v => throw new NotImplementedException()));
    }

    [Fact]
    public void Should_Parse_Rule_1()
    {
        var parser = new Parser();
        var rule = parser.ParseRule("check if right($0, $1), resource($0), operation($1)");

        Assert.Equal(new RuleConstrained(
            new Fact("check1"), 
            new List<Fact>
            {
                new Fact("right", new Variable("0"), new Variable("1")),
                new Fact("resource", new Variable("0")),
                new Fact("operation", new Variable("1")),
            }, 
            new List<Expression>()
        ), rule);
    }

    [Fact]
    public void Should_Parse_Rule_With_Or()
    {
        var parser = new Parser();
        var rule = parser.ParseRule("check if must_be_present($0) or must_be_present($0)");

        Assert.Equal(new RuleConstrained(
            new Fact("check1"), 
            new []
            {
                new Fact("must_be_present", new Variable("0")),
                new Fact("must_be_present", new Variable("0"))
            }, 
            Enumerable.Empty<Expression>()
        ), rule);
    }

    [Fact]
    public void Should_Parse_Rule_With_Default_Symbols()
    {
        var parser = new Parser();
        var check = "check if read(0), write(1), resource(2), operation(3), right(4), time(5), role(6), owner(7), tenant(8), namespace(9), user(10), team(11), service(12), admin(13), email(14), group(15), member(16), ip_address(17), client(18), client_ip(19), domain(20), path(21), version(22), cluster(23), node(24), hostname(25), nonce(26), query(27)";
        var rule = parser.ParseRule(check);

        var tokens = check.Split(", ");
        tokens[0] = tokens[0].Remove(0, "check if ".Length);

        string intTermPattern = @"^([a-zA-Z_]+)\((\d+)\)$";
        var intTermRegex = new Regex(intTermPattern);

        var facts = new List<Fact>();
        foreach(var token in tokens)
        {
            var match = intTermRegex.Match(token.Trim(';'));
            var name = match.Groups[1];
            var intValueString = match.Groups[2];
            
            Assert.True(int.TryParse(intValueString.Value, out var intValue));
            facts.Add(new Fact(name.Value, new Integer(intValue)));
        }
        
        Assert.Equal(new RuleConstrained(
            new Fact("check1"), 
            facts,
            Enumerable.Empty<Expression>()
        ), rule);
    }

    [Fact]
    public void Should_Parse_Rule_With_Funny_Characters()
    {
        var parser = new Parser();
        var rule = parser.ParseRule("check if ns::fact_123(\"hello é\t😁\");");

        
        Assert.Equal(new RuleConstrained(
            new Fact("check1"), 
            new []
            {
                new Fact("ns::fact_123", new biscuit_net.Datalog.String("hello é\t😁"))
            }, 
            Enumerable.Empty<Expression>()
        ), rule);
    }
}