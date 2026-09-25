using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ukinee.Common.Identifiers.Attributes;

namespace Ukinee.Common.Identifiers.SourceGenerators.Identifiers
{
    internal static class GeneratorUtilities
    {
        public static ImmutableArray<string>? GetOrderedNames(ISymbol symbol)
        {
            var attr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == nameof(IdentifierAttribute));

            if (attr == null)
                return null;

            if (attr.ConstructorArguments.IsEmpty)
                return ImmutableArray<string>.Empty;

            var orderArg = attr.ConstructorArguments[0];
            var values = orderArg.Values;

            if (values.IsDefault)
                return ImmutableArray<string>.Empty;

            return values
                .Select(v => v.Value as string)
                .Where(s => s != null)
                .ToImmutableArray();
        }

        public static bool IsStructSyntax(SyntaxNode node) =>
            node is TypeDeclarationSyntax typeDecl &&
            (typeDecl.IsKind(SyntaxKind.StructDeclaration) || typeDecl.IsKind(SyntaxKind.RecordStructDeclaration)) &&
            typeDecl.AttributeLists.Count > 0;

        public static bool IsClassOrRecordSyntax(SyntaxNode node) =>
            node is TypeDeclarationSyntax typeDecl &&
            (typeDecl.IsKind(SyntaxKind.ClassDeclaration) || typeDecl.IsKind(SyntaxKind.RecordDeclaration)) &&
            typeDecl.AttributeLists.Count > 0;

        public static bool IsPartial(TypeDeclarationSyntax typeDecl) =>
            typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        public static AttributeData GetAttribute(INamedTypeSymbol typeSymbol, string attributeName) =>
            typeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == attributeName);

        public static ImmutableArray<IPropertySymbol> GetPublicInstanceProperties(INamedTypeSymbol typeSymbol) =>
            typeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public)
                .ToImmutableArray();

        public static string GetTypeKindKeyword(TypeDeclarationSyntax typeDecl) =>
            typeDecl.IsKind(SyntaxKind.RecordStructDeclaration) ? "record struct" :
            typeDecl.IsKind(SyntaxKind.StructDeclaration) ? "struct" :
            typeDecl.IsKind(SyntaxKind.RecordDeclaration) ? "record" :
            "class";

        public static Location GetIdentifierLocation(TypeDeclarationSyntax typeDecl) =>
            typeDecl.Identifier.GetLocation();
    }
}
