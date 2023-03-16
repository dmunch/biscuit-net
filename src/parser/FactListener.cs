using Antlr4.Runtime.Misc;

namespace biscuit_net.Parser;
using Datalog;

public class FactListener : DatalogBaseListener
{
    readonly TermVisitor _termVisitor = new();
    
    public List<Fact> Facts { get; } = new();

    public override void ExitFact([NotNull] DatalogParser.FactContext context) 
    {
        var name = context.NAME().GetText();
        
        var terms = context.fact_term().Select(t => _termVisitor.Visit(t)).ToList();

        Facts.Add(new Fact(name, terms));
    }
}