window.pdfInterop = {
    loadPdf: async function (pdfUrl, canvasId) {
        // Dynamically import PDF.js as an ES module
        if (!window.pdfjsLib) {
            window.pdfjsLib = await import('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.4.54/pdf.min.mjs');
        }

        // Set workerSrc to the CDN .mjs worker
        window.pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.4.54/pdf.worker.min.mjs';

        // Accepts either a URL or a data URL
        const loadingTask = window.pdfjsLib.getDocument({ url: pdfUrl });
        const pdf = await loadingTask.promise;
        const page = await pdf.getPage(1);

        const scale = 2.0;
        const viewport = page.getViewport({ scale });

        const canvas = document.getElementById(canvasId);
        const context = canvas.getContext('2d');
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };
        await page.render(renderContext).promise;
    },
    saveCanvasAsPng: function(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const image = canvas.toDataURL('image/png');
        const link = document.createElement('a');
        link.href = image;
        link.download = 'pdf-page.png';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};