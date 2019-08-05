using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace calltree
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFolders = new string[] { "../extractor" };
            IEnumerable<string> files = inputFolders.SelectMany(folder
                => Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories));


            StringBuilder builder = new StringBuilder();
            foreach (string filename in files) {
                builder.Append(File.ReadAllText(filename));
            }
            string sourceCode = builder.ToString();


            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            var registry = new DeclarationRegistry(tree);

            var method = new MethodExtraction(registry, "GetReferenced");

            Console.WriteLine("digraph d {");
            foreach (var mthd in method.GetCalls()) {
                Console.WriteLine($"{method.Name} -> {mthd.Name}");
            }
            foreach (var mthd in method.GetCallees()) {
                Console.WriteLine($"{mthd.Name} -> {method.Name}");
            }
            Console.WriteLine("}");

        }
    }
}
