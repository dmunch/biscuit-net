using System.Text.RegularExpressions;

namespace biscuit_net.Expressions;
using Datalog;

public static class Evaluator
{
    public static bool Evaluate(List<Op> ops, Func<Variable, Term> variableResolver)
    {
        var stack = new Stack<Term>();

        foreach(var op in ops)
        {
           switch(op.Type)
           {
                case Op.OpType.None: break;
                case Op.OpType.Binary: {
                    //binary operation: an operation that applies on two arguments.
                    //When executed, it pops two values from the stack, applies the operation, then pushes the result

                    var value2 = stack.Pop();
                    var value1 = stack.Pop();
                                        
                    Term res = (op.BinaryOp.OpKind, value1, value2) switch
                    {
                        (OpBinary.Kind.LessThan, Date d1, Date d2) => new Boolean(d1 < d2),
                        (OpBinary.Kind.LessThan, Integer i1, Integer i2) => new Boolean(i1 < i2),
                        (OpBinary.Kind.GreaterThan, Date d1, Date d2) => new Boolean(d1 > d2),
                        (OpBinary.Kind.GreaterThan, Integer i1, Integer i2) => new Boolean(i1 > i2),
                        (OpBinary.Kind.GreaterOrEqual, Date d1, Date d2) => new Boolean(d1 >= d2),
                        (OpBinary.Kind.GreaterOrEqual, Integer i1, Integer i2) => new Boolean(i1 >= i2),
                        (OpBinary.Kind.LessOrEqual, Date d1, Date d2) => new Boolean(d1 <= d2),
                        (OpBinary.Kind.LessOrEqual, Integer i1, Integer i2) => new Boolean(i1 <= i2),
                        (OpBinary.Kind.Equal, Date d1, Date d2) => new Boolean(d1 == d2),
                        (OpBinary.Kind.Equal, Integer i1, Integer i2) => new Boolean(i1 == i2),
                        (OpBinary.Kind.Equal, String s1, String s2) => new Boolean(s1 == s2),
                        (OpBinary.Kind.Equal, Bytes b1, Bytes b2) => new Boolean(b1 == b2),
                        (OpBinary.Kind.Equal, Set s1, Set s2) => new Boolean(s1.Values.SequenceEqual(s2.Values)),
                        (OpBinary.Kind.And, Boolean b1, Boolean b2) => new Boolean(b1 & b2),
                        (OpBinary.Kind.Or, Boolean b1, Boolean b2) => new Boolean(b1 | b2),
                        (OpBinary.Kind.Regex, String s1, String s2) => new Boolean(Regex.IsMatch(s1.Value, s2.Value)),
                        (OpBinary.Kind.Contains, String s1, String s2) => new Boolean(s1.Value.Contains(s2.Value)),
                        (OpBinary.Kind.Contains, Set set, Integer i) => new Boolean(set.Values.Contains(i)),
                        (OpBinary.Kind.Contains, Set set, String s) => new Boolean(set.Values.Contains(s)),
                        (OpBinary.Kind.Contains, Set set, Boolean b) => new Boolean(set.Values.Contains(b)),
                        (OpBinary.Kind.Contains, Set set, Date d) => new Boolean(set.Values.Contains(d)),
                        (OpBinary.Kind.Contains, Set set, Bytes b) => new Boolean(set.Values.Contains(b)),
                        (OpBinary.Kind.Prefix, String s1, String s2) => new Boolean(s1.Value.StartsWith(s2.Value)),
                        (OpBinary.Kind.Suffix, String s1, String s2) => new Boolean(s1.Value.EndsWith(s2.Value)),
                        (OpBinary.Kind.Add, String s1, String s2) => new String($"{s1.Value}{s2.Value}"),
                        (OpBinary.Kind.Add, Integer i1, Integer i2) => new Integer(checked(i1.Value + i2.Value)),
                        (OpBinary.Kind.Sub, Integer i1, Integer i2) => new Integer(checked(i1.Value - i2.Value)),
                        (OpBinary.Kind.Mul, Integer i1, Integer i2) => new Integer(checked(i1.Value * i2.Value)),
                        (OpBinary.Kind.Div, Integer i1, Integer i2) => new Integer(i1.Value / i2.Value),
                        /*
                        OpBinary.Kind.Intersection => throw new NotImplementedException(),
                        OpBinary.Kind.Union => throw new NotImplementedException(),
                        */
                        (OpBinary.Kind.BitwiseAnd, Integer i1, Integer i2) => new Integer(i1.Value & i2.Value),
                        (OpBinary.Kind.BitwiseOr, Integer i1, Integer i2) => new Integer(i1.Value | i2.Value),                        
                        (OpBinary.Kind.BitwiseXor, Integer i1, Integer i2) => new Integer(i1.Value ^ i2.Value),
                        
                        (OpBinary.Kind.NotEqual, Date d1, Date d2) => new Boolean(d1 != d2),
                        (OpBinary.Kind.NotEqual, Integer i1, Integer i2) => new Boolean(i1 != i2),
                        (OpBinary.Kind.NotEqual, String s1, String s2) => new Boolean(s1 != s2),
                        (OpBinary.Kind.NotEqual, Bytes b1, Bytes b2) => new Boolean(b1 != b2),
                        (OpBinary.Kind.NotEqual, Set s1, Set s2) => new Boolean(!s1.Values.SequenceEqual(s2.Values)),

                        (var opKind, Term t1, Term t2) => throw new NotImplementedException($"Operator {opKind} not implemented on types {t1.GetType()} and {t2.GetType()}")
                    };

                    stack.Push(res);
                    break;            
                }
                case Op.OpType.Unary when op.UnaryOp.OpKind == OpUnary.Kind.Parens: break; //do nothing
                case Op.OpType.Unary when op.UnaryOp.OpKind == OpUnary.Kind.Length: break; //TODO
                case Op.OpType.Unary when op.UnaryOp.OpKind == OpUnary.Kind.Negate: {
                    //unary operation: an operation that applies on one argument.
                    //When executed, it pops a value from the stack, applies the operation, then pushes the result

                    var value = stack.Pop();
                    stack.Push(new Boolean(!(Boolean) value));
                    
                    break; 
                }
                case Op.OpType.Value when op.Value != null: {
                    //value: a raw value of any type.
                    //If it is a variable, the variable must also appear in a predicate, so the variable gets a real value for execution.
                    //When encountering a value opcode, we push it onto the stack

                    if(op.Value is Variable v)
                    {
                        var resolved = variableResolver(v);
                        if(resolved is Variable)
                        {
                            //variable couldn't be resolved, expression evulation thus results to false
                            return false;
                        }
                        stack.Push(resolved);
                        break;
                    }

                    stack.Push(op.Value);
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