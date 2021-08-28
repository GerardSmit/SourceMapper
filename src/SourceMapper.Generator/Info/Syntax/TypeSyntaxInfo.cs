using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Space.SourceGenerator.Client
{
    public class TypeSyntaxInfo : TypeSymbolInfo
    {
        private IReadOnlyList<TypeInfo>? _typeParameters;

        public TypeSyntaxInfo(ITypeSymbol symbol, TypeSyntax syntax, SemanticModel model)
            : base(symbol)
        {
            Syntax = syntax;
            Model = model;

            if (syntax is GenericNameSyntax)
            {

            }
        }
        
        public SemanticModel Model { get; }
        
        public TypeSyntax Syntax { get; }

        public override bool IsGenericType => Syntax is GenericNameSyntax;

        public override IReadOnlyList<TypeInfo> TypeParameters =>
            _typeParameters ??= Syntax is GenericNameSyntax types
                ? types.TypeArgumentList.Arguments.Select(i => From(Model, i)).ToArray()
                : Array.Empty<TypeInfo>();
    }
}