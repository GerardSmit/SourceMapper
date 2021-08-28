using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public abstract class MethodSyntaxInfoBase : MethodInfo
    {
        private BlockSyntax? _expressionBlock;

        public abstract SemanticModel Model { get; }

        protected abstract ArrowExpressionClauseSyntax? ExpressionBody { get; }
        
        protected abstract BlockSyntax? Body { get; }

        public override ExpressionSyntax? GetExpression()
        {
            if (ExpressionBody != null)
            {
                return ExpressionBody.Expression;
            }

            if (Body is { Statements: { Count: 1 } statements } &&
                statements[0] is ReturnStatementSyntax returnStatement)
            {
                return returnStatement.Expression;
            }

            return null;
        }

        public override BlockSyntax? GetBody()
        {
            if (ExpressionBody != null)
            {
                return _expressionBlock ??= SyntaxFactory.Block(SyntaxFactory.ReturnStatement(ExpressionBody.Expression));
            }

            return Body;
        }
    }
}