using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ParserGenerator.Positions;

namespace Language.Ast;

public abstract class AstNodeExpression : Expression, IWithPosition<AstNodeExpression>
{
    public int WrapCount { get; private set; }

    public Position Position { get; private set; }

    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor) => throw new InvalidOperationException("VisitChildrenMustBeOverridden");

    public AstNodeExpression WithPosition(IList<AstNodeExpression> list)
    {
        Position = list[0].Position with
        {
            EndRow = list[^1].Position.EndRow,
            EndCol = list[^1].Position.EndCol
        };
        return this;
    }

    public AstNodeExpression WithPosition(Position position)
    {
        Position = position;
        return this;
    }

    public AstNodeExpression WithWrap()
    {
        WrapCount++;
        return this;
    }

    public virtual List<AstNodeExpression> AsList() => [this];
}