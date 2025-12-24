using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Language.Ast.Nodes;

namespace Language.Ast.Algorithms.Convert;

public class CppTranslatorVisitor(TextWriter textWriter) : AstNodeExpressionVisitor
{
    private readonly Stack<State> _states = new([State.Default]);

    private enum State
    {
        Default,
        ElseBranches,
        ParameterGroups
    }

    private bool ShouldWrapExpressionList
    {
        get
        {
            var state = _states.Pop();

            var result = _states.Peek() switch
            {
                State.ElseBranches => false,
                State.ParameterGroups => false,
                _ => true
            };

            _states.Push(state);

            return result;
        }
    }

    private int _indent;
    private readonly Dictionary<string, string> _operatorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "+", "+" },
        { "-", "-" },
        { "*", "*" },
        { "/", "/" },
        { "=", "==" },
        { "<>", "!=" },
        { "<", "<" },
        { ">", ">" },
        { "<=", "<=" },
        { ">=", ">=" },
        { "and", "&&" },
        { "or", "||" },
        { "not", "!" }
    };

    public override Expression Visit(Expression node)
    {
        _states.Push(State.Default);
        var node2 = base.Visit(node);
        _states.Pop();
        return node2;
    }

    // Update VisitExtension to handle all node types
    protected override Expression VisitExtension(Expression expression) =>
        expression switch
        {
            ExprListNode node => VisitExprListNode(node),
            ModuleNode node => VisitModuleNode(node),
            FunctionNode node => VisitFunctionNode(node),
            TypeNode node => VisitTypeNode(node),
            ParamGroupNode node => VisitParamGroupNode(node),
            IfNode node => VisitIfNode(node),
            IfNode.ElseIfNode node => VisitElseIfNode(node),
            BinaryOpNode node => VisitBinaryOpNode(node),
            UnaryOpNode node => VisitUnaryOpNode(node),
            NumberNode node => VisitNumberNode(node),
            BoolNode node => VisitBoolNode(node),
            IdentifierNode node => VisitIdentifierNode(node),
            PostfixNode node => VisitPostfixNode(node),
            PostfixNode.FunctionCallNode node => VisitFunctionCallNode(node),
            PostfixNode.TypeCastNode node => VisitTypeCastNode(node),

            AstNodeExpression node => VisitAstNodeExpression(node),
            _ => base.VisitExtension(expression)
        };

    protected override Expression VisitAstNodeExpression(AstNodeExpression node)
    {
        Write($"/* Unhandled AstNodeExpression: {node.GetType().Name} */");
        return node;
    }

    protected override Expression VisitExprListNode(ExprListNode node)
    {
        if (node.Expressions.Count == 1)
        {
            Visit(node.Expressions[0]);
        }
        else
        {
            var shouldWrap = ShouldWrapExpressionList;

            if (shouldWrap) Write("std::make_tuple(");
            if (shouldWrap) _indent++;

            WriteNodes(node.Expressions, x => x);

            if (shouldWrap) _indent--;
            if (shouldWrap) Write(")");
        }
        return node;
    }

    protected override Expression VisitModuleNode(ModuleNode node)
    {
        WriteLine("");

        if (node.Name != null)
        {
            WriteLine($"// Module: {node.Name}");
        }

        if (node.Modules != null!)
        {
            Visit(node.Modules);
        }

        Visit(node.Functions);

        return node;
    }

    protected override Expression VisitFunctionNode(FunctionNode node)
    {
        var isLambda = node.Name.As<IdentifierNode>()?.Name == "lambda";

        if (!isLambda)
        {
            WriteLine("");
        }

        // Return type
        if (!isLambda)
        {
            if (node.ResultTypeNames != null!)
            {
                if (!isLambda) WriteI("static ");

                var resultTypes = ExtractTypeNames(node.ResultTypeNames);
                Write(resultTypes.Count == 1
                    ? $"{MapType(resultTypes[0])} "
                    : $"std::tuple<{string.Join(", ", resultTypes.Select(MapType))}> ");
            }
            else
            {
                WriteI("auto ");
            }
        }

        // Function name
        if (!isLambda)
        {
            Visit(node.Name);
        }
        else
        {
            Write("[&]");
        }

        Write("(");

        // Parameters
        if (node.ParameterGroups != null!)
        {
            _states.Push(State.ParameterGroups);
            Visit(node.ParameterGroups);
            _states.Pop();
        }

        if (!isLambda) WriteLine(")"); else Write(")");
        if (!isLambda) WriteLine("{"); else Write("{");

        Within(() =>
        {
            if (!isLambda) WriteI("return "); else Write("return ");
            Visit(node.Body);
            if (!isLambda) WriteLine(";"); else Write(";");
        });

        if (!isLambda) WriteLine("}"); else Write("}");

        return node;
    }

    protected override Expression VisitTypeNode(TypeNode node)
    {
        Visit(node.Name);
        return node;
    }

    protected override Expression VisitParamGroupNode(ParamGroupNode node)
    {
        var typeName = ExtractTypeName(node.GroupTypeName);

        var parameterNames = node.ParameterNames.AsList<IdentifierNode>();

        int count = parameterNames.Count;
        foreach (var paramName in parameterNames)
        {
            Write($"{MapType(typeName)} ");
            Visit(paramName);
            if (--count > 0)
            {
                Write(", ");
            }
        }

        return node;
    }

    protected override Expression VisitElseIfNode(IfNode.ElseIfNode node)
    {
        Visit(node.Test);
        Write(" ? ");
        Visit(node.Body);
        Write(" : ");

        return node;
    }

    protected override Expression VisitIfNode(IfNode node)
    {
        Write("(");
        Visit(node.Test);
        Write(" ? ");
        Visit(node.ThenBranch);
        Write(" : ");
        foreach (var elseIfNode in node.ElseIfs?.AsList<IfNode.ElseIfNode>() ?? [])
        {
            VisitElseIfNode(elseIfNode);
        }
        Visit(node.ElseBranch);
        Write(")");

        return node;
    }


    protected override Expression VisitBinaryOpNode(BinaryOpNode node)
    {
        Write("(");
        Visit(node.Left);
        Write($" {MapOperator(node.Operator)} ");
        Visit(node.Right);
        Write(")");
        return node;
    }

    protected override Expression VisitNumberNode(NumberNode node)
    {
        Write(node.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return node;
    }

    protected override Expression VisitBoolNode(BoolNode node)
    {
        Write(node.Value ? "true" : "false");
        return node;
    }

    protected override Expression VisitIdentifierNode(IdentifierNode node)
    {
        Write(node.Name);
        return node;
    }

    protected override Expression VisitUnaryOpNode(UnaryOpNode node)
    {
        var op = MapOperator(node.Operator);
        Write($"({op}");
        Visit(node.Operand);
        Write(")");
        return node;
    }

    protected virtual Expression VisitPostfixNode(PostfixNode node)
    {
        switch (node.PostfixOperator)
        {
            case PostfixNode.TypeCastNode typeCast:
                Write("static_cast<");
                Visit(typeCast.TargetType);
                Write(">(");
                Visit(node.Factor);
                Write(")");
                break;
            default:
                Visit(node.Factor);
                Visit(node.PostfixOperator);
                break;
        }

        return node;
    }

    protected virtual Expression VisitFunctionCallNode(PostfixNode.FunctionCallNode node)
    {
        Write("(");
        if (node.Parameters != null!)
        {
            Visit(node.Parameters);
        }
        Write(")");
        return node;
    }

    protected virtual Expression VisitTypeCastNode(PostfixNode.TypeCastNode node)
    {
        Write("static_cast<");
        Visit(node.TargetType);
        Write(">(");
        return node;
    }

    private static string MapType(string sisalType)
    {
        return sisalType.ToLower() switch
        {
            "integer" => "int",
            "real" => "double",
            "boolean" => "bool",
            "string" => "std::string",
            var x when x.StartsWith("array of") => $"std::vector<{MapType(x.Substring("array of".Length))}>",
            var x when x.StartsWith("stream of") => $"std::vector<{MapType(x.Substring("stream of".Length))}>",
            _ => sisalType
        };
    }

    private string MapOperator(string sisalOperator)
    {
        return _operatorMap.TryGetValue(sisalOperator.ToLower(), out var found)
            ? found
            : sisalOperator;
    }

    private static List<string> ExtractTypeNames(AstNodeExpression typeNode)
    {
        var types = new List<string>();

        if (typeNode is TypeNode simpleType)
        {
            types.Add(ExtractTypeName(simpleType));
        }
        else if (typeNode is ExprListNode exprList)
        {
            foreach (var expr in exprList.Expressions)
            {
                if (expr is TypeNode t)
                {
                    types.Add(ExtractTypeName(t));
                }
            }
        }

        return types;
    }

    private static string ExtractTypeName(AstNodeExpression typeNode)
    {
        return typeNode switch
        {
            TypeNode { Name: IdentifierNode id } => id.Name,
            _ => "auto"
        };
    }

    private void Within(Action action)
    {
        _indent++;
        action();
        _indent--;
    }

    private void WriteLine(string line) => textWriter.WriteLine(line);

    private void WriteLineI(string line) => textWriter.WriteLine($"{new string(' ', _indent * 4)}{line}");

    private void Write(string line) => textWriter.Write(line);

    private void WriteI(string line) => textWriter.Write($"{new string(' ', _indent * 4)}{line}");

    private void WriteNodes<TNode>(
        List<TNode> list,
        Func<TNode, AstNodeExpression> select,
        string splitter = ", ")
    {
        var count = 0;
        foreach (var x in list)
        {
            Visit(select(x));
            if (++count < list.Count) Write(splitter);
        }
    }
}
