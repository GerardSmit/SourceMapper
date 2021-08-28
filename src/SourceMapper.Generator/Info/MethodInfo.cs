using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = Space.SourceGenerator.Client.TypeInfo;

namespace SourceMapper.Generator.Info
{
    public abstract class MethodInfo
    {
        public abstract string Name { get; }

        public abstract TypeInfo DeclaringType { get; }

        public abstract TypeInfo? ReturnType { get; }

        public abstract IReadOnlyList<ParameterInfo> Parameters { get; }

        public virtual Location? GetLocation() => null;

        public virtual ExpressionSyntax? GetExpression() => null;

        public virtual BlockSyntax? GetBody() => null;

        public IEnumerable<TypeInfo> GetReferencedTypes()
        {
            return ReturnType != null ? new[] { ReturnType } : Array.Empty<TypeInfo>();
        }

        public override string ToString()
        {
            return Name;
        }

        public string GetDeclaration()
        {
            return $"{Name}({string.Join(", ", Parameters.Select(i => i.ToFullString()))})";
        }

        public string GetParameterWithTypes()
        {
            return string.Join(", ", Parameters.Select(i => $"{i.Type} {i.Name}"));
        }

        public string GetCall()
        {
            return $"{Name}({string.Join(", ", Parameters.Select(i => i.Name))})";
        }
    }
}
