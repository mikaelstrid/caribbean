namespace Caribbean.Aruba.Web.ViewModels.Prints
{
    public class OrderPrintViewModel
    {
        public bool AllPagePdfsGenerated { get; set; }
        public string PrintPdfUrl { get; set; }

        public bool PrintGenerationOk => !string.IsNullOrWhiteSpace(PrintPdfUrl);
        public bool PrintPdfValid => AllPagePdfsGenerated && PrintGenerationOk;
    }
}