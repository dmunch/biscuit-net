using biscuit_net;
using System.Text.RegularExpressions;
using parser;
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
    /*
    [InlineData("check if hex:12ab == hex:12ab;")]
    */
    [InlineData("check if [1, 2].contains(2);")]    
    [InlineData("check if [2019-12-04T09:46:41Z, 2020-12-04T09:46:41Z].contains(2020-12-04T09:46:41Z);")]
    [InlineData("check if [false, true].contains(true);")]
    [InlineData("check if [\"abc\", \"def\"].contains(\"abc\");")]
    /*
    [InlineData("check if [hex:12ab, hex:34de].contains(hex:34de);")]
    */
    public void Should_Parse(string expression)
    {
        var parser = new parser.Parser();
        var ops = parser.Parse(expression);

        Assert.True(ExpressionEvaluator.Evaluate(ops, v => throw new NotImplementedException()));
    }
}