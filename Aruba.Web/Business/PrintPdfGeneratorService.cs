using System;
using System.IO;
using System.Linq;
using System.Net;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.Models.Database;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Caribbean.Aruba.Web.Business
{
    public interface IPrintPdfGeneratorService
    {
        PrintPdfGeneratorService.GeneratePdfResult GeneratePdf(Print print);
    }

    public class PrintPdfGeneratorService : IPrintPdfGeneratorService
    {
        private readonly IPrintPdfRepository _printPdfRepository;

        public PrintPdfGeneratorService(IPrintPdfRepository printPdfRepository)
        {
            _printPdfRepository = printPdfRepository;
        }

        public GeneratePdfResult GeneratePdf(Print print)
        {
            var outputDocument = new PdfDocument();
            foreach (var page in print.Pages.OrderBy(p => p.Position))
            {
                var inputDocument = CreatePdfDocumentForPage(page);
                if (inputDocument != null && inputDocument.PageCount > 0) outputDocument.AddPage(inputDocument.Pages[0]);
            }
            var pdfMemoryStream = new MemoryStream();
            outputDocument.Save(pdfMemoryStream, false);

            var pdfInfo = _printPdfRepository.Add(pdfMemoryStream, $"{print.Id}-{Guid.NewGuid().ToString("N")}.pdf");

            return new GeneratePdfResult { PdfName = pdfInfo.Name, PdfUrl = pdfInfo.Url };
        }

        private static PdfDocument CreatePdfDocumentForPage(Page page)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var webClient = new WebClient())
                {
                    using (var inputStream = webClient.OpenRead(page.PdfUrl))
                    {
                        if (inputStream != null)
                        {
                            inputStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            return PdfReader.Open(memoryStream, PdfDocumentOpenMode.Import);
                        }
                    }
                }
            }

            return null;
        }

        public class GeneratePdfResult
        {
            public string PdfName { get; set; }
            public string PdfUrl { get; set; }
        }
    }
}