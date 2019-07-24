using extractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace tests
{
    public class PathFinding
    {
        public static void FindPath(ITypeExtraction type1, ITypeExtraction type2) { }
        public static void FindPath(ITypeExtraction type, MethodExtraction method) { }
        public static void FindPath(MethodExtraction method1, MethodExtraction method2) { }
        
        [Fact]
        public void FindPathBetweenClasses()
        {
            string code = @"
                class A { }
                class B : A { }
            ";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            FindPath
        }       
    }
}