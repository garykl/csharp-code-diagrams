using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class ClassExtraction : TExtraction<ClassDeclarationSyntax>
    {
        public ClassExtraction(DeclarationRegistry registry, string name)  : base(registry, name) { }
    }
}