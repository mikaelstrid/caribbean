using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.RealEstateObject
{
    public class ChooseObjectViewModel
    {
        public IEnumerable<ObjectSummaryViewModel> AvailableObjects { get; set; }
        public string PrintVariantSlug { get; set; }
    }
}