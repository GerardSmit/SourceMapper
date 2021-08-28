using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class ClassSyntaxInfo : TypeSymbolInfo
    {
        private IReadOnlyList<PropertyInfo>? _properties;
        private IReadOnlyList<MethodInfo>? _methods;

        public ClassSyntaxInfo(ITypeSymbol symbol, ClassDeclarationSyntax syntax, SemanticModel model)
            : base(symbol)
        {
            Syntax = syntax;
            Model = model;
        }
        
        public ClassDeclarationSyntax Syntax { get; }

        public SemanticModel Model { get; }

        public override bool SyntaxAvailable => true;

        public override IReadOnlyList<PropertyInfo> Properties =>
            _properties ??= Syntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(i => new PropertySyntaxInfo(i, this))
                .ToArray();

        public override IReadOnlyList<MethodInfo> Methods =>
            _methods ??= Syntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(i => new MethodSyntaxInfo(i, this))
                .ToArray();
    }
}