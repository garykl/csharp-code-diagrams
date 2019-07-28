using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public static class TypeExtraction
    {
        public static ITypeExtraction CreateTypeExtraction(DeclarationRegistry registry, string name)
        {
            TypeDeclarationSyntax node = registry.GetIType(name);

            switch (node)
            {
                case ClassDeclarationSyntax _: return new ClassExtraction(registry, name);
                case StructDeclarationSyntax _: return new StructExtraction(registry, name);
                case InterfaceDeclarationSyntax _: return new InterfaceExtraction(registry, name);
                default: return null;
            }
        }

        private static TypeDeclarationSyntax GetSyntax(CompilationUnitSyntax root, string name)
            => root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(declaration => declaration.Identifier.ValueText == name)
                .FirstOrDefault();

    }
}