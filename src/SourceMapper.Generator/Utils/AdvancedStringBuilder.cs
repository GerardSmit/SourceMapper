using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Space.SourceGenerator.Client
{
    internal class AdvancedStringBuilder
    {
        private static readonly string[] Increments = Enumerable.Range(0, 10)
            .Select(i => i == 0 ? string.Empty : new string(' ', i * 4))
            .ToArray();

        private readonly Compilation _compilation;
        private readonly CloseScope _closeScope;
        private readonly StringBuilder _stringBuilder = new();
        private readonly HashSet<string> _usings = new();
        private readonly List<INamespaceSymbol> _registeredSymbols = new();
        private readonly Dictionary<SyntaxTree, SemanticModel?> _models = new();
        private readonly Stack<(ScopeType? type, string? suffix, bool newLine)> _scopes = new();
        private ScopeType? _lastScope;
        private int _increment;
        private bool _addIncrement;

        public AdvancedStringBuilder(Compilation compilation)
        {
            _compilation = compilation;
            _closeScope = new CloseScope(this);
        }

        public void AppendLine()
        {
            _stringBuilder.AppendLine();
            _addIncrement = true;
        }

        public void AppendLineRaw(string str)
        {
            AppendRaw(str);
            AppendLine();
        }

        public void Append(FormattableString str)
        {
            AppendIncrement();
            _stringBuilder.Append(str);

            foreach (var arg in str.GetArguments())
            {
                IEnumerable<TypeInfo> types;

                switch (arg)
                {
                    case ITypeReference typeReference:
                        types = typeReference.GetReferencedTypes();
                        break;
                    case ExpressionSyntax syntax:
                        types = syntax.DescendantNodes()
                            .OfType<TypeSyntax>()
                            .Select(t =>
                            {
                                var model = GetModel(t.SyntaxTree);

                                return new
                                {
                                    Model = model,
                                    Type = t,
                                    Symbol = model?.GetTypeInfo(t).Type
                                };
                            })
                            .Where(t => t.Model != null && t.Symbol != null)
                            .Select(t => TypeInfo.From(t.Model!, t.Type, t.Symbol!));
                        break;
                    default:
                        continue;
                }

                foreach (var type in types)
                {
                    RegisterUsing(type.Namespace);
                }
            }
        }

        public void AppendLine(FormattableString str)
        {
            Append(str);
            AppendLine();
        }

        public void Append(TypeInfo info)
        {
            RegisterUsing(info.Namespace);
            AppendRaw(info.Name);
        }

        public void AppendRaw(string str)
        {
            AppendIncrement();
            _stringBuilder.Append(str);
        }

        public void Append(char chr)
        {
            AppendIncrement();
            _stringBuilder.Append(chr);
        }

        public IDisposable Namespace(string name)
        {
            AppendRaw("namespace ");
            AppendLineRaw(name);
            return BeginScope(ScopeType.Namespace);
        }

        public IDisposable Struct(string name, Declaration declaration = Declaration.None)
        {
            return Object(ScopeType.Struct, name, declaration);
        }

        public IDisposable Class(string name, Declaration declaration = Declaration.None)
        {
            return Object(ScopeType.Class, name, declaration);
        }

        private IDisposable Object(ScopeType type, string name, Declaration declaration = Declaration.None)
        {
            PrepareScope(type);

            if (declaration.HasFlag(Declaration.IsInternal)) AppendRaw("internal ");
            else if (declaration.HasFlag(Declaration.IsPrivate)) AppendRaw("private ");
            else AppendRaw("public ");

            if (declaration.HasFlag(Declaration.IsStatic))
            {
                AppendRaw("static ");
            }

            if (declaration.HasFlag(Declaration.IsReadOnly))
            {
                AppendRaw("readonly ");
            }

            if (declaration.HasFlag(Declaration.IsPartial))
            {
                AppendRaw("partial ");
            }

            AppendRaw(type switch
            {
                ScopeType.Class => "class ",
                ScopeType.Struct => "struct ",
                _ => throw new InvalidOperationException()
            });
            AppendLineRaw(name);
            return BeginScope(type);
        }

        public IDisposable BeginScope(string? suffix)
        {
            return BeginScope(null, suffix);
        }

        public IDisposable BeginScope(ScopeType? scope = null, string? suffix = null, bool newLine = true)
        {
            _scopes.Push((scope, suffix, newLine));
            Append('{');
            AppendLine();
            Increment();
            return _closeScope;
        }
        
        public void EndScope(string? suffix = null)
        {
            string? scopeSuffix = null;
            var newLine = true;

            if (_scopes.Count > 0)
            {
                ScopeType? type;
                (type, scopeSuffix, newLine) = _scopes.Pop();
                _lastScope = type;
            }

            Decrement();
            Append('}');
            if (suffix != null)
            {
                AppendRaw(suffix);
            }

            if (scopeSuffix != null)
            {
                AppendRaw(scopeSuffix);
            }

            if (newLine)
            {
                AppendLine();
            }
        }

        public void PrepareScope(ScopeType? type)
        {
            if (_lastScope == null)
            {
                return;
            }

            if (type == _lastScope)
            {
                AppendLine();
            }

            _lastScope = null;
        }

        private void AppendIncrement()
        {
            if (_addIncrement)
            {
                if (_increment > 0)
                {
                    _stringBuilder.Append(Increments[_increment]);
                }

                _addIncrement = false;
            }
        }

        public void RegisterUsing(params string[] names)
        {
            foreach (var name in names)
            {
                RegisterUsing(name);
            }
        }

        public void RegisterUsing(string? name)
        {
            if (name is { Length: > 0 })
            {
                _usings.Add(name);
            }
        }

        public SemanticModel? GetModel(SyntaxTree tree)
        {
            if (_models.TryGetValue(tree, out var model))
            {
                return model;
            }

            try
            {
                model = _compilation.GetSemanticModel(tree);
            }
            catch
            {
                model = null;
            }
            _models[tree] = model;
            return model;
        }

        public void Increment() => _increment++;

        public void Decrement() => _increment--;

        public override string ToString()
        {
            while (_scopes.Count > 0)
            {
                EndScope();
            }

            if (_usings.Count == 0)
            {
                return _stringBuilder.ToString();
            }

            var sb = new StringBuilder();

            foreach (var @using in _usings.OrderBy(i => i))
            {
                sb.Append("using ");
                sb.Append(@using);
                sb.AppendLine(";");
            }

            sb.AppendLine();

            sb.Append(_stringBuilder);
            return sb.ToString();
        }

        private class CloseScope : IDisposable
        {
            private readonly AdvancedStringBuilder _builder;

            public CloseScope(AdvancedStringBuilder builder)
            {
                _builder = builder;
            }

            public void Dispose()
            {
                _builder.EndScope();
            }
        }
    }

    public enum ScopeType
    {
        Namespace,
        Class,
        Struct,
        Method
    }

    [Flags]
    public enum Declaration
    {
        None = 0,
        IsPartial = 1,
        IsInternal = 2,
        IsStatic = 4,
        IsPrivate = 8,
        IsReadOnly = 16
    }
}