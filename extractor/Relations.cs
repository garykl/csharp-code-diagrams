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
        Callees,
        Children,
        Referencing
    }

    public struct Relation
    {
      public ITypeExtraction From { get; set; } 
      public ITypeExtraction To { get; set; }  
      public RelationType Kind { get; set; }

      public Relation MapToBase()
      {
          if (Kind == RelationType.Children) {
              return new Relation {
                  From = To,
                  To = From,
                  Kind = RelationType.Parents
              };
          }
          if (Kind == RelationType.Callers) {
              return new Relation {
                  From = To,
                  To = From,
                  Kind = RelationType.Callees
              };
          }
          if (Kind == RelationType.Referenced) {
              return new Relation {
                  From = To,
                  To = From,
                  Kind = RelationType.Referencing
              };
          }
          return this;
      }

        public override bool Equals(object obj)
        {
            if (!(obj is Relation)) { return false; }
            Relation that = MapToBase();
            Relation other = ((Relation)obj).MapToBase();
            
            return that.From.Name.Equals(other.From.Name) &&
                   that.To.Name.Equals(other.To.Name) &&
                   that.Kind == other.Kind;
        }

        public override int GetHashCode()
            => From.Name.GetHashCode() * To.Name.GetHashCode() * Kind.GetHashCode();
    }

    public class RelationRegistry
    {
        public RelationRegistry(SyntaxTree tree) => _tree = tree;
        
        public IEnumerable<Relation> GetRelations(string name, RelationType type)
        {
            var extraction = TypeExtraction.CreateTypeExtraction(_tree, name);
            IEnumerable<ITypeExtraction> types = null;;

            switch (type)
            {
                case RelationType.Parents:
                    types = extraction.GetParents();
                    break;
                case RelationType.Children:
                    types = extraction.GetChildren();
                    break;
                case RelationType.Referenced:
                    types = extraction.GetReferenced();
                    break;
                case RelationType.Referencing:
                    types = extraction.GetReferencing();
                    break;
                case RelationType.MethodReturns:
                    types = extraction.GetMethods().SelectMany(method => method.GetReturnTypes());
                    break;
                case RelationType.MethodArgs:
                    types = extraction.GetMethods().SelectMany(method => method.GetArgumentTypes());
                    break;
                case RelationType.Callers:
                    types = extraction.GetMethods().SelectMany(method => method.GetCallerTypes());
                    break;
                case RelationType.Callees:
                    types = extraction.GetMethods().SelectMany(method => method.GetCalleeTypes());
                    break;
                default:
                    throw new NotImplementedException($"no types can be extracted for relation of type {type}");
            }

            return types.Select(extr => new Relation {
                        From = extraction,
                        To = extr,
                        Kind = type
                    });
        }


        private SyntaxTree _tree;
    }
}