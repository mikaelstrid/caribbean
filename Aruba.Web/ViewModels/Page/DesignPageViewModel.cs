using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.Page
{
    public class DesignPageViewModel
    {
        public int PrintId { get; set; }
        public PageViewModel Page { get; set; }
        public IEnumerable<ObjectImageViewModel> ObjectImages { get; set; }

        public class PageViewModel
        {
            public int Id { get; set; }
            public int Position { get; set; }
            public string PreviewUrl { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public double AspectRatioInPercent => ((double) Height/(double) Width) * 100;
        }

        public class ObjectImageViewModel
        {
            public string PictureUrl { get; set; }
            public string ThumbnailUrl { get; set; }
        }
    }
}