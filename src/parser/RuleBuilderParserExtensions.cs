namespace biscuit_net.Parser;
using biscuit_net.Builder;
using biscuit_net.Datalog;

public static class RuleBuilderParserExtensions
{
    public static CheckRuleBuilder AddRule(this CheckBuilder builder, string ruleCode)
    {
        var parser = new Parser();

        var rule = parser.ParseHeadlessRule(ruleCode, new Fact("check1"));
        return new CheckRuleBuilder(builder, rule);
    }

    public static BlockRuleBuilder AddRule(this BlockBuilder builder, string ruleCode)
    {
        var parser = new Parser();

        var rule = parser.ParseRule(ruleCode);

        return new BlockRuleBuilder(builder, rule);
    }
}