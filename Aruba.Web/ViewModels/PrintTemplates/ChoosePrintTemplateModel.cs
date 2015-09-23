using System.Collections.Generic;

namespace Caribbean.Aruba.Web.ViewModels.PrintTemplates
{
    public class ChoosePrintTemplateModel : FullTopbarLayoutViewModel
    {
        public string SelectedObjectId { get; set; }
        public IEnumerable<AvailablePrintTemplateViewModel> AvailablePrintTypes { get; set; }
    }
}