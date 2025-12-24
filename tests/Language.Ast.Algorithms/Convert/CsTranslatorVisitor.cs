using Language.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;

namespace Language.Ast.Algorithms.Convert;

public sealed class CsTranslatorVisitor(TextWriter textWriter) : AstNodeExpressionVisitor
{
    private int _intent = 1;

    private readonly Dictionary<string, string> _operatorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "=", "==" },
        { "&", "&&" },
        { "|", "||" }
    };


    private string Intent => new(' ', _intent * 4);

    private void WithIntent(Action action)
    {
        _intent++;
        action();
        _intent--;
    }
    
    protected override Expression VisitAstNodeExpression(AstNodeExpression node)
    {
        textWriter.Write($"{Intent}|!{node.GetType().Name}!|");
        return node;
    }

    protected override Expression VisitExprListNode(ExprListNode node)
    {
        foreach (var subNode in node.Expressions)
        {
            Visit(subNode);
        }

        return node;
    }

    protected override Expression VisitModuleNode(ModuleNode node)
    {
        if (node.Modules != null!)
        {
            Visit(node.Modules);
        }

        Visit(node.Functions);

        return node;
    }

    protected override Expression VisitFunctionNode(FunctionNode node)
    {
        textWriter.Write($"public static void");
        Visit(node.Name);
        textWriter.Write($"(");
        Visit(node.ParameterGroups);
        textWriter.Write($",");
        if (node.ResultTypeNames is ExprListNode items)
        {
            for (var i = 0; i < items.Expressions.Count; i++)
            {
                textWriter.Write($" out ");
                Visit(items.Expressions[i]);
                textWriter.Write($" y{i + 1}");
                if (i < items.Expressions.Count - 1) textWriter.Write($",");
            }
        }
        else
        {
            Visit(node.ResultTypeNames);
        }
        textWriter.WriteLine($")");

        textWriter.WriteLine($"{{");

        if (node.Body is not ExprListNode bodyItems)
        {
            bodyItems = new ExprListNode([node.Body]);
        }

        for (var i = 0; i < bodyItems.Expressions.Count; i++)
        {
            textWriter.Write($"{Intent}y{i + 1} = ");
            Visit(bodyItems.Expressions[i]);
            textWriter.WriteLine($";");
        }

        textWriter.WriteLine($"}}");

        return node;
    }

    protected override Expression VisitTypeNode(TypeNode node)
    {
        if (node.Name is IdentifierNode typeId)
        {
            var typeName = typeId.Name.ToLowerInvariant() switch
            {
                "integer" => "int",
                "real" => "float",
                "double" => "double",
                "boolean" => "bool",
                "char" => "char",
                _ => string.Empty
            };

            if (typeName != string.Empty)
            {
                textWriter.Write($" {typeName} ");
            }
            else
            {
                Visit(node.Name);
            }
        }
        else
        {
            Visit(node.Name);
        }

        return node;
    }

    protected override Expression VisitParamGroupNode(ParamGroupNode node)
    {
        var items = node.ParameterNames is ExprListNode list
            ? list.Expressions
            : [node.ParameterNames];

        for (var i = 0; i < items.Count; i++)
        {
            Visit(node.GroupTypeName);
            textWriter.Write($" ");
            Visit(items[i]);
            if (i < items.Count - 1) textWriter.Write($", ");
        }

        return node;
    }

    protected override Expression VisitElseIfNode(IfNode.ElseIfNode node)
    {
        textWriter.Write($"(");
        Visit(node.Test);
        textWriter.Write($") ");
        textWriter.Write($" : ");
        Visit(node.Body);

        return node;
    }

    protected override Expression VisitIfNode(IfNode node)
    {
        textWriter.Write($"(");
        Visit(node.Test);
        textWriter.Write($") ? ");
        Visit(node.ThenBranch);
        if (node.ElseIfs != null)
        {
            Visit(node.ElseIfs);
        }
        textWriter.Write($" : ");
        Visit(node.ElseBranch);

        return node;
    }

    protected override Expression VisitBinaryOpNode(BinaryOpNode node)
    {
        textWriter.Write($"(");
        Visit(node.Left);
        textWriter.Write($" {_operatorMap.GetValueOrDefault(node.Operator, node.Operator)} ");
        Visit(node.Right);
        textWriter.Write($")");

        return node;
    }

    protected override Expression VisitUnaryOpNode(UnaryOpNode node)
    {
        textWriter.Write($"{node.Operator}");
        Visit(node.Operand);
        return node;
    }

    protected override Expression VisitNumberNode(NumberNode node)
    {
        textWriter.Write($" {node.Value.ToString(CultureInfo.InvariantCulture)} ");

        return node;
    }

    protected override Expression VisitBoolNode(BoolNode node)
    {
        textWriter.Write($" {node.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()} ");

        return node;
    }

    protected override Expression VisitIdentifierNode(IdentifierNode node)
    {
        textWriter.Write($" {node.Name} ");

        return node;
    }
}