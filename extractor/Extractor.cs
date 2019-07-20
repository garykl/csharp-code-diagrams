using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Prototypes.RoslynAnalyzer
{
    
    public interface INode
    {
        string Name { get; }
    }

    public class Node : INode
    {
        public string Name { get; set; }
        public bool Equals(INode node) => node.Name == Name;
    }

    public class Interface : Node
    {
        public Interface(string name) => Name = name;
    }

    public class Class : Node
    {
        public Class(string name) => Name = name;
    }

    public class Struct : Node
    {
        public Struct(string name) => Name = name;
    }

    public class UnknownNode : Node
    {
        public UnknownNode(string name) => Name = name;
    }

    public interface IRelationship
    {
        string Parent { get; set; }
        string Child { get; set; }
    }

    public abstract class Relationship : IRelationship
    {
        public string Parent { get; set; }
        public string Child { get; set; }

        public bool Equals(Relationship relationship)
          => Parent.Equals(relationship.Parent) && Child.Equals(relationship.Child);
    }

    public class InheritanceRelationship : Relationship
    {}

    public class ImplementationRelationship : Relationship
    {}

    public class AssociationRelationship : Relationship
    {}

    public class Extractor
    {
        public Extractor(string sourceCode, SemanticModel model)
        {
            _sourceCode = sourceCode;
            _model = model;
        }

        public IEnumerable<INode> GetClasses(SyntaxNode node)
          => GetAll<ClassDeclarationSyntax>(node)
                .Select(declaration => new Class(declaration.Identifier.ValueText));

        public IEnumerable<INode> GetStructs(SyntaxNode node)
          => GetAll<StructDeclarationSyntax>(node)
                .Select(declaration => new Struct(declaration.Identifier.ValueText));

        public IEnumerable<INode> GetInterfaces(SyntaxNode node)
          => GetAll<InterfaceDeclarationSyntax>(node)
                .Select(declaration => new Interface(declaration.Identifier.ValueText));

        public IEnumerable<TDescendent> GetAll<TDescendent>(SyntaxNode node)
          => node.DescendantNodes().OfType<TDescendent>();

        public IEnumerable<IRelationship> GetAssociations(SyntaxNode node) => GetAllAssociations(node).Distinct();

        private IEnumerable<IRelationship> GetAllAssociations(SyntaxNode node)
        {
            var classes = node.DescendantNodes().OfType<TypeDeclarationSyntax>();

            foreach (TypeDeclarationSyntax declaration in classes) {

                string parentName = declaration.Identifier.ValueText;

                foreach (PropertyDeclarationSyntax propDecl in declaration.DescendantNodes().OfType<PropertyDeclarationSyntax>()) {
                    IPropertySymbol propSymbol = _model.GetDeclaredSymbol(propDecl);
                    yield return (IRelationship) new AssociationRelationship {
                        Child = propSymbol.Type.Name,
                        Parent = parentName
                    };

                    if (propDecl.Type is GenericNameSyntax typeSyntax) {
                        foreach (var tArg in typeSyntax.TypeArgumentList.Arguments) {
                            TypeInfo argSymbol = _model.GetTypeInfo(tArg);
                            yield return (IRelationship) new AssociationRelationship {
                                Child = argSymbol.Type.Name,
                                Parent = parentName
                            };
                        }
                    }
                }

                foreach (FieldDeclarationSyntax fieldDecl in declaration.DescendantNodes().OfType<FieldDeclarationSyntax>()) {
                    string childName = null;
                    TypeSyntax fieldType = fieldDecl.Declaration.Type;

                    if (fieldType is GenericNameSyntax talSyntax) {
                        childName = talSyntax.Identifier.ValueText;
                        // generic type parameters
                        foreach (var tArg in talSyntax.TypeArgumentList.Arguments) {
                            TypeInfo argSymbol = _model.GetTypeInfo(tArg);
                            yield return (IRelationship) new AssociationRelationship {
                                Child = argSymbol.Type.Name,
                                Parent = parentName
                            };
                        }

                    }
                    else if (fieldType is IdentifierNameSyntax inSyntax) {
                        childName = inSyntax.Identifier.ValueText;
                    }

                    if (childName != null) {
                        yield return (IRelationship) new AssociationRelationship {
                            Child = childName,
                            Parent = parentName
                        };
                    }
                }
            }
        }

        public IEnumerable<IRelationship> GetInheritances(SyntaxNode node)
        {
            var classes = node.DescendantNodes().OfType<TypeDeclarationSyntax>();

            foreach (TypeDeclarationSyntax declaration in classes) {
                INamedTypeSymbol symbol = _model.GetDeclaredSymbol(declaration);

                foreach (INamedTypeSymbol interfaceSymbol in symbol.Interfaces) {
                    yield return (IRelationship) new ImplementationRelationship {
                        Child = declaration.Identifier.ValueText,
                        Parent = interfaceSymbol.Name
                    };
                }

                if (declaration is ClassDeclarationSyntax classDeclaration) {
                    string parentName = symbol.BaseType.Name;
                    if (parentName != "Object") {
                        yield return (IRelationship) new InheritanceRelationship {
                            Child = declaration.Identifier.ValueText,
                            Parent = parentName
                        };
                    }
                }
            }
        }


        private string _sourceCode;
        private SemanticModel _model;
    }
}