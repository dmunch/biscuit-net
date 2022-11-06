namespace biscuit_net.Parser;

using Antlr4.Runtime;
using biscuit_net.Datalog;
using biscuit_net.Expressions;

public class Parser
{
    private readonly ExpressionsVisitor _expressionsVisitor = new ExpressionsVisitor();
    public List<Op> Parse(string rule)
    {
        var charStream = new AntlrInputStream(rule);
        var lexer = new ExpressionsLexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ExpressionsParser(tokenStream);
        
        parser.AddErrorListener(new ErrorListener());
        var tree = parser.check();

        var errors = parser.NumberOfSyntaxErrors;

        var ops = tree.Accept(_expressionsVisitor);
        
        return ops;
    }

    public RuleExpressions ParseRule(string ruleString)
    {
        var charStream = new AntlrInputStream(ruleString);
        var lexer = new ExpressionsLexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ExpressionsParser(tokenStream);
        
        parser.AddErrorListener(new ErrorListener());

        var ruleListener = new RuleListener();
        parser.AddParseListener(ruleListener);
        var tree = parser.check();

        var errors = parser.NumberOfSyntaxErrors;
        
        return ruleListener.GetRuleExpressions();
    }
}