using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Ukinee.Common.Identifiers.Attributes;

namespace Ukinee.Common.Identifiers.SourceGenerators.Identifiers
{
    [Generator]
    public class MultiGuidIdentityGenerator : IIncrementalGenerator
    {
        private const string AttributeName = nameof(IdentifierAttribute);

    #region Diagnostics
        private static readonly DiagnosticDescriptor NotPartialError = new DiagnosticDescriptor(
            id: "MGI001",
            title: "Struct must be partial",
            messageFormat: "The struct '{0}' must be marked as partial to allow code generation",
            category: "IdentifierAttributes",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor MissingOrderError = new DiagnosticDescriptor(
            id: "MGI002",
            title: "Property order not specified",
            messageFormat: "You must specify the order of all properties in the [IdentifierAttribute] attribute for '{0}'",
            category: "IdentifierAttributes",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor MismatchError = new DiagnosticDescriptor(
            id: "MGI003",
            title: "Property mismatch",
            messageFormat: "The properties specified in [IdentifierAttribute] do not match the supported properties found in '{0}'. Expected: {1}. Found in attribute: {2}.",
            category: "IdentifierAttributes",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        private static readonly DiagnosticDescriptor UnsupportedPropertyTypeError = new DiagnosticDescriptor(
            id: "MGI004",
            title: "Unsupported property type",
            messageFormat: "Property '{0}' in struct '{1}' has unsupported type '{2}'. Supported types: {3}.",
            category: "IdentifierAttributes",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    #endregion

        private static readonly ImmutableList<SupportedType> SupportedTypes = new[] {
            new SupportedType(
                typeName: "Guid",
                isMatch: t => t.Name == "Guid" && t.ContainingNamespace?.ToString() == "System",
                tryParseMethod: "Guid.TryParse",
                parseExpression: (valueVar, resultVar) => $"{resultVar} = {valueVar};"
            ),
            new SupportedType(
                typeName: "long",
                isMatch: t => t.Name == "Int64" && t.ContainingNamespace?.ToString() == "System",
                tryParseMethod: "long.TryParse",
                parseExpression: (valueVar, resultVar) => $"{resultVar} = {valueVar};"
            ),
            new SupportedType(
                typeName: "string",
                isMatch: t => t.Name == "String" && t.ContainingNamespace?.ToString() == "System",
                tryParseMethod: null,
                parseExpression: (valueVar, resultVar) => $"{resultVar} = new string({valueVar});"
            )
        }.ToImmutableList();

        private class SupportedType
        {
            public string TypeName { get; }
            public Func<ITypeSymbol, bool> IsMatch { get; }
            public string TryParseMethod { get; }
            public Func<string, string, string> ParseExpression { get; }

            public SupportedType(string typeName, Func<ITypeSymbol, bool> isMatch, string tryParseMethod, Func<string, string, string> parseExpression)
            {
                TypeName = typeName;
                IsMatch = isMatch;
                TryParseMethod = tryParseMethod;
                ParseExpression = parseExpression;
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var structInfos = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                    predicate:  (node, _) => GeneratorUtilities.IsStructSyntax(node),
                    transform:  (ctx, _) => GetStructInfo(ctx)
                )
                .Where( info => info != null);

            context.RegisterSourceOutput(structInfos,  GenerateCode);
        }

        private static StructInfo GetStructInfo(GeneratorSyntaxContext context)
        {
            var typeDecl = (TypeDeclarationSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;

            if (typeSymbol == null)
                return null;

            var attr = GeneratorUtilities.GetAttribute(typeSymbol, AttributeName);

            if (attr == null)
                return null;

            var orderedNames = GeneratorUtilities.GetOrderedNames(typeSymbol);

            if (!orderedNames.HasValue)
                return null;

            var location = GeneratorUtilities.GetIdentifierLocation(typeDecl);

            if (orderedNames.Value.IsEmpty)
                return StructInfo.CreateError(MissingOrderError, location, typeSymbol.Name);

            if (!GeneratorUtilities.IsPartial(typeDecl))
                return StructInfo.CreateError(NotPartialError, location, typeSymbol.Name);

            var existingProps = GeneratorUtilities
                .GetPublicInstanceProperties(typeSymbol)
                .Select(p => (Symbol: p, SupportedType: SupportedTypes.FirstOrDefault(st => st.IsMatch(p.Type))))
                .Where(p => p.SupportedType != null)
                .Select(p => (Name: p.Symbol.Name, Type: p.Symbol.Type, SupportedType: p.SupportedType))
                .ToImmutableList();

            var orderedProps = new List<(string Name, ITypeSymbol Type, SupportedType SupportedType)>();

            foreach (var name in orderedNames)
            {
                var prop = existingProps.FirstOrDefault(p => p.Name == name);

                if (prop.Name == null)
                {
                    return StructInfo.CreateError(
                        MismatchError,
                        location,
                        typeSymbol.Name,
                        string.Join(", ", existingProps.Select(p => p.Name)),
                        string.Join(", ", orderedNames)
                    );
                }

                orderedProps.Add(prop);
            }

            if (orderedProps.Count != existingProps.Count)
            {
                return StructInfo.CreateError(
                    MismatchError,
                    location,
                    typeSymbol.Name,
                    string.Join(", ", existingProps.Select(p => p.Name)),
                    string.Join(", ", orderedNames)
                );
            }

            return new StructInfo {
                Symbol = typeSymbol,
                OrderedProperties = orderedProps.ToImmutableList(),
                IsRecord = typeDecl.IsKind(SyntaxKind.RecordStructDeclaration),
                IsValid = true,
                Location = location
            };
        }

        private static void GenerateCode(SourceProductionContext context, StructInfo info)
        {
            if (!info.IsValid)
            {
                context.ReportDiagnostic(Diagnostic.Create(info.ErrorDescriptor, info.Location, info.ErrorArgs));

                return;
            }

            var typeName = info.Symbol.Name;
            var ns = info.Symbol.ContainingNamespace.IsGlobalNamespace ? null : info.Symbol.ContainingNamespace.ToString();
            var props = info.OrderedProperties;

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine();

            if (ns != null)
                sb.AppendLine($"namespace {ns}\n{{");

            var kind = info.IsRecord ? "record struct" : "struct";
            sb.AppendLine($"    public partial {kind} {typeName} : ISpanParsable<{typeName}>, Ukinee.Common.Identifiers.Attributes.IComplexIdentifier<{typeName}>");
            sb.AppendLine("    {");

            var createParams = string.Join(", ", props.Select(p => $"{p.SupportedType.TypeName} {p.Name}"));
            var createInit = string.Join(", ", props.Select(p => $"{p.Name} = {p.Name}"));
            sb.AppendLine($"        public static {typeName} Create({createParams})");
            sb.AppendLine("        {");
            sb.AppendLine($"            return new {typeName} {{ {createInit} }};");
            sb.AppendLine("        }");
            sb.AppendLine();

            var toStringInterpolation = string.Join("_", props.Select(p => $"{{{p.Name}}}"));
            sb.AppendLine($"        public override string ToString() => $\"{toStringInterpolation}\";");
            sb.AppendLine();

            GenerateTryParse(sb, typeName, props);

            sb.AppendLine(
                $@"
        public static {typeName} Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);
        public static {typeName} Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => 
            TryParse(s, provider, out var result) ? result : throw new FormatException($""Invalid {typeName}"");
        public static bool TryParse(string? s, IFormatProvider? provider, out {typeName} result) => 
            s != null ? TryParse(s.AsSpan(), provider, out result) : (result = default) == default && false;"
            );

            sb.AppendLine("    }");

            if (ns != null)
                sb.AppendLine("}");

            context.AddSource($"{typeName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private static void GenerateTryParse(StringBuilder sb, string typeName, ImmutableList<(string Name, ITypeSymbol Type, SupportedType SupportedType)> props)
        {
            sb.AppendLine($"        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out {typeName} result)");
            sb.AppendLine("        {");
            sb.AppendLine("            result = default;");
            sb.AppendLine("            ReadOnlySpan<char> remaining = s;");

            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                var type = prop.SupportedType;
                var valueVar = $"v{i}";
                sb.AppendLine($"            {type.TypeName} {valueVar};");

                if (i < props.Count - 1)
                {
                    sb.AppendLine($"            int idx{i} = remaining.IndexOf('_');");
                    sb.AppendLine($"            if (idx{i} == -1) return false;");
                    var segment = $"remaining[..idx{i}]";

                    if (type.TryParseMethod != null)
                    {
                        sb.AppendLine($"            if (!{type.TryParseMethod}({segment}, out {valueVar})) return false;");
                    }
                    else
                    {
                        sb.AppendLine($"            {type.ParseExpression(segment, valueVar)}");
                    }

                    sb.AppendLine($"            remaining = remaining[(idx{i} + 1)..];");
                }
                else
                {
                    if (type.TryParseMethod != null)
                    {
                        sb.AppendLine($"            if (!{type.TryParseMethod}(remaining, out {valueVar})) return false;");
                    }
                    else
                    {
                        sb.AppendLine($"            {type.ParseExpression("remaining", valueVar)}");
                    }
                }
            }

            var assignments = string.Join(", ", props.Select((p, i) => $"{p.Name} = v{i}"));
            sb.AppendLine($"            result = new {typeName} {{ {assignments} }};");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }

        private class StructInfo
        {
            public INamedTypeSymbol Symbol { get; set; }
            public ImmutableList<(string Name, ITypeSymbol Type, SupportedType SupportedType)> OrderedProperties { get; set; } = ImmutableList<(string, ITypeSymbol, SupportedType)>.Empty;
            public bool IsValid { get; set; }
            public bool IsRecord { get; set; }
            public Location Location { get; set; } = Location.None;
            public DiagnosticDescriptor ErrorDescriptor { get; set; }
            public string[] ErrorArgs { get; set; }

            public static StructInfo CreateError(DiagnosticDescriptor d, Location loc, params string[] args)
            {
                return new StructInfo {
                    ErrorDescriptor = d,
                    ErrorArgs = args,
                    Location = loc,
                    IsValid = false,
                };
            }
        }
    }
}
