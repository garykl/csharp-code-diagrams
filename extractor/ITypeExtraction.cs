using System.Collections.Generic;

namespace extractor
{
    public interface INamed
    {
        string Name { get; }
    }

    public interface ITypeExtraction : INamed
    {
        IEnumerable<ITypeExtraction> GetParents();
        IEnumerable<ITypeExtraction> GetReferenced();
        IEnumerable<MethodExtraction> GetMethods();
    }
}