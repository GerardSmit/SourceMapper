using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class TypeSymbolInfo : TypeInfo
    {
        private IReadOnlyList<PropertyInfo>? _properties;
        private IReadOnlyList<MethodInfo>? _methods;
        private IReadOnlyList<TypeInfo>? _typeParameters;

        public TypeSymbolInfo(ITypeSymbol symbol)
        {
            Symbol = symbol;
            Namespace = symbol.ContainingNamespace?.ToString();
        }
        
        public ITypeSymbol Symbol { get; }

        public override string Name => Symbol.Name;

        public override string? Namespace { get; }

        public override IReadOnlyList<PropertyInfo> Properties =>
            _properties ??= Symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Select(i => new PropertySymbolInfo(i, this))
                .ToArray();

        public override IReadOnlyList<MethodInfo> Methods =>
            _methods ??= Symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Select(i => new MethodSymbolInfo(i, this))
                .ToArray();

        public override bool IsGenericType => Symbol is INamedTypeSymbol {IsGenericType: true};

        public override IReadOnlyList<TypeInfo> TypeParameters =>
            _typeParameters ??= Symbol is INamedTypeSymbol types
                ? types.TypeParameters.Select(From).ToArray()
                : Array.Empty<TypeInfo>();
    }
}