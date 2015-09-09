using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public abstract class BlobStorageRepositoryBase
    {
        protected static CloudBlobContainer GetContainer(string containerName)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                return container.Exists() ? container : null;
            }
            catch
            {
                return null;
            }
        }
    }
}