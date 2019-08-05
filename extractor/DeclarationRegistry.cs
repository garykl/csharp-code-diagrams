using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace extractor
{
    public class DeclarationRegistry
    {
        public List<MethodDeclarationSyntax> Methods { get; private set; }
        public IEnumerable<TypeDeclarationSyntax> Types { get; private set; }

        public SemanticModel Model { get; private set; }

        public DeclarationRegistry(SyntaxTree tree)
        {
            var compilation = CSharpCompilation.Create("not an assembly").AddSyntaxTrees(new SyntaxTree[] { tree });
            Model = compilation.GetSemanticModel(tree);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            List<SyntaxNode> allNodes = root.DescendantNodes().ToList();
            
            Methods = allNodes.OfType<MethodDeclarationSyntax>().ToList();
            _interfaces = new SpecificRegistry<InterfaceDeclarationSyntax>(allNodes);
            _classes = new SpecificRegistry<ClassDeclarationSyntax>(allNodes);
            _structs = new SpecificRegistry<StructDeclarationSyntax>(allNodes);
            Types = allNodes.OfType<TypeDeclarationSyntax>();
        }

        public TypeDeclarationSyntax GetIType(string name)
            => (TypeDeclarationSyntax)GetInterface(name)
            ?? (TypeDeclarationSyntax)GetClass(name)
            ?? (TypeDeclarationSyntax)GetStruct(name);


        public MethodDeclarationSyntax GetMethod(string name)
            => Methods.Where(declaration => declaration.Identifier.ValueText == name).FirstOrDefault();

        private InterfaceDeclarationSyntax GetInterface(string name) => _interfaces.GetDeclaration(name);
        private ClassDeclarationSyntax GetClass(string name) => _classes.GetDeclaration(name);
        private StructDeclarationSyntax GetStruct(string name) => _structs.GetDeclaration(name);


        private SpecificRegistry<InterfaceDeclarationSyntax> _interfaces;
        private SpecificRegistry<ClassDeclarationSyntax> _classes;
        private SpecificRegistry<StructDeclarationSyntax> _structs;

    }

    public class SpecificRegistry<TDeclarationSyntax> where TDeclarationSyntax : TypeDeclarationSyntax
    {
        public SpecificRegistry(List<SyntaxNode> nodes)
        {
            _declarations = nodes.OfType<TDeclarationSyntax>().ToList();
        }

        public TDeclarationSyntax GetDeclaration(string name)
            => _declarations.Where(declaration => declaration.Identifier.ValueText == name).FirstOrDefault();

        private List<TDeclarationSyntax> _declarations;
    }
}