using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using Prototypes.RoslynAnalyzer;

namespace roslynqueries
{
    public class UnitTest1
    {
        [Fact]
        public void InheritanceRelationship()
        {
            string text = @"
            interface I { }
            interface J { }
            class A { }
            class B : A { }
            class C : B, I, J";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(text);
            var compilation = CSharpCompilation.Create("not an assemly").AddSyntaxTrees(new SyntaxTree[] { tree });
            var model = compilation.GetSemanticModel(tree);

            var extractor = new Extractor(text, model);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var inheritances = extractor.GetInheritances(root).ToList();

            Assert.Equal(4, inheritances.Count());
            var cParents = inheritances.Where(inh => inh.Child == "C").Select(inh => inh.Parent).ToList();
            Assert.Equal(3, cParents.Count());
            Assert.Contains("B", cParents);
            Assert.Contains("I", cParents);
            Assert.Contains("J", cParents);
        }

        [Fact]
        public void AssociationsToMembersAndTypeParameters()
        {
            string text = @"
            class C1 { }
            class C2 { }
            class T { }
            class C3<TType> { }
            class A {
                public C1 c1;
                public C2 c2 { get; set; }
                public C3<T> deep;
                public C3<T> deepProp { get; set; }
            }";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(text);
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            var model = compilation.GetSemanticModel(tree);

            var extractor = new Extractor(text, model);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            IEnumerable<AssociationRelationship> relations = extractor.GetAssociations(root);
            
            Assert.Equal(4, relations.Count());
            List<string> ends = relations.Select(rel => rel.Child).ToList();
            Assert.Contains("C1", ends);
            Assert.Contains("C2", ends);
            Assert.Contains("C3", ends);
            Assert.Contains("T", ends);
        }

    }


    
}
