using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class InterfaceExtraction : TExtraction<InterfaceDeclarationSyntax>
    {
        public InterfaceExtraction(DeclarationRegistry registry, string name) : base(registry, name) { }
    }
}