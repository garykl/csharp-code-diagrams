using System.Collections.Generic;

namespace extractor
{
    public class MethodExtraction
    {
        public string Name { get; }

        public IEnumerable<MethodExtraction> GetCallees() { yield break; }
    }
}