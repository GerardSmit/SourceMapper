using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;

namespace SourceMapper.Generator
{
    public class MapFactory
    {
        public MapFactory(MethodInfo method)
        {
            Method = method;
            SourceParameterName = method.Parameters[0].Name;
            Parameters = method.Parameters.Skip(1).ToList();
        }

        public List<ParameterInfo> Parameters { get; set; }

        public string SourceParameterName { get; }

        public MethodInfo Method { get; }

        public List<StatementSyntax> Statements { get; } = new();

        public List<MapFactoryVariable> Variables { get; } = new();

        public List<MapFactoryProperty> Properties { get; } = new();

        public string ToCode(string prefix = "")
        {
            if (Parameters.Count == 0)
            {
                return "";
            }

            return prefix + string.Join(", ", Parameters.Select(i =>
            {
                var defaultValue = i.GetDefaultSyntax();
                var defaultCode = defaultValue == null ? "" : $" = {defaultValue.ToFullString()}";
                return $"{i.Type} {i.Name}{defaultCode}";
            }));
        }

        public string ToParameters(string prefix = "")
        {
            if (Parameters.Count == 0)
            {
                return "";
            }

            return prefix + string.Join(", ", Parameters.Select(i => i.Name));
        }
    }
}