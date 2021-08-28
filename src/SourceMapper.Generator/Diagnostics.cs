using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceMapper.Generator
{
    public static class Diagnostics
    {
        public static DiagnosticDescriptor InvalidFirstParameter = new(
            "SM0001",
            "Invalid factory parameter",
            "First parameter type of method '{1}.{1}' should be the source type '{2}'",
            "SourceMapper",
            DiagnosticSeverity.Error,
            true);
        
        public static DiagnosticDescriptor NoReturn = new(
            "SM0002",
            "No return in factory",
            "Missing return statement in method '{1}.{0}'",
            "SourceMapper",
            DiagnosticSeverity.Error,
            true);
        
        public static DiagnosticDescriptor UnsupportedStatement = new(
            "SM0003",
            "Invalid statement",
            "Invalid statement in method '{1}.{0}': only variables can access the source parameter '{3}'",
            "SourceMapper",
            DiagnosticSeverity.Error,
            true);

        public static DiagnosticDescriptor InvalidVariableReference = new(
            "SM0004",
            "Invalid variable",
            "Cannot access variable '{2}' in method '{1}.{0}': the variable uses the source parameter '{3}', which can only be accessed once",
            "SourceMapper",
            DiagnosticSeverity.Error,
            true);
    }
}
