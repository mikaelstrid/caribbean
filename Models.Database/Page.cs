using System;
using System.Collections.Generic;

namespace Caribbean.Models.Database
{
    public class Page
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public string PageTemplateSlug { get; set; }

        public Guid ThumbnailJobId { get; set; }
        public DateTime ThumbnailJobEnqueueTimeUtc { get; set; }
        public JobStatus ThumbnailJobStatus { get; set; }
        public DateTime ThumbnailJobCompletionTimeUtc { get; set; }
        public long ThumbnailJobDurationMs { get; set; }
        public string ThumbnailName { get; set; }
        public string ThumbnailUrl { get; set; }
        
        public Guid PdfJobId { get; set; }
        public DateTime PdfJobEnqueueTimeUtc { get; set; }
        public JobStatus PdfJobStatus { get; set; }
        public DateTime PdfJobCompletionTimeUtc { get; set; }
        public long PdfJobDurationMs { get; set; }
        public string PdfName { get; set; }
        public string PdfUrl { get; set; }

        public int PrintId { get; set; }

        public virtual Print Print { get; set; }
        public virtual ICollection<FieldValue> FieldValues { get; set; }
    }
}