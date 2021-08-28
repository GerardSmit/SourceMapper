using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public abstract class PropertyInfo : ITypeReference
    {
        public abstract string Name { get; }

        public abstract TypeInfo DeclaringType { get; }

        public abstract TypeInfo Type { get; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public abstract MethodInfo? GetMethod { get; }

        public abstract MethodInfo? SetMethod { get; }

        public virtual bool SyntaxAvailable => false;

        public virtual Location? GetLocation() => null;

        public IEnumerable<TypeInfo> GetReferencedTypes()
        {
            return new[] { Type };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}