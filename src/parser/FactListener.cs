using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;
using Datalog;
using Expressions;

public class FactListener : ExpressionsBaseListener
{
    TermVisitor _termVisitor = new TermVisitor();
    
    public List<Fact> Facts { get; } = new List<Fact>();
    List<Expression> _expressions = new List<Expression>();

    public override void ExitFact([NotNull] ExpressionsParser.FactContext context) 
    {
        var name = context.NAME().GetText();
        
        var terms = context.fact_term().Select(t => _termVisitor.Visit(t)).ToList();

        Facts.Add(new Fact(name, terms));
    }
}