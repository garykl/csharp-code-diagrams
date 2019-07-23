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


        private string _sourceCode;
        private SemanticModel _model;
    }
}