using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class ClassExtraction : TExtraction<ClassDeclarationSyntax>
    {
        public ClassExtraction(SyntaxTree tree, string name)  : base(tree, name) { }
    }
}