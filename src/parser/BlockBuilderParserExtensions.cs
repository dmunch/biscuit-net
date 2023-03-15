namespace biscuit_net.Parser;
using biscuit_net.Builder;
using biscuit_net.Datalog;

public static class BlockBuilderParserExtensions
{
    public static BlockBuilder Add(this BlockBuilder builder, string factName, params Term[] terms) 
    {
        return builder.Add(new Fact(factName, terms));
    }

    public static BlockBuilder Add(this BlockBuilder builder, string blockCode) 
    {
        var parser = new Parser();

        var block = parser.ParseBlock(blockCode);
        foreach(var fact in block.Facts)
        {
            builder.Add(fact);
        }
        foreach(var rule in block.Rules)
        {
            builder.Add(rule);
        }
        foreach(var check in block.Checks)
        {
            builder.Add(check);
        }
        return builder;
    }

    public static ThirdPartyBlockBuilder Add(this ThirdPartyBlockBuilder builder, string blockCode) 
    {
        var parser = new Parser();

        var block = parser.ParseBlock(blockCode);
        foreach(var fact in block.Facts)
        {
            builder.Add(fact);
        }
        foreach(var rule in block.Rules)
        {
            builder.Add(rule);
        }
        foreach(var check in block.Checks)
        {
            builder.Add(check);
        }
        return builder;
    }

    public static BlockBuilder AuthorityBlock(this BiscuitBuilder builder, string authorityCode)
    {
        return builder.AuthorityBlock().Add(authorityCode);
    }

    public static BlockBuilder AddBlock(this IBiscuitBuilder builder, string blockCode)
    {
        return builder.AddBlock().Add(blockCode);
    }
}