using System;
using System.Globalization;

namespace Language.Ast.Nodes;

public class NumberNode(double value) : AstNodeExpression
{
    public double Value { get; } = value;

    public bool IsInt => Math.Ceiling(Value) - Value == 0.0;

    public override string ToString() => $"{Value.ToString(CultureInfo.InvariantCulture)}";
}
