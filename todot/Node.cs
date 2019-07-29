using System;

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
}
