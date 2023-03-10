namespace biscuit_net.Parser;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using biscuit_net.Expressions;

public class Parser
{
    private readonly ExpressionsVisitor _expressionsVisitor = new ExpressionsVisitor();
    public List<Op> Parse(string rule)
    {
        var parser = InitializeParser(rule, out _);
        
        return parser.check().Accept(_expressionsVisitor);
    }

    public Datalog.RuleConstrained ParseRule(string ruleString)
    {
        var parser = InitializeParser(ruleString, out _);

        var ruleListener = new RuleListener();
        ParseTreeWalker.Default.Walk(ruleListener, parser.check());

        return ruleListener.GetRule();
    }

    public AuthorizerBlock ParseAuthorizer(string authorizerBlock)
    {
        var parser = InitializeParser(authorizerBlock, out _);

        var listener = new AuthorizerListener();
        ParseTreeWalker.Default.Walk(listener, parser.authorizer());
        
        return listener.GetAuthorizerBlock();
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