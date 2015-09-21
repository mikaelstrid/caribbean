using System;
using System.Collections.Generic;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.ViewModels.Prints
{
    public class IndexPrintsViewModel
    {
        public IEnumerable<PrintViewModel> Prints { get; set; }

        public class PrintViewModel
        {
            public string Address { get; set; }
            public string ThumbnailUrl { get; set; }
            public string TemplateType { get; set; }
            public string TemplateName { get; set; }
            public PrintStatus Status { get; set; }
            public DateTime CreationTimeUtc { get; set; }
            public string PdfUrl { get; set; }
        }
    }
}