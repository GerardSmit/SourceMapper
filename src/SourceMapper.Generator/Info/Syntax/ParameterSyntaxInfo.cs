using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class ParameterSyntaxInfo : ParameterInfo
    {
        private TypeInfo? _parameterType;
        private readonly MethodSyntaxInfoBase _declaringMethod;

        public ParameterSyntaxInfo(ParameterSyntax syntax, MethodSyntaxInfoBase declaringMethod)
        {
            Syntax = syntax;
            _declaringMethod = declaringMethod;
        }

        public ParameterSyntax Syntax { get; }

        public SemanticModel Model => _declaringMethod.Model;

        public override string Name => Syntax.Identifier.Text;

        public override MethodInfo DeclaringMethod => _declaringMethod;

        public override TypeInfo Type => _parameterType ??= TypeInfo.From(_declaringMethod.Model, Syntax.Type!);

        public override ExpressionSyntax? GetDefaultSyntax() => Syntax.Default?.Value;
    }
}