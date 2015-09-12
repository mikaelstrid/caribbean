namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public interface IPagePdfRepository
    {
        void Delete(string name);
    }

    public class PagePdfRepository : BlobStorageRepositoryBase, IPagePdfRepository
    {
        public void Delete(string name)
        {
            var container = GetContainer("pagepdfs");
            if (container == null) return;
            var blob = container.GetBlockBlobReference(name);
            blob.Delete();
        }
    }
}