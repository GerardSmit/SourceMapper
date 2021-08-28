using Microsoft.CodeAnalysis;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class PropertySymbolInfo : PropertyInfo
    {
        private MethodInfo? _getter;
        private MethodInfo? _setter;
        private TypeSymbolInfo _declaringType;

        public PropertySymbolInfo(IPropertySymbol symbol, TypeSymbolInfo declaringType)
        {
            Symbol = symbol;
            _declaringType = declaringType;
        }

        public override TypeInfo DeclaringType => _declaringType;

        public override TypeInfo Type => TypeInfo.From(Symbol.Type);

        private IPropertySymbol Symbol { get; }

        public override string Name => Symbol.Name;

        public override bool CanRead => !Symbol.IsWriteOnly;

        public override bool CanWrite => !Symbol.IsReadOnly;

        public override MethodInfo? GetMethod => Symbol.GetMethod != null
            ? _getter ??= new MethodSymbolInfo(Symbol.GetMethod, _declaringType)
            : null;

        public override MethodInfo? SetMethod => Symbol.SetMethod != null
            ? _setter ??= new MethodSymbolInfo(Symbol.SetMethod, _declaringType)
            : null;
    }
}