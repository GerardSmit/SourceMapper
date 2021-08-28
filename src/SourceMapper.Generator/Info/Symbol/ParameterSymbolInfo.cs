using Microsoft.CodeAnalysis;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class ParameterSymbolInfo : ParameterInfo
    {
        private TypeInfo? _parameterType;

        public ParameterSymbolInfo(IParameterSymbol symbol, MethodSymbolInfo declaringMethod)
        {
            Symbol = symbol;
            DeclaringMethod = declaringMethod;
        }

        public IParameterSymbol Symbol { get; }

        public override string Name => Symbol.Name;

        public override MethodInfo DeclaringMethod { get; }

        public override TypeInfo Type => _parameterType ??= TypeInfo.From(Symbol.Type);
    }
}