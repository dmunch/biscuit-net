using System.Text.RegularExpressions;
using biscuit_net.Proto;
using VeryNaiveDatalog;

namespace biscuit_net;

public static class ExpressionEvaluator
{
    public static bool Evaluate(Substitution substitution, List<Op> ops, List<string> symbols)
    {
        var stack = new Stack<Term>();

        bool StringRegex(Term term1, Term term2)
        {
            var str = (String) term1;
            var regex = (String) term2;
            return Regex.IsMatch(str.Value, regex.Value);
        };

        foreach(var op in ops)
        {
           switch(op.ContentCase)
           {
                case Op.ContentOneofCase.None: break;
                case Op.ContentOneofCase.Binary: {
                    //binary operation: an operation that applies on two arguments.
                    //When executed, it pops two values from the stack, applies the operation, then pushes the result

                    var value2 = stack.Pop();
                    var value1 = stack.Pop();

                    var result = op.Binary.kind switch
                    {
                        OpBinary.Kind.LessThan => (Date)value1 < (Date)value2,
                        OpBinary.Kind.GreaterThan => (Date)value1 > (Date)value2,
                        OpBinary.Kind.GreaterOrEqual => (Date)value1 >= (Date)value2,
                        OpBinary.Kind.LessOrEqual => (Date)value1 <= (Date)value2,
                        OpBinary.Kind.Equal => (Date)value1 == (Date)value2,
                        OpBinary.Kind.Regex => StringRegex(value1, value2),

                        /*
                        OpBinary.Kind.Contains => throw new NotImplementedException(),
                        OpBinary.Kind.Prefix => throw new NotImplementedException(),
                        OpBinary.Kind.Suffix => throw new NotImplementedException( ),
                        
                        OpBinary.Kind.Add => throw new NotImplementedException(),
                        OpBinary.Kind.Sub => throw new NotImplementedException(),
                        OpBinary.Kind.Mul => throw new NotImplementedException(),
                        OpBinary.Kind.Div => throw new NotImplementedException(),
                        */
                        OpBinary.Kind.And => (Boolean) value1 & (Boolean) value2,
                        OpBinary.Kind.Or => (Boolean) value1 | (Boolean) value2,
                        /*
                        OpBinary.Kind.Intersection => throw new NotImplementedException(),
                        OpBinary.Kind.Union => throw new NotImplementedException(),
                        */
                        _ => throw new NotSupportedException($"{op.Binary.kind}")
                    };
                    
                    stack.Push(new Boolean(result));
                    break;
                }
                case Op.ContentOneofCase.Unary: {
                    //unary operation: an operation that applies on one argument.
                    //When executed, it pops a value from the stack, applies the operation, then pushes the result
                    var value = stack.Pop();
                    var result = op.Unary.kind switch
                    {
                        OpUnary.Kind.Length => throw new NotImplementedException(),
                        OpUnary.Kind.Negate => !(Boolean) value,
                        OpUnary.Kind.Parens => throw new NotImplementedException(),
                        _ => throw new NotSupportedException($"{op.Binary.kind}")
                    };
                    stack.Push(new Boolean(result));
                    break; 
                }
                case Op.ContentOneofCase.Value: {
                    //value: a raw value of any type.
                    //If it is a variable, the variable must also appear in a predicate, so the variable gets a real value for execution.
                    //When encountering a value opcode, we push it onto the stack
                    var atom = Converters.ToAtom(op.Value, symbols);
                    
                    stack.Push(atom.Apply(substitution));
                    break;
                }
           }
        }


        if(stack.Count > 1)
        {
            throw new Exception("Expression evaluation error");
        }
        var finalBool = (Boolean)stack.Pop();
        if(finalBool == null)
        {
            throw new Exception("Expression evaluation error");
        }

        return finalBool.Value;
    }
}