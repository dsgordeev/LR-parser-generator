using System.Linq.Expressions;
using Language.Ast.Nodes;

namespace Language.Ast;

public abstract class AstNodeExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitExtension(Expression expression) =>
        expression switch
        {
            ModuleNode node => VisitModuleNode(node),
            FunctionNode node => VisitFunctionNode(node),
            ParamGroupNode node => VisitParamGroupNode(node),

            TypeNode node => VisitTypeNode(node),
            
            ExprListNode node => VisitExprListNode(node),

            IfNode node => VisitIfNode(node),
            IfNode.ElseIfNode node => VisitElseIfNode(node),

            BinaryOpNode node => VisitBinaryOpNode(node),
            UnaryOpNode node => VisitUnaryOpNode(node),

            NumberNode node => VisitNumberNode(node),
            BoolNode node => VisitBoolNode(node),
            IdentifierNode node => VisitIdentifierNode(node),

            AstNodeExpression node => VisitAstNodeExpression(node),
            _ => base.VisitExtension(expression)
        };

    protected abstract Expression VisitUnaryOpNode(UnaryOpNode node);

    protected abstract Expression VisitAstNodeExpression(AstNodeExpression node);

    protected abstract Expression VisitExprListNode(ExprListNode node);

    protected abstract Expression VisitMissedNode(ExprListNode.MissedNode node);

    protected abstract Expression VisitModuleNode(ModuleNode node);

    protected abstract Expression VisitFunctionNode(FunctionNode node);

    protected abstract Expression VisitTypeNode(TypeNode node);

    protected abstract Expression VisitParamGroupNode(ParamGroupNode node);

    protected abstract Expression VisitElseIfNode(IfNode.ElseIfNode node);

    protected abstract Expression VisitIfNode(IfNode node);

    protected abstract Expression VisitBinaryOpNode(BinaryOpNode node);

    protected abstract Expression VisitNumberNode(NumberNode node);

    protected abstract Expression VisitBoolNode(BoolNode node);

    protected abstract Expression VisitIdentifierNode(IdentifierNode node);

    public class DefaultAstNodeExpressionVisitor : AstNodeExpressionVisitor
    {
        protected override Expression VisitUnaryOpNode(UnaryOpNode node) => node;

        protected override Expression VisitAstNodeExpression(AstNodeExpression node) => node;

        protected override Expression VisitExprListNode(ExprListNode node) => node;
       
        protected override Expression VisitMissedNode(ExprListNode.MissedNode node)
        {
            return node;
        }

        protected override Expression VisitModuleNode(ModuleNode node) => node;

        protected override Expression VisitFunctionNode(FunctionNode node) => node;
       
        protected override Expression VisitTypeNode(TypeNode node) => node;

        protected override Expression VisitParamGroupNode(ParamGroupNode node) => node;

        protected override Expression VisitElseIfNode(IfNode.ElseIfNode node) => node;

        protected override Expression VisitIfNode(IfNode node) => node;

        protected override Expression VisitBinaryOpNode(BinaryOpNode node) => node;

        protected override Expression VisitNumberNode(NumberNode node) => node;

        protected override Expression VisitBoolNode(BoolNode node) => node;

        protected override Expression VisitIdentifierNode(IdentifierNode node) => node;
    }
}