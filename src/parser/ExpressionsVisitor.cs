namespace biscuit_net.Parser;

using Antlr4.Runtime.Misc;
using biscuit_net.Expressions;

public class ExpressionsVisitor : DatalogBaseVisitor<List<Op>>
{
    readonly TermVisitor _termVisitor = new();
    
    public override List<Op> VisitExpressionParentheses([NotNull] DatalogParser.ExpressionParenthesesContext context) 
    {
        return base.Visit(context.expression());
    }

    public override List<Op> VisitExpressionMult([NotNull] DatalogParser.ExpressionMultContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.mult);
    
    public override List<Op> VisitExpressionAdd([NotNull] DatalogParser.ExpressionAddContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.add);
    
    public override List<Op> VisitExpressionLogic([NotNull] DatalogParser.ExpressionLogicContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.logic);
    
    public override List<Op> VisitExpressionComp([NotNull] DatalogParser.ExpressionCompContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.comp);
    
    public override List<Op> VisitExpressionMethod([NotNull] DatalogParser.ExpressionMethodContext context) 
    {
        var operands = new List<Op>();
        operands.AddRange(base.Visit(context.expression()));
        
        foreach(var termContext in context.term())
        {
            operands.Add(new Op(_termVisitor.Visit(termContext)));
        }
        
        operands.Add(ToBinaryOp(context.METHOD_INVOCATION().GetText().TrimStart('.')));

        return operands;
    }
    
    List<Op> VisitExpressionBinary(DatalogParser.ExpressionContext context1, DatalogParser.ExpressionContext context2, Antlr4.Runtime.IToken op) 
    {
        var operands = new List<Op>();
        operands.AddRange(base.Visit(context1));
        operands.AddRange(base.Visit(context2));
        operands.Add(ToBinaryOp(op.Text));
        return operands;
    }

    static Op ToBinaryOp(string opText)
    {
        var kind = opText switch {
            "<"  =>  OpBinary.Kind.LessThan,
            ">" => OpBinary.Kind.GreaterThan,
            "<="  => OpBinary.Kind.LessOrEqual,
            ">=" =>  OpBinary.Kind.GreaterOrEqual,
            "==" =>  OpBinary.Kind.Equal,
            "&&" =>  OpBinary.Kind.And,
            "||" =>  OpBinary.Kind.Or,
            "+" =>  OpBinary.Kind.Add,
            "-" =>  OpBinary.Kind.Sub,
            "*" =>  OpBinary.Kind.Mul,
            "/" =>  OpBinary.Kind.Div,
            "starts_with" => OpBinary.Kind.Prefix,
            "ends_with" => OpBinary.Kind.Suffix,
            "contains" => OpBinary.Kind.Contains,
            "matches" => OpBinary.Kind.Regex,
            _ => throw new NotSupportedException(opText)
        };
        
        return new Op(new OpBinary(kind));
    }

    public override List<Op> VisitExpressionUnary([NotNull] DatalogParser.ExpressionUnaryContext context) 
    {
        var ops = Visit(context.expression());
        ops.Add(new Op(new OpUnary(OpUnary.Kind.Negate)));

        return ops;
    }

    public override List<Op> VisitExpressionTerm([NotNull] DatalogParser.ExpressionTermContext context) 
    {
        var term = _termVisitor.Visit(context.fact_term());
        return new List<Op> { new Op(term) };
    }

    public override List<Op> VisitExpressionVariable([NotNull] DatalogParser.ExpressionVariableContext context) 
    {
        var variable = new Datalog.Variable(context.VARIABLE().GetText().TrimStart('$'));
        return new List<Op> { new Op(variable) };
    }
}