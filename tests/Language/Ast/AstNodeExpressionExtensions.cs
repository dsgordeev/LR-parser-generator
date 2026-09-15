using System.Collections.Generic;
using System.Linq;
using Language.Ast.Nodes;

namespace Language.Ast;

public static class AstNodeExpressionExtensions
{
    public static List<TNode> AsList<TNode>(this AstNodeExpression x)
        where TNode : AstNodeExpression
    {
        IEnumerable<TNode> Unwrap(AstNodeExpression y)
        {
            return y switch
            {
                ExprListNode list => list.Expressions.SelectMany(Unwrap),
            TNode tNode => [tNode],
            _ => []
        };
    }

        return [.. Unwrap(x)];
    }

    public static TNode? As<TNode>(this AstNodeExpression x)
        where TNode : AstNodeExpression
    {
        return x as TNode;
    }
}