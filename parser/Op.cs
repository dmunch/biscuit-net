using VeryNaiveDatalog;

namespace parser;

public record struct Op
{
    public Op()
    {
        Type = OpType.None;
    }

    public Op(Term value)
    {
        Value = value;
        Type = OpType.Value;
    }

    public Op(OpUnary unaryOp)
    {
        Value = null;
        UnaryOp = unaryOp;
        Type = OpType.Unary;
    }

    public Op(OpBinary binaryOp)
    {
        Value = null;
        BinaryOp = binaryOp;
        Type = OpType.Binary;
    }
    
    public Term? Value { get; }
    public OpBinary BinaryOp { get; }
    public OpUnary UnaryOp { get; }
    public OpType Type { get; }

    public enum OpType
    {
        None = 0,
        Value = 1,
        Unary = 2,
        Binary = 3,
    }
}

public readonly record struct OpBinary(OpBinary.Kind OpKind)
{
        public enum Kind
        {
            LessThan = 0,
            GreaterThan = 1,
            LessOrEqual = 2,
            GreaterOrEqual = 3,
            Equal = 4,
            Contains = 5,
            Prefix = 6,
            Suffix = 7,
            Regex = 8,
            Add = 9,
            Sub = 10,
            Mul = 11,
            Div = 12,
            And = 13,
            Or = 14,
            Intersection = 15,
            Union = 16,
        }
}

public readonly record struct OpUnary(OpUnary.Kind OpKind)
{
    public enum Kind
    {
        Negate = 0,
        Parens = 1,
        Length = 2,
    }
}