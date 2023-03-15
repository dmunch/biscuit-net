namespace biscuit_net.Parser;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using biscuit_net.Expressions;

public class Parser
{
    public static Authorizer Authorizer(string code)
    {
        var parser = new Parser();
        return new Authorizer(parser.ParseAuthorizer(code));
    }
    
    public static Block Block(string code)
    {
        var parser = new Parser();
        return parser.ParseBlock(code);
    }

    private readonly ExpressionsVisitor _expressionsVisitor = new();
    public List<Op> Parse(string rule)
    {
        var parser = InitializeParser(rule, out _);
        
        return parser.check().Accept(_expressionsVisitor);
    }

    public Check ParseCheck(string ruleString)
    {
        var parser = InitializeParser(ruleString, out _);

        var listener = new AuthorizerListener();
        ParseTreeWalker.Default.Walk(listener, parser.check());

        return listener.GetAuthorizerBlock().Checks.First();;
        //return ruleListener.GetHeadlessRule(new Datalog.Fact("check1"));
    }

    public Datalog.Rule ParseHeadlessRule(string ruleString, Datalog.Fact head)
    {
        var parser = InitializeParser(ruleString, out _);

        var listener = new RuleBodyListener();
        ParseTreeWalker.Default.Walk(listener, parser.rule_body());

        return listener.GetHeadlessRule(head);
    }

    public Datalog.Rule ParseRule(string ruleString)
    {
        var parser = InitializeParser(ruleString, out _);

        var ruleListener = new RuleBodyListener();
        ParseTreeWalker.Default.Walk(ruleListener, parser.rule_());

        return ruleListener.GetRule();
    }

    public Datalog.Fact ParseFact(string factString)
    {
        var parser = InitializeParser(factString, out _);

        var factListener = new FactListener();
        ParseTreeWalker.Default.Walk(factListener, parser.fact());

        return factListener.Facts.First();
    }

    public AuthorizerBlock ParseAuthorizer(string authorizerBlock)
    {
        var parser = InitializeParser(authorizerBlock, out _);

        var listener = new AuthorizerListener();
        ParseTreeWalker.Default.Walk(listener, parser.authorizer());
        
        return listener.GetAuthorizerBlock();
    }

    public Block ParseBlock(string block)
    {
        var parser = InitializeParser(block, out _);

        var listener = new AuthorizerListener();
        ParseTreeWalker.Default.Walk(listener, parser.block());
        
        return listener.GetBlock();
    }

    ExpressionsParser InitializeParser(string code, out ErrorListener errorListener)
    {
        var charStream = new AntlrInputStream(code);
        var lexer = new ExpressionsLexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ExpressionsParser(tokenStream);
        
        errorListener = new ErrorListener();
        parser.AddErrorListener(errorListener);

        return parser;
    }
}