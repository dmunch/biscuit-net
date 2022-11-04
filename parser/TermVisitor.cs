namespace parser;

using Antlr4.Runtime.Misc;

public class TermVisitor : ExpressionsBaseVisitor<Op>
{
    public override Op VisitBooleanFactTerm([NotNull] ExpressionsParser.BooleanFactTermContext context) 
    { 
        var text = context.BOOLEAN().GetText();
        return new Op(new Boolean(bool.Parse(text)));
    }

    public override Op VisitNumberFactTerm([NotNull] ExpressionsParser.NumberFactTermContext context) 
    { 
        var text = context.NUMBER().GetText();
        return new Op(new Integer(long.Parse(text)));
    }

    public override Op VisitDateFactTerm([NotNull] ExpressionsParser.DateFactTermContext context) 
    { 
        var text = context.DATE().GetText();
        var dateParsed = DateTime.Parse(text);
        var dateTAI = Date.ToTAI64(dateParsed);
        return new Op(new Date(dateTAI));
    }

    public override Op VisitStringFactTerm([NotNull] ExpressionsParser.StringFactTermContext context) 
    { 
        var text = context.STRING().GetText();
        return new Op(new String(text.Trim('"')));
    }

    public override Op VisitSetFactTerm([NotNull] ExpressionsParser.SetFactTermContext context) 
    { 
        if(context.set().fact_term() == null)
        {
            //empty list
            return new Op(new Set(new List<VeryNaiveDatalog.Constant>()));    
        }

        var firstTerm = base.Visit(context.set().fact_term()).Value as VeryNaiveDatalog.Constant;
        var nextTerms = context.set().set_term().Select(st => base.Visit(st).Value as VeryNaiveDatalog.Constant).ToList();

        var terms = new List<VeryNaiveDatalog.Constant>();
        terms.Add(firstTerm);
        terms.AddRange(nextTerms);
        return new Op(new Set(terms));
    }
}