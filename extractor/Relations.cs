using System;
using System.Collections.Generic;
using System.Linq;
using extractor;
using Microsoft.CodeAnalysis;

namespace relations
{
    public enum RelationType {
        Parents,
        Referenced,
        MethodReturns,
        MethodArgs,
        Callers,
        Referencing,
        Children,
        MethodReturned,
        UsedAsArg,
        Callees
    }

    public struct Relation
    {
        public ITypeExtraction From { get; private set; } 
        public ITypeExtraction To { get; private set; }  
        public RelationType Kind { get; private set; }
  
        public Relation(ITypeExtraction from, ITypeExtraction to, RelationType kind)
        {
            if (from == null || to == null) {
                throw new InvalidProgramException("From or to node of relation cannot be null");
            }
            From = from;
            To = to;
            Kind = kind;
        }

        public override bool Equals(object obj)
            => (obj is Relation other) ? From.Name.Equals(other.From.Name) &&
                                         To.Name.Equals(other.To.Name) &&
                                         Kind == other.Kind
                                       : false;


        public override int GetHashCode()
         => From.Name.GetHashCode() * To.Name.GetHashCode() * Kind.GetHashCode();
    }

    public class RelationRegistry
    {
        public RelationRegistry(DeclarationRegistry registry) => _registry = registry;
        
        public IEnumerable<Relation> GetRelations(string name, RelationType type)
        {
            var extraction = TypeExtraction.CreateTypeExtraction(_registry, name);

            IEnumerable<ITypeExtraction> types = null;;

            if (extraction != null) {
                switch (type)
                {
                    case RelationType.Parents:
                        types = extraction.GetParents();
                        break;
                    case RelationType.Children:
                        types = ReversedExtraction.Reversed(_registry, name, extr => extr.GetParents());
                        break;
                    case RelationType.Referenced:
                        types = extraction.GetReferenced();
                        break;
                    case RelationType.Referencing:
                        types = ReversedExtraction.Reversed(_registry, name, extr => extr.GetReferenced());
                        break;
                    case RelationType.MethodReturns:
                        types = MethodReturns(extraction);
                        break;
                    case RelationType.MethodReturned:
                        types = ReversedExtraction.Reversed(_registry, name, MethodReturns);
                        break;
                    case RelationType.MethodArgs:
                        types = MethodArgs(extraction);
                        break;
                    case RelationType.UsedAsArg:
                        types = ReversedExtraction.Reversed(_registry, name, MethodArgs);
                        break;
                    case RelationType.Callers:
                        types = GetCallers(extraction);
                        break;
                    case RelationType.Callees:
                        types = ReversedExtraction.Reversed(_registry, name, GetCallers);
                        break;
                    default:
                        throw new NotImplementedException($"no types can be extracted for relation of type {type}");
                }
            }

            return types?.Select(extr => new Relation(from: extraction, to: extr, kind: type)) ?? new Relation[] { };
        }

        private IEnumerable<ITypeExtraction> GetCallers(ITypeExtraction extraction)
            => extraction.GetMethods().SelectMany(method => method.GetCallerTypes());

        private IEnumerable<ITypeExtraction> MethodArgs(ITypeExtraction extraction)
            => extraction.GetMethods().SelectMany(method => method.GetArgumentTypes());

        private IEnumerable<ITypeExtraction> MethodReturns(ITypeExtraction extraction)
            => extraction.GetMethods().SelectMany(method => method.GetReturnTypes());


        private DeclarationRegistry _registry;
    }
}