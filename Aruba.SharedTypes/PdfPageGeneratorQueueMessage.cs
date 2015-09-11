namespace Caribbean.Aruba.SharedTypes
{
    public class PdfPageGeneratorQueueMessage
    {
        public int PageId { get; set; }
        public string AgentUserId { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public int Dpi { get; set; }
    }
}
