using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public interface ITypeReference
    {
        IEnumerable<TypeInfo> GetReferencedTypes();
    }

    public abstract class TypeInfo : ITypeReference
    {
        public abstract string Name { get; }

        public abstract string? Namespace { get; }

        public virtual bool SyntaxAvailable => false;

        public abstract IReadOnlyList<PropertyInfo> Properties { get; }

        public abstract IReadOnlyList<MethodInfo> Methods { get; }

        public abstract bool IsGenericType { get; }

        public abstract IReadOnlyList<TypeInfo> TypeParameters { get; }

        public bool IsType(TypeInfo? info)
        {
            if (info == null) return false;
            return Name == info.Name && Namespace == info.Namespace;
        }

        public bool IsType(ITypeSymbol? info)
        {
            if (info == null) return false;
            return Name == info.Name && Namespace == info.ContainingNamespace?.ToString();
        }

        public IEnumerable<TypeInfo> GetReferencedTypes()
        {
            return new[] { this };
        }

        public override string ToString()
        {
            return IsGenericType
                ? $"{Name}<{string.Join(", ", TypeParameters)}>"
                : Name;
        }

        public static TypeSyntaxInfo From(SemanticModel model, TypeSyntax syntax, ITypeSymbol symbol)
        {
            return new TypeSyntaxInfo(symbol, syntax, model);
        }

        public static TypeSyntaxInfo From(SemanticModel model, TypeSyntax syntax)
        {
            var symbol = model.GetTypeInfo(syntax).Type;

            if (symbol == null)
            {
                throw new InvalidOperationException("Could not find the type symbol");
            }

            return new TypeSyntaxInfo(symbol, syntax, model);
        }

        public static ClassSyntaxInfo From(SemanticModel model, ClassDeclarationSyntax syntax)
        {
            var symbol = model.GetDeclaredSymbol(syntax) as ITypeSymbol;

            if (symbol == null)
            {
                throw new InvalidOperationException("Could not find the type symbol");
            }

            return new ClassSyntaxInfo(symbol, syntax, model);
        }

        public static TypeSymbolInfo From(ITypeSymbol symbol) => new(symbol);
    }
}