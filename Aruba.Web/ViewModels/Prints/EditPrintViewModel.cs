using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.Prints
{
    public class EditPrintViewModel
    {
        public int PrintId { get; set; }
        public IEnumerable<PageViewModel> Pages { get; set; }
        public string PrintVariantType { get; set; }
        public string RealEstateObjectId { get; set; }

        public class PageViewModel
        {
            public int Id { get; set; }
            public int Position { get; set; }
            public string PreviewUrl { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public double AspectRatioInPercent => ((double)Height / (double)Width) * 100;
        }
    }
}