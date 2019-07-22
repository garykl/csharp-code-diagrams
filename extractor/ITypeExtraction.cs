using System.Collections.Generic;

namespace extractor
{
    public interface ITypeExtraction
    {
        IEnumerable<ITypeExtraction> GetParents();
    }
}