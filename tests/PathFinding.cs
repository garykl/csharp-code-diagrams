using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace tests
{
    public class PathFinding
    {
        public class Relation { }
        public class Inheritance : Relation { }

        private static IEnumerable<MethodInfo> GetNamesOfMethodsThatReturn<TContaining, TReturn>()
          => typeof(TContaining).GetMethods()
                .Where(method => method.ReturnType == typeof(IEnumerable<TReturn>));

        private static IEnumerable<(string, TReturnExtraction)> CallAll<TInputExtraction, TReturnExtraction>(TInputExtraction extraction)
        {
            IEnumerable<MethodInfo> methods = GetNamesOfMethodsThatReturn<TInputExtraction, TReturnExtraction>();
            foreach (MethodInfo method in methods) {
                foreach (TReturnExtraction extr in (IEnumerable<TReturnExtraction>)method.Invoke(extraction, new object[] { })) {
                    yield return (method.Name, extr);
                }
            }
        }

        public static string FindPath(ITypeExtraction type1, ITypeExtraction type2)
        {
            string directed1 = FindDirectedPath(type1, type2);
            if (directed1 == null) {
                return FindDirectedPath(type2, type1);
            }
            return directed1;
        }

        private static string FindDirectedPath(ITypeExtraction type1, ITypeExtraction type2)
        {
            IEnumerable<(string, ITypeExtraction)> typeExtractions = CallAll<ITypeExtraction, ITypeExtraction>(type1);
            // IEnumerable<(string, ITypeExtraction)> typeExtractions2 = CallAll<ITypeExtraction, ITypeExtraction>(type2);
            // IEnumerable<ITypeExtraction> intersection = typeExtractions1.Where(extr1 => typeExtractions2.Select(extr2 => extr2.Name).Contains(extr1.Name));
            return typeExtractions.Where(extr => extr.Item2.Name == type2.Name).Select(extr => extr.Item1).FirstOrDefault();
        }

        public static string FindPath(ITypeExtraction type, MethodExtraction method) => null;
        public static string FindPath(MethodExtraction method1, MethodExtraction method2) => null;
    }
    
    public class PathFindingTests
    {
        [Fact]
        public void FindPathBetweenClasses()
        {
            string code = @"
                class A { }
                class B : A { }
            ";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            string relation = PathFinding.FindPath(new ClassExtraction(tree, "A"), new ClassExtraction(tree, "B"));
            Assert.Equal("GetParents", relation);
        }       
    }
}