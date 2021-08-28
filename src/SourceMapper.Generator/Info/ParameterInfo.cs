using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Space.SourceGenerator.Client;

namespace SourceMapper.Generator.Info
{
    public interface IParameter
    {
        TypeInfo Type { get; }

        string Name { get; }

        ExpressionSyntax? GetDefaultSyntax();
    }

    public abstract class ParameterInfo : ITypeReference, IParameter
    {
        public abstract string Name { get; }

        public abstract MethodInfo DeclaringMethod { get; }

        public abstract TypeInfo Type { get; }

        public virtual ExpressionSyntax? GetDefaultSyntax() => null;

        public IEnumerable<TypeInfo> GetReferencedTypes()
        {
            return new[] { Type };
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToFullString()
        {
            return $"{Type} {Name}";
        }
    }
}