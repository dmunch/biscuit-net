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
    public void Should_Parse_Rule()
    {
        var text = "rule($rulevar) <- fact($rulevar)";
            
        var parser = new Parser();
        var rule = parser.ParseRule(text);

        Assert.Equal(new Rule(
            new Fact("rule", new Variable("rulevar")), 
            new List<Fact>
            {
                new Fact("fact", new Variable("rulevar"))                
            }, 
            new List<Expression>(),
            Scope.DefaultBlockScope
        ), rule);
    }

    [Fact]
    public void Should_Parse_Check_1()
    {
        var parser = new Parser();
        var check = parser.ParseCheck("check if right($0, $1), resource($0), operation($1)");

        Assert.Equal(new Rule(
            new Fact("check1"), 
            new List<Fact>
            {
                new Fact("right", new Variable("0"), new Variable("1")),
                new Fact("resource", new Variable("0")),
                new Fact("operation", new Variable("1")),
            }, 
            new List<Expression>(),
            Scope.DefaultBlockScope
        ), check.Rules.First());
    }

    [Fact]
    public void Should_Parse_Check_With_Or()
    {
        var parser = new Parser();
        var check = parser.ParseCheck("check if must_be_present($0) or must_be_present($1)");

        Assert.Equal(new Check(new[]{
            new Rule(
                new Fact("check1"), 
                new []
                {
                    new Fact("must_be_present", new Variable("0"))
                }, 
                Enumerable.Empty<Expression>(),
                Scope.DefaultBlockScope
            ),
            new Rule(
                new Fact("check1"), 
                new []
                {
                    new Fact("must_be_present", new Variable("1"))
                }, 
                Enumerable.Empty<Expression>(),
                Scope.DefaultBlockScope
            )
        }, Check.CheckKind.One)
        , check);
    }

    [Fact]
    public void Should_Parse_Check_With_Default_Symbols()
    {
        var parser = new Parser();
        var code = "check if read(0), write(1), resource(2), operation(3), right(4), time(5), role(6), owner(7), tenant(8), namespace(9), user(10), team(11), service(12), admin(13), email(14), group(15), member(16), ip_address(17), client(18), client_ip(19), domain(20), path(21), version(22), cluster(23), node(24), hostname(25), nonce(26), query(27)";
        var check = parser.ParseCheck(code);

        var tokens = code.Split(", ");
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
        
        Assert.Equal(new Rule(
            new Fact("check1"), 
            facts,
            Enumerable.Empty<Expression>(),
            Scope.DefaultBlockScope
        ), check.Rules.First());
    }

    [Fact]
    public void Should_Parse_Check_With_Funny_Characters()
    {
        var parser = new Parser();
        var check = parser.ParseCheck("check if ns::fact_123(\"hello é\t😁\");");

        
        Assert.Equal(new Rule(
            new Fact("check1"), 
            new []
            {
                new Fact("ns::fact_123", new biscuit_net.Datalog.String("hello é\t😁"))
            }, 
            Enumerable.Empty<Expression>(),
            Scope.DefaultBlockScope
        ), check.Rules.First());
    }

    [Fact]
    public void Should_Parse_Authorizer_Facts()
    {
        var text = "resource(\"file1\"); allow if true;";
        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);
    }

    [Fact]
    public void Should_Parse_Authorizer_Facts_2()
    {
        var text = @"resource(""file1"");
            operation(""read"");
            time(2020-12-21T09:23:12Z);

            allow if true;";

        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);
    }


    [Fact]
    public void Should_Parse_Authorizer_Checks_And_Policies_With_Public_Keys()
    {
        var text = @"check if query(1, 2) trusting ed25519/3c8aeced6363b8a862552fb2b0b4b8b0f8244e8cef3c11c3e55fd553f3a90f59, ed25519/ecfb8ed11fd9e6be133ca4dd8d229d39c7dcb2d659704c39e82fd7acf0d12dee;
                    deny if query(3);
                    deny if query(1, 2);
                    deny if query(0) trusting ed25519/3c8aeced6363b8a862552fb2b0b4b8b0f8244e8cef3c11c3e55fd553f3a90f59;
                    allow if true;";
            
        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);
    }

    [Fact]
    public void Should_Parse_Authorizer_Checks_And_Policies_Without_Public_Keys()
    {
        var text = "deny if true; allow if false;";
            
        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);
    }

    [Fact]
    public void Should_Parse_Authorizer_Single_Rule()
    {
        var text = "rule($rulevar) <- fact($rulevar);";
            
        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);

        Assert.Equal(1, block.Rules.Count());

        var rule = block.Rules.First();
        
        Assert.Equal(new Rule(
            new Fact("rule", new Variable("rulevar")), 
            new List<Fact>
            {
                new Fact("fact", new Variable("rulevar"))                
            }, 
            new List<Expression>(),
            Scope.DefaultBlockScope
        ), rule);
    }

    [Fact]
    public void Should_Parse_Authorizer_Multiple_Rules()
    {
        var text = "rule1($rule1var) <- fact1($rule1var); rule2($rule2var) <- fact2($rule2var);";
            
        var parser = new Parser();
        var block = parser.ParseAuthorizer(text);

        Assert.Equal(2, block.Rules.Count());

        var rule1 = block.Rules.First();
        
        Assert.Equal(new Rule(
            new Fact("rule1", new Variable("rule1var")), 
            new List<Fact>
            {
                new Fact("fact1", new Variable("rule1var"))                
            }, 
            new List<Expression>(),
            Scope.DefaultBlockScope
        ), rule1);

        var rule2 = block.Rules.ElementAt(1);
        
        Assert.Equal(new Rule(
            new Fact("rule2", new Variable("rule2var")), 
            new List<Fact>
            {
                new Fact("fact2", new Variable("rule2var"))                
            }, 
            new List<Expression>(),
            Scope.DefaultBlockScope
        ), rule2);
    }
}