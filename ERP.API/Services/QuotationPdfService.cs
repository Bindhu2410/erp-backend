using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ERP.API.Models;
using Microsoft.AspNetCore.Hosting;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RazorLight;

namespace ERP.API.Services
{
    // Real PDF rendering: render Razor view to HTML (RazorLight) then use PuppeteerSharp
    // to generate a true PDF. This will download Chromium on first use.
    public class QuotationPdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly RazorLightEngine _razor;

        public QuotationPdfService(IWebHostEnvironment env)
        {
            _env = env;
            _razor = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(_env.ContentRootPath, "Views", "Templates"))
                .UseMemoryCachingProvider()
                .Build();
        }

        private static async Task EnsureBrowserDownloadedAsync()
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
        }

        public async Task<byte[]> GeneratePdfAsync(QuotationModel model, string templateName = "Quotation.cshtml")
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Render HTML from Razor template
            // Use the non-generic overload to avoid type identity issues between assemblies
            string html = await _razor.CompileRenderAsync(templateName, (object)model);

            // Ensure Chromium is available (downloads on first run)
            await EnsureBrowserDownloadedAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "10mm", Bottom = "10mm", Left = "12mm", Right = "12mm" }
            };

            var pdf = await page.PdfDataAsync(pdfOptions);
            return pdf;
        }
    }
}
