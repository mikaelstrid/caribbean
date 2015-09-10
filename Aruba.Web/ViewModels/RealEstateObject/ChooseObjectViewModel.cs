using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.RealEstateObject
{
    public class ChooseObjectViewModel
    {
        public string SelectedPrintTemplateSlug { get; set; }
        public IEnumerable<ObjectSummaryViewModel> AvailableObjects { get; set; }
    }
}