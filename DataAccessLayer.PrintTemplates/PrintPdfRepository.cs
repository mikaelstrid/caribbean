using System.IO;
using Caribbean.Models.Database;

namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public interface IPrintPdfRepository
    {
        PdfInfo Add(Stream stream, string name);
        void Delete(string name);
    }

    public class PrintPdfRepository : BlobStorageRepositoryBase, IPrintPdfRepository
    {
        public PdfInfo Add(Stream stream, string name)
        {
            var container = GetContainer("printpdfs");
            if (container == null) return null;
            var blob = container.GetBlockBlobReference(name);
            blob.UploadFromStream(stream);
            return new PdfInfo {Name = blob.Name, Url = blob.Uri.ToString()};
        }

        public void Delete(string name)
        {
            var container = GetContainer("printpdfs");
            if (container == null) return;
            var blob = container.GetBlockBlobReference(name);
            blob.Delete();
        }
    }
}