using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language.Ast.Nodes;

public class ExprListNode(List<AstNodeExpression> expressions) : AstNodeExpression
{
    public List<AstNodeExpression> Expressions { get; } = expressions;

    public override List<AstNodeExpression> AsList() => Expressions;

    public override string ToString() => Expressions.Aggregate(new StringBuilder(), (sb, x) => sb.Append($", {x}")).ToString().TrimStart(',');
}