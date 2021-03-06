using System;
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

        public TExtraction(DeclarationRegistry registry, string name)
        {
            Name = name;
            _model = registry.Model;
            _registry = registry;
            _node = (TDeclarationSyntax)_registry.GetIType(name);
        }

        public IEnumerable<ITypeExtraction> GetReferenced()
        { 
            foreach (PropertyDeclarationSyntax propDecl in _node.DescendantNodes().OfType<PropertyDeclarationSyntax>()) {
                IPropertySymbol propSymbol = _model.GetDeclaredSymbol(propDecl);
                if (propSymbol != null) {
                    ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(_registry, propSymbol.Type.Name);
                    if (extraction != null) {
                        yield return extraction;
                    };
                }
            
                if (propDecl.Type is GenericNameSyntax typeSyntax) {
                    foreach (var tArg in typeSyntax.TypeArgumentList.Arguments) {
                        TypeInfo argSymbol = _model.GetTypeInfo(tArg);
                        ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(_registry, argSymbol.Type.Name);
                        if (extraction != null) {
                            yield return extraction;
                        }
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
                        ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(_registry, argSymbol.Type.Name);
                        if (extraction != null) {
                            yield return extraction;
                        }
                    }
                }
                else if (fieldType is IdentifierNameSyntax inSyntax) {
                    childName = inSyntax.Identifier.ValueText;
                }

                if (childName != null) {
                    ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(_registry, childName);
                    if (extraction != null) {
                        yield return extraction;
                    }
                }
            }
        }

        public IEnumerable<MethodExtraction> GetMethods()
        {
            var methods = _node.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax methodDecl in methods) {
                yield return new MethodExtraction(_registry, methodDecl.Identifier.ValueText);
            }
        }

        public IEnumerable<ITypeExtraction> GetParents()
        {
            INamedTypeSymbol symbol = (INamedTypeSymbol)_model.GetDeclaredSymbol(_node);
            foreach (INamedTypeSymbol interfaceSymbol in symbol.Interfaces) {   
                var extraction = TypeExtraction.CreateTypeExtraction(_registry, interfaceSymbol.Name);
                if (extraction != null) {
                    yield return extraction;
                }
            }

            string parentName = symbol.BaseType?.Name;
            if (parentName != null && parentName != "Object") {
                var extraction = TypeExtraction.CreateTypeExtraction(_registry, symbol.BaseType.Name);
                if (extraction != null) {
                    yield return extraction;
                }
            }
        }

        public IEnumerable<ITypeExtraction> GetChildren()
        {
            yield break;
            // foreach (TypeDeclarationSyntax syntax in _root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
            //     var symbol = (INamedTypeSymbol) _model.GetDeclaredSymbol(syntax);
            //     string name = symbol?.BaseType?.Name;
            //     IEnumerable<string> interfaces = symbol.Interfaces.Select(interf => interf.Name);
            //     if (name != null && (name == Name || interfaces.Contains(Name))) {
            //         yield return TypeExtraction.CreateTypeExtraction(_registry, syntax.Identifier.ValueText);
            //     }
            // }
        }

        public IEnumerable<ITypeExtraction> GetReferencing()
        {
            yield break;
            // var declarations = _root.DescendantNodes().OfType<TypeDeclarationSyntax>();
            // foreach (TypeDeclarationSyntax declaration in declarations) {
            //     ITypeExtraction extraction = TypeExtraction.CreateTypeExtraction(_registry, declaration.Identifier.ValueText);
            //     foreach (ITypeExtraction extr in extraction.GetReferenced()) {
            //         if (extr.Name == Name) {
            //             yield return extraction;
            //         }
            //     }
            // }
        }


        private SemanticModel _model;
        private DeclarationRegistry _registry;
        protected TDeclarationSyntax _node;
    }
}