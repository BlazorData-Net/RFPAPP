using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace BlazorWebAssemblyPDF.Services
{
    public class PdfToPngService
    {
        private readonly IJSRuntime _jsRuntime;
        public PdfToPngService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> GetPdfDataUrlAsync(IBrowserFile pdfFile)
        {
            if (pdfFile == null) return null;
            using var stream = pdfFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB limit
            var buffer = new byte[pdfFile.Size];
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead));
                if (bytesRead == 0)
                {
                    break; // End of stream
                }
                totalRead += bytesRead;
            }
            return "data:application/pdf;base64," + Convert.ToBase64String(buffer);
        }

        public async Task RenderPdfToCanvasAsync(string pdfDataUrl, string canvasId)
        {
            await _jsRuntime.InvokeVoidAsync("pdfInterop.loadPdf", pdfDataUrl, canvasId);
        }

        // Returns PNG bytes from the canvas
        public async Task<byte[]> GetCanvasPngBytesAsync(string canvasId)
        {
            var dataUrl = await _jsRuntime.InvokeAsync<string>("pdfInterop.getCanvasPngDataUrl", canvasId);
            // dataUrl is like "data:image/png;base64,..."
            var base64 = dataUrl.Substring(dataUrl.IndexOf(",") + 1);
            return Convert.FromBase64String(base64);
        }

        public async Task<List<byte[]>> GetPdfPagesAsPngBytesAsync(IBrowserFile pdfFile)
        {
            var pdfDataUrl = await GetPdfDataUrlAsync(pdfFile);
            if (pdfDataUrl == null) return new List<byte[]>();
            var dataUrls = await _jsRuntime.InvokeAsync<string[]>("pdfInterop.getPdfPagesAsPng", pdfDataUrl);
            var images = new List<byte[]>();
            foreach (var dataUrl in dataUrls)
            {
                var base64 = dataUrl.Substring(dataUrl.IndexOf(",") + 1);
                images.Add(Convert.FromBase64String(base64));
            }
            return images;
        }

        // SaveCanvasAsPngAsync can remain for download, but use GetCanvasPngBytesAsync for bytes
        public async Task SaveCanvasAsPngAsync(string canvasId)
        {
            await _jsRuntime.InvokeVoidAsync("pdfInterop.saveCanvasAsPng", canvasId);
        }
    }
}