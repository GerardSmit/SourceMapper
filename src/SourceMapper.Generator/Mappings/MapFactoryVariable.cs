using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;
using Space.SourceGenerator.Client;

namespace SourceMapper.Generator
{
    public class MapFactoryVariable : IParameter
    {
        public MapFactoryVariable(string name, ExpressionSyntax expression, TypeInfo type)
        {
            Name = name;
            Expression = expression;
            Type = type;
        }

        public string Name { get; }

        public ExpressionSyntax Expression { get; }

        public TypeInfo Type { get; }

        public override string ToString()
        {
            return Name;
        }

        public ExpressionSyntax GetDefaultSyntax()
        {
            return Expression;
        }
    }
}