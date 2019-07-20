namespace extractor
{
    public class InterfaceExtraction : ITypeExtraction
    {
        public string Name { get; }

        public ITypeExtraction GetParent() { }
    }
}