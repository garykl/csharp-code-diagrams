using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class InterfaceExtraction : TExtraction<InterfaceDeclarationSyntax>
    {
        public InterfaceExtraction(SyntaxTree tree, string name) : base(tree, name) { }
    }
}