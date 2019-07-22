using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class MethodExtraction
    {
        public MethodExtraction(SyntaxTree tree, string name)
        {
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            _tree = tree;
            _model = compilation.GetSemanticModel(tree);
            _root = (CompilationUnitSyntax)tree.GetRoot();
            _node = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(declaration => declaration.Identifier.ValueText == name).First();
        }

        public IEnumerable<ITypeExtraction> ReturnType() { yield break; }

        public IEnumerable<ITypeExtraction> ArgumentTypes() { yield break; }

        public IEnumerable<MethodExtraction> GetCallees() { yield break; }

        public IEnumerable<ITypeExtraction> GetLocalTypes() { yield break; }

        public IEnumerable<MethodExtraction> GetCalls() { yield break; }

        private SyntaxTree _tree;
        private object _model;
        private CompilationUnitSyntax _root;
        private MethodDeclarationSyntax _node;

    }
}