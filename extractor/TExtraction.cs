using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class TExtraction<TDeclarationSyntax> : ITypeExtraction where TDeclarationSyntax : TypeDeclarationSyntax
    {
        public string Name { get; private set; }

        public TExtraction(SyntaxTree tree, string name)
        {
            Name = name;
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            _tree = tree;
            _model = compilation.GetSemanticModel(tree);
            _root = (CompilationUnitSyntax)tree.GetRoot();
            _node = _root.DescendantNodes().OfType<TDeclarationSyntax>().Where(declaration => declaration.Identifier.ValueText == name).First();
        }

        public IEnumerable<ITypeExtraction> GetFieldsAndProperties()
        { 
            foreach (PropertyDeclarationSyntax propDecl in _node.DescendantNodes().OfType<PropertyDeclarationSyntax>()) {
                IPropertySymbol propSymbol = _model.GetDeclaredSymbol(propDecl);
                yield return TypeExtraction.CreateTypeExtraction(_tree, propSymbol.Type.Name);
            
                if (propDecl.Type is GenericNameSyntax typeSyntax) {
                    foreach (var tArg in typeSyntax.TypeArgumentList.Arguments) {
                        TypeInfo argSymbol = _model.GetTypeInfo(tArg);
                        yield return TypeExtraction.CreateTypeExtraction(_tree, argSymbol.Type.Name);
                    }
                }
            }

            foreach (FieldDeclarationSyntax fieldDecl in _node.DescendantNodes().OfType<FieldDeclarationSyntax>()) {
                string childName = null;
                TypeSyntax fieldType = fieldDecl.Declaration.Type;

                if (fieldType is GenericNameSyntax talSyntax) {
                    childName = talSyntax.Identifier.ValueText;
                    // generic type parameters
                    foreach (var tArg in talSyntax.TypeArgumentList.Arguments) {
                        TypeInfo argSymbol = _model.GetTypeInfo(tArg);
                        yield return TypeExtraction.CreateTypeExtraction(_tree, argSymbol.Type.Name);
                    }
                }
                else if (fieldType is IdentifierNameSyntax inSyntax) {
                    childName = inSyntax.Identifier.ValueText;
                }

                if (childName != null) {
                    yield return TypeExtraction.CreateTypeExtraction(_tree, childName);
                }
            }
        }

        public IEnumerable<MethodExtraction> GetMethods()
        {
            var methods = _node.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax methodDecl in methods) {
                yield return new MethodExtraction(_tree, methodDecl.Identifier.ValueText);
            }
        }

        public IEnumerable<ITypeExtraction> GetParents()
        {
            INamedTypeSymbol symbol = (INamedTypeSymbol)_model.GetDeclaredSymbol(_node);
            foreach (INamedTypeSymbol interfaceSymbol in symbol.Interfaces) {   
                yield return new InterfaceExtraction(_tree, interfaceSymbol.Name);
            }

            string parentName = symbol.BaseType.Name;
            if (parentName != "Object") {
                yield return new ClassExtraction(_tree, symbol.BaseType.Name);
            }
        }

        private SyntaxTree _tree;
        private SemanticModel _model;
        private SyntaxNode _root;
        protected TDeclarationSyntax _node;
    }
}