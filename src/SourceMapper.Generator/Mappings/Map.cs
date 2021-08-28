using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceMapper.Generator.Info;
using Space.SourceGenerator.Client;
using TypeInfo = Space.SourceGenerator.Client.TypeInfo;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceMapper.Generator
{
    public class Map
    {
        private Map(TypeInfo sourceType, ClassSyntaxInfo targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public TypeInfo SourceType { get; }

        public ClassSyntaxInfo TargetType { get; }

        public List<MapProperty> Properties { get; } = new();

        public List<MapFactory> Factories { get; } = new();

        public static Map Create(GeneratorExecutionContext context, TypeInfo sourceType, ClassSyntaxInfo targetType)
        {
            var map = new Map(sourceType, targetType);
            var sourceProperties = sourceType.Properties.ToDictionary(i => i.Name);

            foreach (var targetProperty in targetType.Properties)
            {
                if (!targetProperty.CanRead || !targetProperty.CanWrite)
                {
                    continue;
                }

                if (sourceProperties.TryGetValue(targetProperty.Name, out var sourceProperty))
                {
                    map.Properties.Add(new MapProperty(sourceProperty, targetProperty));
                }
            }

            foreach (var method in targetType.Methods.Where(i => i.Name == "Map"))
            {
                RegisterFactory(context, map, method);
            }

            return map;
        }

        private static void RegisterFactory(
            GeneratorExecutionContext context,
            Map map,
            MethodInfo method)
        {
            var syntax = method.GetBody();

            if (syntax == null)
            {
                return;
            }

            var sourceType = map.SourceType;
            var targetType = map.TargetType;

            if (method.Parameters.Count == 0 || !sourceType.IsType(method.Parameters[0].Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.InvalidFirstParameter,
                    method.GetLocation(),
                    method.Name,
                    targetType.Name,
                    sourceType.Name
                ));

                return;
            }

            if (syntax.Statements.LastOrDefault() is not ReturnStatementSyntax
            {
                Expression: ObjectCreationExpressionSyntax creationExpression
            })
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.NoReturn,
                    method.GetLocation(),
                    method.Name,
                    targetType.Name
                ));

                return;
            }

            var factory = new MapFactory(method);
            var statements = syntax.Statements.Take(syntax.Statements.Count - 1);
            var valid = true;

            if (creationExpression.Initializer is {Expressions: {Count: > 0} properties})
            {
                foreach (var property in properties)
                {
                    if (property is not AssignmentExpressionSyntax
                    {
                        Left: IdentifierNameSyntax {Identifier: {Text: { } propertyName}},
                        Right: { } initializer
                    })
                    {
                        continue;
                    }

                    var mapProperty = map.TargetType.Properties.FirstOrDefault(i => i.Name == propertyName);

                    if (mapProperty == null)
                    {
                        continue;
                    }

                    factory.Properties.Add(new MapFactoryProperty(mapProperty, initializer));
                }
            }

            var invalidVariables = new List<string>();
            var sourceName = method.Parameters[0].Name;

            foreach (var statement in statements)
            {
                var allowSource = statement is LocalDeclarationStatementSyntax;
                var invalidReference = statement
                    .DescendantNodesAndSelf()
                    .OfType<IdentifierNameSyntax>()
                    .FirstOrDefault(i => invalidVariables.Contains(i.Identifier.Text) || !allowSource && i.Identifier.Text == sourceName);

                var isSourceReference = invalidReference != null && invalidReference.Identifier.Text == sourceName;

                if (invalidReference != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        isSourceReference
                            ? Diagnostics.UnsupportedStatement
                            : Diagnostics.InvalidVariableReference,
                        statement.GetLocation(),
                        method.Name,
                        targetType.Name,
                        invalidReference.Identifier.Text,
                        sourceName
                    ));

                    valid = false;
                    continue;
                }

                if (statement is not LocalDeclarationStatementSyntax declaration)
                {
                    factory.Statements.Add(statement);
                    continue;
                }

                foreach (var declarationVariable in declaration.Declaration.Variables)
                {
                    if (declarationVariable.Initializer == null)
                    {
                        continue;
                    }

                    var expression = declarationVariable.Initializer.Value;

                    if (expression
                        .DescendantNodesAndSelf()
                        .OfType<IdentifierNameSyntax>()
                        .Any(i => i.Identifier.Text == sourceName))
                    {
                        var type = targetType.Model.GetTypeInfo(expression);

                        if (type.Type == null)
                        {
                            continue;
                        }

                        var name = declarationVariable.Identifier.Text;
                        invalidVariables.Add(name);
                        factory.Variables.Add(new MapFactoryVariable(name, expression, TypeInfo.From(type.Type)));
                    }
                    else
                    {
                        factory.Statements.Add(LocalDeclarationStatement(
                            declaration.AttributeLists,
                            declaration.Modifiers,
                            VariableDeclaration(declaration.Declaration.Type, new SeparatedSyntaxList<VariableDeclaratorSyntax>
                            {
                                declarationVariable
                            })
                        ));
                    }
                }
            }

            if (!valid)
            {
                return;
            }

            map.Factories.Add(factory);
        }
    }
}
