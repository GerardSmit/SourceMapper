using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class MethodSyntaxInfo : MethodSyntaxInfoBase
    {
        private TypeInfo? _returnType;
        private readonly ClassSyntaxInfo _declaringType;
        private IReadOnlyList<ParameterInfo>? _parameters;

        public MethodSyntaxInfo(MethodDeclarationSyntax syntax, ClassSyntaxInfo declaringType)
        {
            Syntax = syntax;
            _declaringType = declaringType;
            Name = syntax.Identifier.Text;
        }

        public MethodDeclarationSyntax Syntax { get; }

        public override SemanticModel Model => _declaringType.Model;

        public override string Name { get; }

        public override TypeInfo DeclaringType => _declaringType;

        public override TypeInfo? ReturnType => _returnType ??= TypeInfo.From(_declaringType.Model, Syntax.ReturnType);

        public override IReadOnlyList<ParameterInfo> Parameters =>
            _parameters ??= Syntax.ParameterList.Parameters
                .Select(i => new ParameterSyntaxInfo(i, this))
                .ToArray();

        public override Location GetLocation() => Syntax.GetLocation();

        protected override ArrowExpressionClauseSyntax? ExpressionBody => Syntax.ExpressionBody;

        protected override BlockSyntax? Body => Syntax.Body;
    }
}