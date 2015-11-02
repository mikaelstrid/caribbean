using System;
using System.Collections.Generic;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.ViewModels.Prints
{
    public class IndexPrintsViewModel : FullTopbarLayoutViewModel
    {
        public IndexPrintsViewModel()
        {
            ActiveMenuItem = MenuItem.Trycksaksoversikt;
        }

        public IEnumerable<PrintViewModel> Prints { get; set; }

        public class PrintViewModel
        {
            public int Id { get; set; }
            public string Address { get; set; }
            public string ThumbnailUrl { get; set; }
            public string TemplateType { get; set; }
            public string TemplateName { get; set; }
            public PrintStatus Status { get; set; }
            public DateTime CreationTimeUtc { get; set; }
            public string PdfUrl { get; set; }
            public JobStatus PdfStatus { get; set; }
        }
    }
}