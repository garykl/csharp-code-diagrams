using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using relations;

namespace todot
{
    public enum NodeType {
        Interface,
        Class,
        Struct
    }
    
    struct Node
    {
        public string Name { get; set; }
        public NodeType Kind { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Node node &&
                   Name == node.Name &&
                   Kind == node.Kind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Kind);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<string> files = Directory.EnumerateFiles(
                "D:\\dev\\visweb_trajectory2\\development\\backend\\Storer\\TrajectoryPipe",
                "*.cs",
                SearchOption.AllDirectories);
            foreach (string filename in files) {
                builder.Append(File.ReadAllText(filename));
            }
            string sourceCode = builder.ToString();

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            var registry = new DeclarationRegistry(tree);

            var relationRegistry = new RelationRegistry(registry);

            string typeName = "IEventListener";
            List<Relation> overnextNeighbors = await GetOvernextNeighbors(relationRegistry, typeName);

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

        private static async Task<List<Relation>> GetOvernextNeighbors(RelationRegistry relationRegistry, string typeName)
        {
            var nextNeighbors = GetNextNeighbors(relationRegistry, typeName);
         
            var tasks = new List<Task<List<Relation>>>();
            foreach (Relation neighbor in nextNeighbors) {
                tasks.Add(Task.Run(() => GetNextNeighbors(relationRegistry, neighbor.From.Name)));
                tasks.Add(Task.Run(() => GetNextNeighbors(relationRegistry, neighbor.To.Name)));
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

        private static List<Relation> GetNextNeighbors(RelationRegistry registry, string typeName)
        {
            List<Relation> context = new List<Relation>();
            foreach (var relationType in Enum.GetValues(typeof(RelationType))) {
                context.AddRange(registry.GetRelations(typeName, (RelationType)relationType).ToList());
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
