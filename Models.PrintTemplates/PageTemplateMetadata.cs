namespace Caribbean.Models.PrintTemplates
{
    public class PageTemplateMetadata
    {
        public string StorageUri { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Dpi { get; set; }

        private string _reason;
        public string Reason
        {
            get { return string.IsNullOrWhiteSpace(_reason) ? "Name is undefined." : _reason; }
            set { _reason = value; }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Reason) && !string.IsNullOrWhiteSpace(Name);
        
        public static PageTemplateMetadata CreateInvalid(string reason)
        {
            return CreateInvalid(reason, null);
        }

        public static PageTemplateMetadata CreateInvalid(string reason, string storageUri)
        {
            return new PageTemplateMetadata { Reason = reason, StorageUri = storageUri };
        }
    }
}