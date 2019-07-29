using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using relations;

namespace todot
{
    class Options
    {
        [Option('c', "contextcenter", HelpText = "the class to which the context is searched")]
        public string ContextCenter { get; set; }

        [Option('r', "relation", HelpText = "relation type to be considered")]
        public IEnumerable<string> RelationTypes { get; set; }

        [Option('i', "input", HelpText = "folder in which .cs files reside")]
        public IEnumerable<string> InputFolders { get; set; }

        [Option('o', "output", HelpText = "dot output file")]
        public string OutputFile { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            IEnumerable<string> inputFolders = null;
            IEnumerable<RelationType> relationTypes = null;
            string contextcenter = null;

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => {
                    inputFolders = opts.InputFolders;
                    relationTypes = opts.RelationTypes.Select(rt => {
                        Enum.TryParse(rt, true, out RelationType rtout);
                        return rtout;
                    });
                    contextcenter = opts.ContextCenter;
                });

            await DoWithOptions(contextcenter, inputFolders, relationTypes);
        }

        private static async Task DoWithOptions(
            string contextcenter,
            IEnumerable<string> inputFolders,
            IEnumerable<RelationType> relationTypes)
        {

            IEnumerable<string> files = inputFolders.SelectMany(folder
                => Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories));


            StringBuilder builder = new StringBuilder();
            foreach (string filename in files) {
                builder.Append(File.ReadAllText(filename));
            }
            string sourceCode = builder.ToString();


            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            var registry = new DeclarationRegistry(tree);

            var relationRegistry = new RelationRegistry(registry);

            List<Relation> overnextNeighbors = await GetOvernextNeighbors(relationRegistry, relationTypes.ToList(), contextcenter);

            IEnumerable<Node> nodes = overnextNeighbors
                .SelectMany(relation => new Node[] {
                    new Node { Name = relation.From.Name, Kind = MapKind(relation.From) },
                    new Node { Name = relation.To.Name, Kind = MapKind(relation.To) }
                }).Distinct();

            Console.WriteLine("digraph d {");

            List<string> dotLines = nodes.Select(NodeToDotLine).ToList();
            dotLines.AddRange(overnextNeighbors.Select(RelationToDotLine));

            foreach (string line in dotLines) {
                Console.WriteLine(line);
            }

            Console.WriteLine("}");
        }

        private static async Task<List<Relation>> GetOvernextNeighbors(
            RelationRegistry relationRegistry,
            List<RelationType> relationTypes,
            string typeName)
        {
            var nextNeighbors = GetNextNeighbors(relationRegistry, relationTypes, typeName);
         
            var tasks = new List<Task<List<Relation>>>();
            foreach (Relation neighbor in nextNeighbors) {
                tasks.Add(Task.Run(() => GetNextNeighbors(relationRegistry, relationTypes, neighbor.From.Name)));
                tasks.Add(Task.Run(() => GetNextNeighbors(relationRegistry, relationTypes, neighbor.To.Name)));
            }
            return (await Task.WhenAll(tasks)).SelectMany(t => t).Distinct().ToList();
        }

        private static NodeType MapKind(ITypeExtraction from)
        {
            switch (from)
            {
                case InterfaceExtraction _: return NodeType.Interface;
                case ClassExtraction _: return NodeType.Class;
                case StructExtraction _: return NodeType.Struct;
                default : throw new NotImplementedException("Extraction cannot be mapped to NodeType.");
            }
        }

        private static List<Relation> GetNextNeighbors(
            RelationRegistry registry,
            List<RelationType> relationTypes,
            string typeName)
        {   
            List<Relation> context = new List<Relation>();
            foreach (var relationType in Enum.GetValues(typeof(RelationType))) {
                var rt = (RelationType)relationType;
                if (relationTypes.Contains(rt)) {
                    context.AddRange(registry.GetRelations(typeName, rt).ToList());
                }
            }
            return context;
        }

        private static string NodeToDotLine(Node node)
        {
            switch (node.Kind)
            {
                case NodeType.Interface: return $"\"{node.Name}\" [shape=box style=filled fillcolor=\"#333333\" fontcolor=\"#cccccc\"]";
                case NodeType.Class: return $"\"{node.Name}\" [shape=box style=filled fillcolor=\"#ffff22\" fontcolor=\"#333333\"]";
                case NodeType.Struct: return $"\"{node.Name}\" [shape=box style=filled fillcolor=\"#cccc00\" fontcolor=\"#333333\"]";
                default: throw new NotImplementedException($"no style for NodeType {node.Kind}");
            }
        }

        private static string RelationToDotLine(Relation relation)
        {
            switch (relation.Kind)
            {
                case RelationType.Parents: return ToDotLine(relation, "[arrowhead=onormal]");
                case RelationType.Children: return ToDotLine(relation, "[dir=back arrowhead=onormal]");
                case RelationType.Referenced: return ToDotLine(relation, "[dir=back arrowhead=diamond]");
                case RelationType.Referencing: return ToDotLine(relation, "[arrowhead=diamond]");
                case RelationType.MethodReturns: return ToDotLine(relation, "[arrowhead=vee]");
                case RelationType.MethodReturned: return ToDotLine(relation, "[dir=back arrowhead=vee]");
                case RelationType.MethodArgs: return ToDotLine(relation, "[arrowhead=crow]");
                case RelationType.UsedAsArg: return ToDotLine(relation, "[dir=back arrowhead=crow]");
                case RelationType.Callers: return ToDotLine(relation, "[dir=back arrowhead=normal]");
                case RelationType.Callees: return ToDotLine(relation, "[arrowhead=normal]");
                default: throw new NotImplementedException($"No arrow symbol for RelationType {relation.Kind}");
            }
        }

        private static string ToDotLine(Relation relation, string properties)
        {
            return $"\"{relation.From.Name}\" -> \"{relation.To.Name}\" {properties}";
        }
    }
}
