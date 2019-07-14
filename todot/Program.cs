using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prototypes.RoslynAnalyzer;

namespace todot
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<string> folderContent = Directory.GetFiles("../../../../arena/Logic").Where(file => file.EndsWith(".cs"));
            StringBuilder builder = new StringBuilder();
            foreach (string filename in folderContent) {
                builder.Append(File.ReadAllText(filename));
            }
            string sourceCode = builder.ToString();

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            var model = compilation.GetSemanticModel(tree);

            var extractor = new Extractor(sourceCode, model);
            var root = (CompilationUnitSyntax)tree.GetRoot();


            List<IRelationship> relations = extractor.GetAssociations(root).ToList();
            relations.AddRange(extractor.GetInheritances(root));

            List<INode> nodes = CreateNodes(relations).ToList();


            Console.WriteLine("digraph D {");

            foreach (INode node in nodes) {
                if (node is Class clss) {
                    Console.WriteLine($"{clss.Name} [style=filled color=\"#333333\"]");
                } else if (node is Interface ntrfc) {
                    Console.WriteLine($"{ntrfc.Name} [style=filled color=\"#aaaaaa\"]");
                }
            }

            foreach (IRelationship relation in relations) {
                if (relation is ImplementationRelationship implementation) {
                    Console.WriteLine($"{relation.Parent} -> {relation.Child} [arrowType=empty]");
                } else if (relation is InheritanceRelationship inheritance) {
                    Console.WriteLine($"{relation.Parent} -> {relation.Child} [arrowType=empty]");
                } else if (relation is AssociationRelationship association) {
                    Console.WriteLine($"{relation.Parent} -> {relation.Child}");
                }
            }


            Console.WriteLine("}");
        }

        interface INode
        {
            string Name { get; }
        }

        class Node : INode
        {
            public string Name { get; set; }
            public bool Equals(INode node) => node.Name == Name;
        }

        class Interface : Node
        {
            public Interface(string name) => Name = name;
        }

        class Class : Node
        {
            public Class(string name) => Name = name;
        }

        private static IEnumerable<INode> CreateNodes(List<IRelationship> relations) => CreateAllNodes(relations).Distinct();

        private static IEnumerable<INode> CreateAllNodes(List<IRelationship> relations)
        {
            foreach (IRelationship relation in relations) {
                if (relation is InheritanceRelationship inheritance) {
                    yield return new Class(inheritance.Child);
                    yield return new Class(inheritance.Parent);
                } else if (relation is ImplementationRelationship implementation) {
                    yield return new Class(implementation.Child);
                    yield return new Interface(implementation.Parent);
                } else if (relation is AssociationRelationship association) { }
            }
        }
    }
}
