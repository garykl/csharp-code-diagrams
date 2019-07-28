using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Collections.Generic;
using extractor;

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
            var registry = new DeclarationRegistry(tree);
            ITypeExtraction c = new ClassExtraction(registry, "C");

            List<ITypeExtraction> csParents = c.GetParents().ToList();
            Assert.Equal(3, csParents.Count());
            ITypeExtraction b = csParents.Where(parent => parent.Name == "B").First();
            Assert.True(b is ClassExtraction);
            Assert.True(csParents.Where(parent => parent.Name == "I").First() is InterfaceExtraction);
            Assert.True(csParents.Where(parent => parent.Name == "J").First() is InterfaceExtraction);
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
            var registry = new DeclarationRegistry(tree);

            ITypeExtraction a = new ClassExtraction(registry, "A");
            List<ITypeExtraction> fields = a.GetReferenced().ToList();

            ITypeExtraction c1 = fields.Find(field => field.Name == "C1");
            ITypeExtraction c2 = fields.Find(field => field.Name == "C2");

            Assert.True(c1 is ClassExtraction);
            Assert.True(c2 is ClassExtraction);
            Assert.Equal(2, fields.Where(field => field.Name == "C3").Count());
            Assert.Equal(2, fields.Where(field => field.Name == "T").Count());
        }

        [Fact]
        public void Calls()
        {
            string code = @"
            class A
            {
                public A()
                {
                    _b = new B();
                }
                public void f()
                {
                    return _b.g();
                }
                private B _b;
            }
            class B
            {
                public void g() { }
            }
            ";
            
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            var registry = new DeclarationRegistry(tree);
            
            ITypeExtraction b = new ClassExtraction(registry, "B");
            MethodExtraction g1 = b.GetMethods().First();
            MethodExtraction f = g1.GetCallees().First();
            MethodExtraction g2 = f.GetCalls().First();

            Assert.Equal("g", g1.Name);
            Assert.Equal("f", f.Name);
            Assert.Equal("g", g2.Name);
        }

        [Fact]
        public void MethodTypes()
        {
            string code = @"
                class A
                {
                    public B f(C c)
                    {
                        return new B();
                    }
                }

                class B { }
                struct C { }
            ";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            var registry = new DeclarationRegistry(tree);

            var method = new MethodExtraction(registry, "f");

            ITypeExtraction b = method.GetReturnTypes().First();
            ITypeExtraction c = method.GetArgumentTypes().First();

            Assert.True(b is ClassExtraction);
            Assert.True(c is StructExtraction);
        }


        [Fact]
        public void ParameterizedMethodTypes()
        {
            string code = @"
                class A<T> { }
                class C { }
                class B
                {
                    public A<C> f(A<C> a)
                    {
                        var a = new A<C>();
                        return a;
                    }
                }
            ";
            
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            var registry = new DeclarationRegistry(tree);

            var method = new MethodExtraction(registry, "f");
            List<ITypeExtraction> returns = method.GetReturnTypes().ToList();
            List<ITypeExtraction> arguments = method.GetArgumentTypes().ToList();
            
            Assert.Equal(2, returns.Count());
            Assert.Equal(1, returns.Where(rtrn => rtrn.Name == "A").Count());
            Assert.Equal(1, returns.Where(rtrn => rtrn.Name == "C").Count());
            
            Assert.Equal(2, arguments.Count());
            Assert.Equal(1, arguments.Where(arg => arg.Name == "A").Count());
            Assert.Equal(1, arguments.Where(arg => arg.Name == "C").Count());
        }

        [Fact]
        public void DoNotCrashWithUnknownTypesOfProperties()
        {
            string code = @"
                class A
                {
                    public string Prop { get; }
                }
            ";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            var registry = new DeclarationRegistry(tree);

            var clss = new ClassExtraction(registry, "A");

            var properties = clss.GetReferenced();
            Assert.Empty(properties);
        }

    }


    
}
