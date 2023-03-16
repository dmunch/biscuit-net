namespace biscuit_net.Parser;
using Datalog;

using Antlr4.Runtime.Misc;

public class TermVisitor : DatalogBaseVisitor<Term>
{
    public override Term VisitBooleanFactTerm([NotNull] DatalogParser.BooleanFactTermContext context) 
    { 
        var text = context.BOOLEAN().GetText();
        return new Boolean(bool.Parse(text));
    }

    public override Term VisitNumberFactTerm([NotNull] DatalogParser.NumberFactTermContext context) 
    { 
        var text = context.NUMBER().GetText();
        return new Integer(long.Parse(text));
    }

    public override Term VisitDateFactTerm([NotNull] DatalogParser.DateFactTermContext context) 
    { 
        var text = context.DATE().GetText();
        var dateParsed = DateTime.Parse(text);
        var dateTAI = Date.ToTAI64(dateParsed);
        return new Date(dateTAI);
    }

    public override Term VisitStringFactTerm([NotNull] DatalogParser.StringFactTermContext context) 
    { 
        var text = context.STRING().GetText();
        return new String(text.Trim('"'));
    }

    public override Term VisitBytesFactTerm([NotNull] DatalogParser.BytesFactTermContext context) 
    { 
        var text = context.BYTES().GetText();
        var bytes = HexConvert.FromHexString(text["hex:".Length..]);
        return new Bytes(bytes);
    }

    public override Term VisitSetFactTerm([NotNull] DatalogParser.SetFactTermContext context) 
    { 
        if(context.set().fact_term() == null)
        {
            //empty list
            return new Set(new List<Term>());
        }

        var firstTerm = base.Visit(context.set().fact_term());// as Constant;
        var nextTerms = context.set().set_term().Select(st => base.Visit(st)/* as Constant*/).ToList();

        var terms = new List<Term>
        {
            firstTerm
        };
        terms.AddRange(nextTerms);
        return new Set(terms);
    }
    
    public override Term VisitExpressionVariable([NotNull] DatalogParser.ExpressionVariableContext context) 
    { 
        return new Variable(context.VARIABLE().GetText().Trim('$'));
    }

    
    public override Term VisitTerm([NotNull] DatalogParser.TermContext context) 
    { 
        if(context.VARIABLE() != null)
            return new Variable(context.VARIABLE().GetText().Trim('$'));

        return base.Visit(context.fact_term());
    }
}