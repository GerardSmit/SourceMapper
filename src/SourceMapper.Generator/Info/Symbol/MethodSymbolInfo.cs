using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class MethodSymbolInfo : MethodInfo
    {
        private TypeInfo? _returnType;
        private IReadOnlyList<ParameterInfo>? _parameters;

        public MethodSymbolInfo(IMethodSymbol symbol, TypeSymbolInfo declaringType)
        {
            Symbol = symbol;
            DeclaringType = declaringType;
        }

        public IMethodSymbol Symbol { get; }

        public override string Name => Symbol.Name;

        public override TypeInfo DeclaringType { get; }

        public override TypeInfo? ReturnType => _returnType ??= TypeInfo.From(Symbol.ReturnType);

        public override IReadOnlyList<ParameterInfo> Parameters => _parameters ??= Symbol.Parameters
            .Select(i => new ParameterSymbolInfo(i, this))
            .ToArray();
    }
}