using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace Space.SourceGenerator.Client
{
    public class PropertySyntaxInfo : PropertyInfo
    {
        private readonly ClassSyntaxInfo _declaringType;
        private TypeInfo? _type;
        private MethodSyntaxInfoBase? _getter;
        private MethodSyntaxInfoBase? _setter;
        private readonly AccessorDeclarationSyntax? _readSyntax;
        private readonly AccessorDeclarationSyntax? _writeSyntax;

        public PropertySyntaxInfo(PropertyDeclarationSyntax syntax, ClassSyntaxInfo declaringType)
        {
            Syntax = syntax;
            _declaringType = declaringType;
            Name = syntax.Identifier.Text;
            _readSyntax = syntax.AccessorList?.Accessors.FirstOrDefault(i => i.Kind() == SyntaxKind.GetAccessorDeclaration);
            _writeSyntax = syntax.AccessorList?.Accessors.FirstOrDefault(i => i.Kind() == SyntaxKind.SetAccessorDeclaration);
        }

        public override TypeInfo DeclaringType => _declaringType;

        public override TypeInfo Type => _type ??= TypeInfo.From(Model, Syntax.Type);

        public SemanticModel Model => _declaringType.Model;

        public PropertyDeclarationSyntax Syntax { get; }

        public override string Name { get; }

        public override bool CanRead => _readSyntax != null;

        public override bool CanWrite => _writeSyntax != null;

        public override MethodInfo? GetMethod => _readSyntax != null ?
            _getter ??= new PropertyAccessorSyntaxInfo(this, _readSyntax)
            : null;

        public override MethodInfo? SetMethod => _writeSyntax != null
            ? _setter ??= new PropertyAccessorSyntaxInfo(this, _writeSyntax)
            : null;

        public override bool SyntaxAvailable => true;

        public override Location GetLocation()
        {
            return Syntax.Identifier.GetLocation();
        }
    }
}