using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public static class TypeExtraction
    {
        public static ITypeExtraction CreateTypeExtraction(SyntaxTree tree, string name)
        {
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            SemanticModel model = compilation.GetSemanticModel(tree);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            TypeDeclarationSyntax node = GetSyntax(root, name);

            switch (node)
            {
                case ClassDeclarationSyntax _: return new ClassExtraction(tree, name);
                case StructDeclarationSyntax _: return new StructExtraction(tree, name);
                case InterfaceDeclarationSyntax _: return new InterfaceExtraction(tree, name);
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