using System.Globalization;

namespace Language.Ast.Nodes;

public class NumberNode : AstNodeExpression
{
    public NumberNode(int value) => Value = IntValue = value;
    public NumberNode(long value) => Value = LongValue = value;
    public NumberNode(float value) => Value = FloatValue = value;
    public NumberNode(double value) => Value = DoubleValue = value;
    public object Value { get; }
    public long? LongValue { get; }
    public int? IntValue { get; }
    public double? DoubleValue { get; }
    public float? FloatValue { get; }

    public bool IsInt => IntValue.HasValue || LongValue.HasValue;
    public bool IsReal => FloatValue.HasValue || DoubleValue.HasValue;

    public override string ToString() =>
        Value is float or double
            ? ((double)Value).ToString(CultureInfo.InvariantCulture)
            : Value.ToString();
}
