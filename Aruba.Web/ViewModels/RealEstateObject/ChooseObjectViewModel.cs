using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.RealEstateObject
{
    public class ChooseObjectViewModel
    {
        public string SelectedPrintTemplateName { get; set; }
        public IEnumerable<ObjectSummaryViewModel> AvailableObjects { get; set; }
    }
}