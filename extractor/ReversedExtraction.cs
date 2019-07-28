using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public static class ReversedExtraction
    {
        public static IEnumerable<ITypeExtraction> Reversed(DeclarationRegistry registry, string name, Func<ITypeExtraction, IEnumerable<ITypeExtraction>> getter)
        {
            IEnumerable<TypeDeclarationSyntax> declarations = registry.Types;
            foreach (TypeDeclarationSyntax declaration in declarations) {
                ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(registry, declaration.Identifier.ValueText);
                foreach (ITypeExtraction extr in getter(extraction)) {
                    if (extr.Name == name) {
                        yield return extraction;
                    }
                }
            }
        }
    }
}