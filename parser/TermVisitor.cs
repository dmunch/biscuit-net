namespace parser;

using Antlr4.Runtime.Misc;
using biscuit_net.Proto;

public class TermVisitor : ExpressionsBaseVisitor<Op>
{
    public override Op VisitBooleanFactTerm([NotNull] ExpressionsParser.BooleanFactTermContext context) 
    { 
        var text = context.BOOLEAN().GetText();

        return new Op() 
        {
            Value = new TermV2()
            {
                Bool = bool.Parse(text)
            }
        };
    }
}
