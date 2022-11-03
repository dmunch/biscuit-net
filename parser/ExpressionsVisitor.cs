namespace parser;

using Antlr4.Runtime.Misc;
using biscuit_net.Proto;

public class ExpressionsVisitor : ExpressionsBaseVisitor<List<Op>>
{
    TermVisitor _termVisitor = new TermVisitor();
    
     public override List<Op> VisitExpressionParentheses([NotNull] ExpressionsParser.ExpressionParenthesesContext context) 
    {
        return base.Visit(context.expression());
    }

    public override List<Op> VisitExpressionMult([NotNull] ExpressionsParser.ExpressionMultContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.mult);
    
    public override List<Op> VisitExpressionAdd([NotNull] ExpressionsParser.ExpressionAddContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.add);
    
    public override List<Op> VisitExpressionLogic([NotNull] ExpressionsParser.ExpressionLogicContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.logic);
    
    public override List<Op> VisitExpressionComp([NotNull] ExpressionsParser.ExpressionCompContext context) 
        => VisitExpressionBinary(context.expression(0), context.expression(1), context.comp);
    

    List<Op> VisitExpressionBinary(ExpressionsParser.ExpressionContext context1, ExpressionsParser.ExpressionContext context2, Antlr4.Runtime.IToken op) 
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
            _ => throw new NotSupportedException(opText)
        };
        
        return new Op() { Binary = new OpBinary() { kind = kind }};
    }

    public override List<Op> VisitExpressionUnary([NotNull] ExpressionsParser.ExpressionUnaryContext context) 
    {
        var ops = Visit(context.expression());
        ops.Add(new Op() { Unary = new OpUnary() { kind = OpUnary.Kind.Negate }});

        return ops;
    }

    public override List<Op> VisitExpressionTerm([NotNull] ExpressionsParser.ExpressionTermContext context) 
    {
        var op = _termVisitor.Visit(context.fact_term());
        return new List<Op> { op };
    }
}
