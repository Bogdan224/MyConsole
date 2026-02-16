namespace MyConsole2
{
    public interface IFile;

    public class ProductsListFile : IFile
    {
        public ProductFileNote FileNote { get; set; }

        public ProductsListFile(ProductFileNote productFileNote)
        {
            FileNote = productFileNote;
        }
    }

    public class SpecificationsFile : IFile
    {
        private SpecificationFileNote fileNotes;
    }
}
