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
            StringBuilder builder = new StringBuilder();
            foreach (string filename in args) {
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

            List<INode> nodes = extractor.GetClasses(root).ToList();
            nodes.AddRange(extractor.GetStructs(root));
            nodes.AddRange(extractor.GetInterfaces(root));

            Console.WriteLine("digraph D {");

            foreach (INode node in nodes) {
                if (node is Class clss) {
                    Console.WriteLine($"\"{clss.Name}\" [style=filled color=\"#ffff77\"]");
                } else if (node is Interface ntrfc) {
                    Console.WriteLine($"\"{ntrfc.Name}\" [style=filled color=\"#aaaaaa\"]");
                } else if (node is Struct strct) {
                    Console.WriteLine($"\"{strct.Name}\" [style=filled color=\"#ddddaa\"]");
                }
            }

            List<string> nodeNames = nodes.Select(node => node.Name).ToList();
            foreach (IRelationship relation in relations) {

                if ((!nodeNames.Contains(relation.Parent)) || (!nodeNames.Contains(relation.Child))) { continue; }

                if (relation is ImplementationRelationship implementation) {
                    Console.WriteLine($"\"{relation.Child}\" -> \"{relation.Parent}\" [arrowhead=onormal]");
                } else if (relation is InheritanceRelationship inheritance) {
                    Console.WriteLine($"\"{relation.Child}\" -> \"{relation.Parent}\" [arrowhead=onormal]");
                } else if (relation is AssociationRelationship association) {
                    Console.WriteLine($"\"{relation.Parent}\" -> \"{relation.Child}\"");
                }
            }


            Console.WriteLine("}");
        }
    }
}
