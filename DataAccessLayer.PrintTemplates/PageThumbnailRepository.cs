namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public interface IPageThumbnailRepository
    {
        void Delete(string name);
    }

    public class PageThumbnailRepository : BlobStorageRepositoryBase, IPageThumbnailRepository
    {
        public void Delete(string name)
        {
            var container = GetContainer("pagethumbnails");
            if (container == null) return;
            var blob = container.GetBlockBlobReference(name);
            blob.Delete();
        }
    }
}