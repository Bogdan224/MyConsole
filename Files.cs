namespace MyConsole2
{
    public interface IFile;

    public class ProductsListFile(ProductFileNote productFileNote) : IFile
    {
        public ProductFileNote FileNote { get; set; } = productFileNote;
    }

    public class SpecificationsListFile(SpecificationFileNote productFileNote) : IFile
    {
        public SpecificationFileNote FileNote { get; set; } = productFileNote;
    }
}
