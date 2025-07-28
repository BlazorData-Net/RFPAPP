using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Threading.Tasks;

namespace BlazorWebAssemblyPDF.Services
{
    public class PdfToPngService
    {
        private readonly IJSRuntime _jsRuntime;
        public PdfToPngService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetPdfDataUrlAsync(IBrowserFile pdfFile)
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

        public async Task SaveCanvasAsPngAsync(string canvasId)
        {
            await _jsRuntime.InvokeVoidAsync("pdfInterop.saveCanvasAsPng", canvasId);
        }
    }
}