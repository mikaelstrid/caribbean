using System.Collections.Generic;

namespace Caribbean.Models.Database
{
    public class Page
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public string PageTemplateSlug { get; set; }
        public string ThumbnailName { get; set; }
        public string ThumbnailUrl { get; set; }

        public string PdfName { get; set; }
        public string PdfUrl { get; set; }

        public int PrintId { get; set; }

        public virtual Print Print { get; set; }
        public virtual ICollection<FieldValue> FieldValues { get; set; }
    }
}