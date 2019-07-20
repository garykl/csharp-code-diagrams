namespace extractor
{
    public interface ITypeExtraction
    {
        string Name { get; }

        ITypeExtraction GetParent();
    }
}