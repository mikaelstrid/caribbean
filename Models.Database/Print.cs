using System;
using System.Collections.Generic;

namespace Caribbean.Models.Database
{
    public class Print
    {
        public int Id { get; set; }
        public string PrintVariantSlug { get; set; }
        public string ObjectId { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime ModifiedTimeUtc { get; set; }
        public PrintStatus Status { get; set; }

        public string PdfName { get; set; }
        public string PdfUrl { get; set; }

        public int AgentId { get; set; }

        public ICollection<Page> Pages { get; set; }
    }
}