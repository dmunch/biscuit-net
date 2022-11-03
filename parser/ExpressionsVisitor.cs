namespace parser;

using Antlr4.Runtime.Misc;
using biscuit_net.Proto;
using System.Linq;

public class ExpressionsVisitor : ExpressionsBaseVisitor<List<Op>>
{
    TermVisitor _termVisitor = new TermVisitor();

    public override List<Op> VisitExpression([NotNull] ExpressionsParser.ExpressionContext context) 
    { 
        var eeList = context.expression_element().Select(ee => Visit(ee)).ToList();
        
        var operatorContext = context.OPERATOR();

        if(eeList.Count() == 1)
        {
            return eeList.First();
        }

        //binary expression(s)
        if(eeList.Count != operatorContext.Count() + 1)
            throw new Exception("Parsing Error");

        
        var operands = new List<Op>();
        var operatorStack = new Stack<Op>();

        void PushOperator(Op incomingOp)
        {
            if(operatorStack.Count == 0)
            {
                operatorStack.Push(incomingOp);
                return;
            } 
            
            var topOp = operatorStack.Peek();

            if(precedence[incomingOp.Binary.kind] > precedence[topOp.Binary.kind])
            {
                operatorStack.Push(incomingOp);
                return;
            }

            if(precedence[incomingOp.Binary.kind] == precedence[topOp.Binary.kind])
            {
                operatorStack.Pop();
                operands.Add(topOp);
                operatorStack.Push(incomingOp);
                return;
            }
            
            
            operatorStack.Pop();
            operands.Add(topOp);
            PushOperator(incomingOp);
        }

        operands.AddRange(eeList[0]);
        for(int opId = 0; opId < operatorContext.Count(); opId++)
        {
            var opText = operatorContext[opId].GetText();

            var incomingOp = ToBinaryOp(opText);
            PushOperator(incomingOp);
            
            operands.AddRange(eeList[opId + 1]);
        }

        while(operatorStack.TryPop(out var op))
        {
            operands.Add(op);
        }
        
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

    Dictionary<OpBinary.Kind, int> precedence = new Dictionary<OpBinary.Kind, int> {
         {OpBinary.Kind.LessThan, 0},
         {OpBinary.Kind.GreaterThan, 0},
         {OpBinary.Kind.LessOrEqual, 0},
         {OpBinary.Kind.GreaterOrEqual, 0},
         {OpBinary.Kind.Equal, 0},
         {OpBinary.Kind.And, 2},
         {OpBinary.Kind.Add, 1},
         {OpBinary.Kind.Sub, 1},
         {OpBinary.Kind.Mul, 2},
         {OpBinary.Kind.Div, 2}
    };


    public override List<Op> VisitExpression_unary([NotNull] ExpressionsParser.Expression_unaryContext context) 
    {
        var ops = Visit(context.expression());
        ops.Add(new Op() { Unary = new OpUnary() { kind = OpUnary.Kind.Negate }});

        return ops;
    }

    public override List<Op> VisitExpression_term([NotNull] ExpressionsParser.Expression_termContext context) 
    {
        if(context.term() != null)
        {
            var op = _termVisitor.Visit(context.term());
            return new List<Op> { op };
        }
        return base.Visit(context.expression());
    }
}
