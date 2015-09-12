using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.Prints
{
    public class EditPrintViewModel
    {
        public int PrintId { get; set; }
        public IEnumerable<PageViewModel> Pages { get; set; }
        public string PrintVariantType { get; set; }

        public class PageViewModel
        {
            public int Id { get; set; }
            public int Position { get; set; }
            public string TemplateName { get; set; }
            public string ThumbnailUrl { get; set; }
        }
    }
}