using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class StructExtraction : TExtraction<StructDeclarationSyntax>
    {
        public StructExtraction(DeclarationRegistry registry, string name) : base(registry, name) { }
    }
}