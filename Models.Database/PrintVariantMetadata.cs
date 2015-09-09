namespace Caribbean.Models.Database
{
    public class PrintVariantMetadata
    {
        public string StorageUri { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string ThumbnailUrl { get; set; }
        public string[] ProposedPageSlugs { get; set; }
        public string[] AvailablePageTemplateSlugs { get; set; }

        private string _reason;
        public string Reason
        {
            get { return string.IsNullOrWhiteSpace(_reason) ? "Either name or type is undefined." : _reason; }
            set { _reason = value; }
        }

        public bool IsValid
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(Reason) &&
                    !string.IsNullOrWhiteSpace(Name) &&
                    !string.IsNullOrWhiteSpace(Type);
            }
        }


        public static PrintVariantMetadata CreateInvalid(string reason)
        {
            return CreateInvalid(reason, null);
        }
        public static PrintVariantMetadata CreateInvalid(string reason, string storageUri)
        {
            return new PrintVariantMetadata { Reason = reason, StorageUri = storageUri };
        }
    }
}