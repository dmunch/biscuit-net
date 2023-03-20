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
    
    public List<Op> ParseExpression(string expression)
    {
        var parser = InitializeParser(expression, out _);
        var expressionsVisitor = new ExpressionsVisitor();;        
        return parser.expression().Accept(expressionsVisitor);
    }

    public Check ParseCheck(string ruleString)
    {
        var parser = InitializeParser(ruleString, out _);

        var listener = new AuthorizerListener();
        ParseTreeWalker.Default.Walk(listener, parser.check());

        return listener.GetAuthorizerBlock().Checks.First();;
        //return ruleListener.GetHeadlessRule(new Datalog.Fact("check1"));
    }

    public RuleScoped ParseHeadlessRule(string ruleString, Datalog.Fact head)
    {
        var parser = InitializeParser(ruleString, out _);

        var listener = new RuleBodyListener();
        ParseTreeWalker.Default.Walk(listener, parser.rule_body());

        return listener.GetHeadlessRule(head);
    }

    public RuleScoped ParseRule(string ruleString)
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

    DatalogParser InitializeParser(string code, out ErrorListener errorListener)
    {
        var charStream = new AntlrInputStream(code);
        var lexer = new DatalogLexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new DatalogParser(tokenStream);
        
        errorListener = new ErrorListener();
        parser.AddErrorListener(errorListener);

        return parser;
    }
}