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

        public MethodExtraction(DeclarationRegistry registry, string name)
        {   
            Name = name;
            _model = registry.Model;
            _registry = registry;
            _node = _registry.GetMethod(name);
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
            => TypeExtraction.CreateTypeExtraction(_registry, symbol.Name);


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
            foreach (var method in _registry.Methods) {
                var extraction = new MethodExtraction(_registry, method.Identifier.ValueText);
                if (extraction.GetCalls().Where(call => call.Name == Name).Count() > 0) {
                    yield return extraction;
                }
            }
        }

        public IEnumerable<MethodExtraction> GetCalls()
        {
            var invocations = _node.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (InvocationExpressionSyntax invDecl in invocations) {
                ISymbol symbol = _model.GetSymbolInfo(invDecl).Symbol;
                if (symbol != null) {
                    yield return new MethodExtraction(_registry, symbol.Name);
                }
            }
        }

        public IEnumerable<ITypeExtraction> GetCalleeTypes()
            => GetCalls().Select(method => method?.GetContainingType()).Where(type => type != null);

        public IEnumerable<ITypeExtraction> GetCallerTypes()
            => GetCallees().Select(method => method?.GetContainingType()).Where(type => type != null);

        private ITypeExtraction GetContainingType()
        {
            SymbolInfo symbol = _model.GetSymbolInfo(_node);
            if (symbol.Symbol?.ContainingType?.ContainingType != null) {
                return TypeExtraction.CreateTypeExtraction(_registry, symbol.Symbol.ContainingType.Name);
            }
            return null;
        }

        private SemanticModel _model;
        private DeclarationRegistry _registry;
        private MethodDeclarationSyntax _node;

    }
}