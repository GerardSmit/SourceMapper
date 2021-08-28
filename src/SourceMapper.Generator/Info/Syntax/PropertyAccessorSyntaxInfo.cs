using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class PropertyAccessorSyntaxInfo : MethodSyntaxInfoBase
    {
        private readonly PropertySyntaxInfo _declaringProperty;
        private readonly bool _isGetter;
        private ParameterInfo[]? _parameters;

        public PropertyAccessorSyntaxInfo(PropertySyntaxInfo declaringProperty, AccessorDeclarationSyntax syntax)
        {
            Syntax = syntax;
            _declaringProperty = declaringProperty;
            _isGetter = syntax.IsKind(SyntaxKind.GetAccessorDeclaration);
            Name = (_isGetter ? "get_" : "set_") + _declaringProperty.Name;
        }

        public AccessorDeclarationSyntax Syntax { get; }

        public override string Name { get; }

        public override TypeInfo DeclaringType => _declaringProperty.DeclaringType;

        public override TypeInfo? ReturnType => _isGetter ? _declaringProperty.Type : null;

        public override IReadOnlyList<ParameterInfo> Parameters =>
            _parameters ??= _isGetter ? Array.Empty<ParameterInfo>() : new ParameterInfo[]
            {
                new ParameterSyntaxInfo(
                    SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), _declaringProperty.Syntax.Type, SyntaxFactory.Identifier("value"), null),
                    this
                )
            };

        public override SemanticModel Model => _declaringProperty.Model;

        protected override ArrowExpressionClauseSyntax? ExpressionBody => Syntax.ExpressionBody;

        protected override BlockSyntax? Body => Syntax.Body;
    }
}