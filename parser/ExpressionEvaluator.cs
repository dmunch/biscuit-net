using System.Text.RegularExpressions;
using VeryNaiveDatalog;

namespace parser;

public static class ExpressionEvaluator
{
    static TResult BinaryOp<T1, TResult>(Term t1, Term t2, Func<T1, T1, TResult> op1)
        where T1: Term
    {
        switch (t1, t2) {
            case (T1 t11, T1 t21): {
                return op1(t11, t21);
            }
        }

        throw new Exception();
    }

    static TResult BinaryOp<T1, T2, TResult>(Term t1, Term t2, Func<T1, T1, TResult> op1, Func<T2, T2, TResult> op2)  
        where T1: Term
        where T2: Term
    {
        switch (t1, t2) {
            case (T1 t11, T1 t21): {
                return op1(t11, t21);
            }
            case (T2 t12, T2 t22): {
                return op2(t12, t22);
            }
        }

        throw new Exception();
    }

    static TResult BinaryOp<T1, T2, T3, TResult>(Term t1, Term t2, Func<T1, T1, TResult> op1, Func<T2, T2, TResult> op2, Func<T3, T3, TResult> op3)
        where T1: Term
        where T2: Term
        where T3: Term
    {
        switch (t1, t2) {
            case (T1 t11, T1 t21): {
                return op1(t11, t21);
            }
            case (T2 t12, T2 t22): {
                return op2(t12, t22);
            }
            case (T3 t13, T3 t23): {
                return op3(t13, t23);
            }
        }

        throw new Exception();
    }

    public static bool Evaluate(List<Op> ops, Func<Variable, Term> variableResolver)
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
           switch(op.Type)
           {
                case Op.OpType.None: break;
                case Op.OpType.Binary: {
                    //binary operation: an operation that applies on two arguments.
                    //When executed, it pops two values from the stack, applies the operation, then pushes the result

                    var value2 = stack.Pop();
                    var value1 = stack.Pop();
                    
                    var boolResult = op.BinaryOp.OpKind switch
                    {
                        OpBinary.Kind.LessThan => 
                            BinaryOp<Date, Integer, bool>(value1, value2, (t1, t2) => t1 < t2, (t1, t2) => t1 < t2),
                        OpBinary.Kind.GreaterThan => 
                            BinaryOp<Date, Integer, bool>(value1, value2, (t1, t2) => t1 > t2, (t1, t2) => t1 > t2),
                        OpBinary.Kind.GreaterOrEqual => 
                            BinaryOp<Date, Integer, bool>(value1, value2, (t1, t2) => t1 >= t2, (t1, t2) => t1 >= t2),
                        OpBinary.Kind.LessOrEqual => 
                            BinaryOp<Date, Integer, bool>(value1, value2, (t1, t2) => t1 <= t2, (t1, t2) => t1 <= t2),
                        OpBinary.Kind.Equal => 
                            BinaryOp<Date, Integer, String, bool>(value1, value2, (t1, t2) => t1 == t2, (t1, t2) => t1 == t2, (t1, t2) => t1 == t2),
                        OpBinary.Kind.And => 
                            BinaryOp<Boolean, bool>(value1, value2, (t1, t2) => t1 & t2),
                        OpBinary.Kind.Or => 
                            BinaryOp<Boolean, bool>(value1, value2, (t1, t2) => t1 | t2),
                        OpBinary.Kind.Regex => StringRegex(value1, value2),
                        OpBinary.Kind.Contains => BinaryOp<String, bool>(value1, value2, (t1, t2) => t1.Value.Contains(t2.Value)),
                        OpBinary.Kind.Prefix => BinaryOp<String, bool>(value1, value2, (t1, t2) => t1.Value.StartsWith(t2.Value)),
                        OpBinary.Kind.Suffix => BinaryOp<String, bool>(value1, value2, (t1, t2) => t1.Value.EndsWith(t2.Value)),
                        /*
                        OpBinary.Kind.Intersection => throw new NotImplementedException(),
                        OpBinary.Kind.Union => throw new NotImplementedException(),
                        */
                        _ => (bool?)null
                    };
                    

                    if(boolResult.HasValue) 
                    {
                        stack.Push(new Boolean(boolResult.Value));
                        break;
                    }

                    var intResult = op.BinaryOp.OpKind switch
                    {
                        OpBinary.Kind.Add => BinaryOp<Integer, long>(value1, value2, (t1, t2) => t1.Value + t2.Value),
                        OpBinary.Kind.Sub => BinaryOp<Integer, long>(value1, value2, (t1, t2) => t1.Value - t2.Value),
                        OpBinary.Kind.Mul => BinaryOp<Integer, long>(value1, value2, (t1, t2) => t1.Value * t2.Value),
                        OpBinary.Kind.Div => BinaryOp<Integer, long>(value1, value2, (t1, t2) => t1.Value / t2.Value),

                        _ => (long?)null
                    };
                    
                    if(intResult.HasValue) 
                    {
                        stack.Push(new Integer(intResult.Value));
                        break;
                    }

                    throw new NotImplementedException();
                }
                case Op.OpType.Unary: {
                    //unary operation: an operation that applies on one argument.
                    //When executed, it pops a value from the stack, applies the operation, then pushes the result
                    var value = stack.Pop();
                    var result = op.UnaryOp.OpKind switch
                    {
                        OpUnary.Kind.Length => throw new NotImplementedException(),
                        OpUnary.Kind.Negate => !(Boolean) value,
                        OpUnary.Kind.Parens => throw new NotImplementedException(),
                        _ => throw new NotSupportedException($"{op.UnaryOp.OpKind}")
                    };
                    stack.Push(new Boolean(result));
                    break; 
                }
                case Op.OpType.Value when op.Value != null: {
                    //value: a raw value of any type.
                    //If it is a variable, the variable must also appear in a predicate, so the variable gets a real value for execution.
                    //When encountering a value opcode, we push it onto the stack

                    //var atom = Converters.ToAtom(op.Value, symbols);                    
                    //stack.Push(atom.Apply(substitution));

                    if(op.Value is Variable v)
                    {
                        stack.Push(variableResolver(v));
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