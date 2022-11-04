namespace parser;

using Antlr4.Runtime.Misc;
using VeryNaiveDatalog;

public class TermVisitor : ExpressionsBaseVisitor<Term>
{
    public override Term VisitBooleanFactTerm([NotNull] ExpressionsParser.BooleanFactTermContext context) 
    { 
        var text = context.BOOLEAN().GetText();
        return new Boolean(bool.Parse(text));
    }

    public override Term VisitNumberFactTerm([NotNull] ExpressionsParser.NumberFactTermContext context) 
    { 
        var text = context.NUMBER().GetText();
        return new Integer(long.Parse(text));
    }

    public override Term VisitDateFactTerm([NotNull] ExpressionsParser.DateFactTermContext context) 
    { 
        var text = context.DATE().GetText();
        var dateParsed = DateTime.Parse(text);
        var dateTAI = Date.ToTAI64(dateParsed);
        return new Date(dateTAI);
    }

    public override Term VisitStringFactTerm([NotNull] ExpressionsParser.StringFactTermContext context) 
    { 
        var text = context.STRING().GetText();
        return new String(text.Trim('"'));
    }

    public override Term VisitBytesFactTerm([NotNull] ExpressionsParser.BytesFactTermContext context) 
    { 
        var text = context.BYTES().GetText();
        var bytes = Convert.FromHexString(text.Substring("hex:".Length));
        return new Bytes(bytes);
    }

    public override Term VisitSetFactTerm([NotNull] ExpressionsParser.SetFactTermContext context) 
    { 
        if(context.set().fact_term() == null)
        {
            //empty list
            return new Set(new List<VeryNaiveDatalog.Term>());
        }

        var firstTerm = base.Visit(context.set().fact_term()) as VeryNaiveDatalog.Constant;
        var nextTerms = context.set().set_term().Select(st => base.Visit(st) as VeryNaiveDatalog.Constant).ToList();

        var terms = new List<VeryNaiveDatalog.Term>();
        terms.Add(firstTerm);
        terms.AddRange(nextTerms);
        return new Set(terms);
    }
    
    public override Term VisitExpressionVariable([NotNull] ExpressionsParser.ExpressionVariableContext context) 
    { 
        return new Variable(context.VARIABLE().GetText().Trim('$'));
    }

    
    public override Term VisitTerm([NotNull] ExpressionsParser.TermContext context) 
    { 
        if(context.VARIABLE() != null)
            return new Variable(context.VARIABLE().GetText().Trim('$'));

        return base.Visit(context.fact_term());
    }
}