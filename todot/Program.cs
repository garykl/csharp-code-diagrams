using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using relations;

namespace todot
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<string> files = Directory.GetFiles("../../arena/Logic").Where(filename => filename.EndsWith(".cs"));
            foreach (string filename in files) {
                builder.Append(File.ReadAllText(filename));
            }
            string sourceCode = builder.ToString();

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);

            var relationRegistry = new RelationRegistry(tree);

            string typeName = "CircularMoveEventSource";
            List<Relation> nextNeighbors = GetNeighbors(relationRegistry, typeName);
            IEnumerable<Relation> overnextNeighbors = nextNeighbors
                .SelectMany(relation => new string[] { relation.From.Name, relation.To.Name })
                .Distinct()
                .SelectMany(name => GetNeighbors(relationRegistry, name));

            Console.WriteLine("digraph d {");
            IEnumerable<string> dotLines = overnextNeighbors.Distinct().Select(relation => RelationToDotLine(relation));
            foreach (string line in dotLines) {
                Console.WriteLine(line);
            }
            Console.WriteLine("}");
        }

        private static List<Relation> GetNeighbors(RelationRegistry registry, string typeName)
        {
            List<Relation> context = new List<Relation>();
            foreach (var relationType in Enum.GetValues(typeof(RelationType))) {
                context.AddRange(registry.GetRelations(typeName, (RelationType)relationType).ToList());
            }
            return context;
        }

        private static string RelationToDotLine(Relation unbased)
        {   Relation relation = unbased.MapToBase();
            switch (relation.Kind)
            {
                case RelationType.Parents: return ToDotLine(relation, "[arrowhead=onormal]");
                case RelationType.Children: return ToDotLine(relation, "[dir=back arrowhead=onormal]");
                case RelationType.Referenced: return ToDotLine(relation, "[dir=back arrowhead=diamond]");
                case RelationType.Referencing: return ToDotLine(relation, "[arrowhead=diamond]");
                case RelationType.MethodReturns: return ToDotLine(relation, "[arrowhead=curve]");
                case RelationType.MethodArgs: return ToDotLine(relation, "[arrowhead=icurve]");
                case RelationType.Callers: return ToDotLine(relation, "[dir=back arrowhead=vee]");
                case RelationType.Callees: return ToDotLine(relation, "[arrowhead=vee]");
                default: throw new NotImplementedException($"No arrow symbol for RelationType {relation.Kind}");
            }
        }

        private static string ToDotLine(Relation relation, string properties)
            => $"\"{relation.From.Name}\" -> \"{relation.To.Name}\" {properties}";
    }
}
