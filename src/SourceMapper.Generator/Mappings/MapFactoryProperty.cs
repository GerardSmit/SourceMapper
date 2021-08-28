using Microsoft.CodeAnalysis.CSharp.Syntax;
using Space.SourceGenerator.Client;

namespace SourceMapper.Generator
{
    public class MapFactoryProperty
    {
        public MapFactoryProperty(PropertyInfo targetProperty, ExpressionSyntax expression)
        {
            TargetProperty = targetProperty;
            Expression = expression;
        }

        public PropertyInfo TargetProperty { get; }

        public ExpressionSyntax Expression { get; }
    }
}