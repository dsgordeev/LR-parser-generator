using System.Collections.Generic;
using System.Linq;
using Language.Ast.Nodes;

namespace Language.Ast;

public static class AstNodeExpressionExtensions
{
    public static List<TNode> AsList<TNode>(this AstNodeExpression x)
        where TNode : AstNodeExpression
    {
        return x switch
        {
            ExprListNode list => list.Expressions.Cast<TNode>().ToList(),
            TNode tNode => [tNode],
            _ => []
        };
    }

    public static TNode? As<TNode>(this AstNodeExpression x)
        where TNode : AstNodeExpression
    {
        return x as TNode;
    }
}