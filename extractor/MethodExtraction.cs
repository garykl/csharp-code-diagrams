using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class MethodExtraction : INamed
    {
        public string Name { get; private set; }

        public MethodExtraction(SyntaxTree tree, string name)
        {
            Name = name;
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            _tree = tree;
            _model = compilation.GetSemanticModel(tree);
            _root = (CompilationUnitSyntax)tree.GetRoot();
            _node = _root.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(declaration => declaration.Identifier.ValueText == name).First();
        }

        public IEnumerable<ITypeExtraction> GetReturnTypes()
        {
            ITypeSymbol symbol = _model.GetDeclaredSymbol(_node).ReturnType;
            return GetSymbolExtractions(symbol);
        }

        private IEnumerable<ITypeExtraction> GetSymbolExtractions(ITypeSymbol symbol)
        {
            ITypeExtraction extraction = GetSymbolExtraction(symbol);
            if (extraction != null) {
                yield return extraction;
            }

            if (symbol is INamedTypeSymbol namedSymbol) {
                foreach (ITypeSymbol tSymbol in namedSymbol.TypeArguments) {
                    ITypeExtraction tExtraction = GetSymbolExtraction(tSymbol);
                    if (tExtraction != null) {
                        yield return tExtraction;
                    }
                }
            }
        }

        private ITypeExtraction GetSymbolExtraction(ITypeSymbol symbol)
        {
            if (symbol.IsValueType && Has<StructDeclarationSyntax>(symbol.Name)) {
                return new StructExtraction(_tree, symbol.Name);
            } else if (symbol.IsReferenceType && Has<ClassDeclarationSyntax>(symbol.Name)) {
                return new ClassExtraction(_tree, symbol.Name);
            } else {
                return null;
            }

        }

        private bool Has<TSyntax>(string name) where TSyntax : TypeDeclarationSyntax
            => _root.DescendantNodes().OfType<TSyntax>().Where(strct => strct.Identifier.ValueText == name).Count() > 0;

        public IEnumerable<ITypeExtraction> GetArgumentTypes()
        {
            IEnumerable<IParameterSymbol> parameters = _model.GetDeclaredSymbol(_node).Parameters;
            foreach (IParameterSymbol parameter in parameters) {
                foreach (ITypeExtraction extraction in GetSymbolExtractions(parameter.Type)) {
                    yield return extraction;
                }
            }
        }

        public IEnumerable<MethodExtraction> GetCallees()
        {
            var methods = _root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods) {
                foreach (var inv in method.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
                    if (_model.GetSymbolInfo(inv).Symbol.Name == Name) {
                        yield return new MethodExtraction(_tree, method.Identifier.ValueText);
                    }
                }
            }
        }

        public IEnumerable<ITypeExtraction> GetLocalTypes()
        {
            // complicated:
            // - local variables
            // - invocations return and parameters
            yield break;
        }

        public IEnumerable<MethodExtraction> GetCalls()
        {
            var invocations = _node.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (InvocationExpressionSyntax invDecl in invocations) {
                yield return new MethodExtraction(_tree, _model.GetSymbolInfo(invDecl).Symbol.Name);
            }
        }

        private SyntaxTree _tree;
        private SemanticModel _model;
        private CompilationUnitSyntax _root;
        private MethodDeclarationSyntax _node;

    }
}