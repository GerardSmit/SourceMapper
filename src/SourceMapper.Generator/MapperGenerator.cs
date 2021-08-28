using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceMapper.Generator;
using SourceMapper.Generator.Info;
using SourceMapper.Generator.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Space.SourceGenerator.Client.Declaration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Space.SourceGenerator.Client
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Debugger.Launch();

            var maps = context.Compilation.SyntaxTrees
                .SelectMany(st =>
                {
                    SemanticModel? model = null;
                    SemanticModel GetModel() => model ??= context.Compilation.GetSemanticModel(st);

                    return st.GetRoot()
                        .DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .SelectMany(c => c.AttributeLists.SelectMany(a => a.Attributes)
                            .Where(a => a.Name.ToFullString() == "MapFrom" && a is { ArgumentList: { Arguments: { Count: 1 } }})
                            .Select(a => a.ArgumentList!.Arguments[0].Expression)
                            .OfType<TypeOfExpressionSyntax>()
                            .Select(t => ModelExtensions.GetTypeInfo(GetModel(), t.Type).Type?.ToString())
                            .Select(t => t != null ? context.Compilation.GetTypeByMetadataName(t) : null)
                            .Where(t => t != null)
                            .Select(t => Map.Create(context, TypeInfo.From(t!), TypeInfo.From(GetModel(), c))));
                });
            
            var sb = new AdvancedStringBuilder(context.Compilation);

            sb.RegisterUsing("System");
            sb.RegisterUsing("System.Linq");
            sb.RegisterUsing("System.Collections.Generic");
            sb.RegisterUsing("System.Linq.Expressions");
            sb.RegisterUsing("System.Diagnostics");
            sb.RegisterUsing("System.CodeDom.Compiler");
            sb.RegisterUsing("System.Reflection");

            sb.Namespace("SourceMapper");

            foreach (var map in maps)
            {
                sb.PrepareScope(ScopeType.Class);
                sb.AppendLineRaw("/// <summary>");
                sb.AppendLine($"/// Automatically generated mapper for <see cref=\"{map.TargetType}\"/>.");
                sb.AppendLine($"/// To stop generating this class, remove attribute <see cref=\"MapFromAttribute\"/> from <see cref=\"{map.TargetType}\"/>.");
                sb.AppendLineRaw("/// </summary>");
                sb.AppendLineRaw("[DebuggerStepThrough]");
                sb.AppendLineRaw("[GeneratedCodeAttribute(\"SourceMapper\", \"1.0.0.0\")]");
                using (sb.Class($"{map.TargetType}Mapper", IsStatic))
                {
                    if (map.Factories.Count > 0)
                    {
                        foreach (var factory in map.Factories)
                        {
                            CreateFactoryMapping(map, sb, factory);
                        }
                    }
                    else
                    {
                        CreateDefaultMapping(map, sb);
                    }
                }
            }

            var result = sb.ToString();
            // File.WriteAllText(@"C:\Temp\Data.cs", result);
            context.AddSource("Mapper.g", SourceText.From(result, Encoding.UTF8));
        }

        private static void CreateFactoryMapping(Map map, AdvancedStringBuilder sb, MapFactory factory)
        {
            if (factory is {Parameters: {Count: 0}, Variables: {Count: 0}})
            {
                CreateDefaultMapping(map, sb);
                return;
            }

            var hash = factory.Method.GetDeclaration().ToMd5().Substring(0, 6);
            var factoryProperties = factory.Properties.Select(i => i.TargetProperty.Name).ToList();
            var mapProperties = map.Properties.Where(i => !factoryProperties.Contains(i.TargetProperty.Name)).ToList();
            var target = map.TargetType;
            var source = map.SourceType;

            sb.AppendLine($"#region Factory {factory.Method.GetDeclaration()}");
            sb.AppendLine();

            // Class: Scope
            string? middleware = null;
            var paramsName = $"Params_{hash}";
            var targetName = paramsName;

            sb.AppendLine($"private static readonly ParameterExpression e_{factory.SourceParameterName}_{hash} = Expression.Parameter(typeof({map.SourceType}), \"{factory.SourceParameterName}\");");
            sb.AppendLine($"private static readonly Type t_{hash} = typeof({paramsName});");
            foreach (var parameter in factory.Method.Parameters)
            {
                sb.AppendLine($"private static readonly PropertyInfo p_{parameter.Name}_{hash} = t_{hash}.GetProperty(\"{parameter.Name}\");");
            }

            sb.AppendLine();

            using (sb.Class(paramsName, IsPrivate))
            {
                foreach (var parameter in factory.Method.Parameters)
                {
                    sb.AppendLine($"public {parameter.Type} {parameter.Name} {{ get; set; }} ");
                }
            }

            // Class: Variables
            if (factory.Variables.Count > 0)
            {
                var varsName = $"Vars_{hash}";
                middleware = $"VarsExpression_{hash}";
                targetName = varsName;

                using (sb.Class(varsName, IsPrivate))
                {
                    foreach (var parameter in factory.Method.Parameters
                        .Cast<IParameter>()
                        .Concat(factory.Variables))
                    {
                        sb.AppendLine($"public {parameter.Type} {parameter.Name} {{ get; set; }} ");
                    }
                }

                sb.AppendLine();

                // Static Field: VarsExpression
                sb.AppendLine($"private static readonly Expression<Func<{paramsName}, {varsName}>> {middleware} = _params => new {varsName}");
                using (sb.BeginScope(";"))
                {
                    var parameters = factory.Method.Parameters.Select(i => i.Name).ToList();

                    foreach (var parameter in factory.Method.Parameters)
                    {
                        sb.AppendLine($"{parameter.Name} = _params.{parameter.Name},");
                    }

                    foreach (var property in factory.Variables)
                    {
                        var replaceNodes = property.Expression
                            .DescendantNodesAndSelf()
                            .OfType<IdentifierNameSyntax>()
                            .Where(i => parameters.Contains(i.Identifier.Text));

                        var expression = property.Expression
                            .ReplaceNodes(replaceNodes, (i, _) => MemberAccessExpression(SimpleMemberAccessExpression, IdentifierName("_params"), i));

                        sb.AppendLine($"{property.Name} = {expression},");
                    }
                }
            }

            sb.AppendLine();

            // Static Field: Expression
            sb.AppendLine($"private static readonly Expression<Func<{targetName}, {target}>> Expression_{hash} = _params => new {target}");
            using (sb.BeginScope(";"))
            {
                var variables = factory.Method.Parameters.Select(i => i.Name)
                    .Concat(factory.Variables.Select(i => i.Name))
                    .ToList();

                foreach (var property in factory.Properties)
                {
                    var replaceNodes = property.Expression
                        .DescendantNodesAndSelf()
                        .OfType<IdentifierNameSyntax>()
                        .Where(i => variables.Contains(i.Identifier.Text));

                    var expression = property.Expression
                        .ReplaceNodes(replaceNodes, (i, _) => MemberAccessExpression(SimpleMemberAccessExpression, IdentifierName("_params"), i));

                    sb.AppendLine($"{property.TargetProperty} = {expression},");
                }

                foreach (var property in mapProperties)
                {
                    sb.AppendLine($"{property.TargetProperty} = _params.{factory.SourceParameterName}.{property.SourceProperty},");
                }
            }

            sb.AppendLine();

            // Method: Map
            sb.AppendLine($"public static {target} Map({factory.Method.GetParameterWithTypes()})");
            using (sb.BeginScope())
            {
                sb.AppendLine($"var result = {factory.Method.DeclaringType}.{factory.Method.GetCall()};");

                foreach (var property in mapProperties)
                {
                    sb.AppendLine($"result.{property.TargetProperty} = {factory.SourceParameterName}.{property.SourceProperty};");
                }

                sb.AppendLineRaw("return result;");
            }

            sb.AppendLine();

            // Queryable extension
            sb.AppendLine($"public static IQueryable<{target}> To{target}(this IQueryable<{source}> query{factory.ToCode(", ")})");
            using (sb.BeginScope())
            {
                if (factory.Statements.Count > 0)
                {
                    foreach (var statement in factory.Statements)
                    {
                        sb.AppendLineRaw(statement.ToString());
                    }

                    sb.AppendLine();
                }

                sb.AppendLineRaw("return query");

                // The following is the compiled version of the following:
                //
                //      sb.AppendLine($".Select({factory.SourceParameterName} => new {paramsName}");
                //      using (sb.BeginScope(newLine: false))
                //      {
                //          foreach (var parameter in factory.Method.Parameters)
                //          {
                //              sb.AppendLine($"{parameter} = {parameter},");
                //          }
                //      }
                //      sb.AppendLine($")");
                //
                // We cache the properties in fields for less allocations.

                sb.Increment();
                sb.AppendLine($".Select(Expression.Lambda<Func<{map.SourceType}, {paramsName}>>(");
                sb.Increment();
                sb.AppendLineRaw("Expression.MemberInit(");
                sb.Increment();

                sb.AppendLine($"Expression.New(t_{hash}),");
                sb.AppendLineRaw("new MemberBinding[]");
                sb.AppendLineRaw("{");
                sb.Increment();

                sb.AppendLine($"Expression.Bind(p_{factory.SourceParameterName}_{hash}, e_{factory.SourceParameterName}_{hash}),");

                foreach (var parameter in factory.Parameters)
                {
                    sb.AppendLine($"Expression.Bind(p_{parameter}_{hash}, Expression.Constant({parameter.Name})),");
                }

                sb.Decrement();
                sb.AppendLineRaw("}");

                sb.Decrement();
                sb.AppendLineRaw("),");
                sb.AppendLine($"e_{factory.SourceParameterName}_{hash}");
                sb.Decrement();
                sb.AppendLineRaw("))");

                if (middleware != null)
                {
                    sb.AppendLine($".Select({middleware})");
                }
                sb.AppendLine($".Select(Expression_{hash});");
                sb.Decrement();
            }
            sb.AppendLine();

            // Enumerable extension
            sb.AppendLine($"public static IEnumerable<{target}> To{target}(this IEnumerable<{source}> query{factory.ToCode(", ")}) => query");
            sb.Increment();
            sb.AppendLine($".Select(x => Map(x{factory.ToParameters(", ")}));");
            sb.Decrement();
            sb.AppendLine();

            // Type extension
            sb.AppendLine($"public static {target} To{target}(this {source} {factory.SourceParameterName}{factory.ToCode(", ")}) =>");
            sb.Increment();
            sb.AppendLine($"Map({factory.SourceParameterName}{factory.ToParameters(", ")});");
            sb.Decrement();
            sb.AppendLine();

            sb.AppendLineRaw("#endregion");
        }

        private static void CreateDefaultMapping(Map map, AdvancedStringBuilder sb)
        {
            var target = map.TargetType;
            var source = map.SourceType;

            void AppendNewTarget()
            {
                sb.AppendLine($"new {target}()");
                using (sb.BeginScope(";"))
                {
                    foreach (var parameter in map.Properties)
                    {
                        sb.AppendLine($"{parameter.TargetProperty} = source.{parameter.SourceProperty},");
                    }
                }
            }

            sb.AppendLineRaw("#region No Factory");
            sb.AppendLine();

            // Static Field: NewFunc
            sb.AppendLine($"public static readonly Func<{source}, {target}> NewFunc = Map;");

            // Static Field: NewExpression
            sb.Append($"public static readonly Expression<Func<{source}, {target}>> NewExpression = source => ");
            AppendNewTarget();
            sb.AppendLine();

            // Method: Map
            sb.Append($"public static {target} Map({source} source) => ");
            AppendNewTarget();
            sb.AppendLine();

            // Source extension
            sb.AppendLine($"public static {target} To{target}(this {source} x) => Map(x);");
            sb.AppendLine($"public static IQueryable<{target}> To{target}(this IQueryable<{source}> query) => query.Select(NewExpression);");
            sb.AppendLine($"public static IEnumerable<{target}> To{target}(this IEnumerable<{source}> query) => query.Select(NewFunc);");
            sb.AppendLine();

            sb.AppendLineRaw("#endregion");
        }
    }
}
