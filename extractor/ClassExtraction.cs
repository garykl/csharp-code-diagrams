using System.Collections.Generic;

namespace extractor
{
    public class ClassExtraction : ITypeExtraction
    {
        public string Name { get; }

        public ITypeExtraction GetParent() { }

        public IEnumerable<ITypeExtraction> GetFieldsAndProperties() { yield break; }

        public IEnumerable<MethodExtraction> GetMethods() { yield break; }
    }
}