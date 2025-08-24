window.pdfInterop = {
    setupTextAreaMonitoring: function(textAreaSelector, dotNetRef, methodName, debounceMs = 300) {
        const textArea = document.querySelector(textAreaSelector);
        if (!textArea) {
            console.error("TextArea not found with selector:", textAreaSelector);
            return;
        }

        let debounceTimer;
        
        const handleInput = function(event) {
            const currentValue = event.target.value;
            
            // Clear existing timer
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
            
            // Set new timer
            debounceTimer = setTimeout(() => {
                dotNetRef.invokeMethodAsync(methodName, currentValue);
            }, debounceMs);
        };

        // Add event listener for input changes
        textArea.addEventListener('input', handleInput);
        
        // Return cleanup function
        return function cleanup() {
            textArea.removeEventListener('input', handleInput);
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
        };
    },

    removeTextAreaMonitoring: function(textAreaSelector) {
        const textArea = document.querySelector(textAreaSelector);
        if (textArea) {
            // Clone the element to remove all event listeners
            const newTextArea = textArea.cloneNode(true);
            textArea.parentNode.replaceChild(newTextArea, textArea);
        }
    },

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
    },
    getCanvasPngDataUrl: function(canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return null;
        return canvas.toDataURL('image/png');
    },
    getPdfPagesAsPng: async function (pdfUrl) {
        if (!window.pdfjsLib) {
            window.pdfjsLib = await import('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.4.54/pdf.min.mjs');
        }
        window.pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/5.4.54/pdf.worker.min.mjs';

        const loadingTask = window.pdfjsLib.getDocument({ url: pdfUrl });
        const pdf = await loadingTask.promise;
        const images = [];
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        const scale = 2.0;

        for (let pageNumber = 1; pageNumber <= pdf.numPages; pageNumber++) {
            const page = await pdf.getPage(pageNumber);
            const viewport = page.getViewport({ scale });
            canvas.height = viewport.height;
            canvas.width = viewport.width;
            context.clearRect(0, 0, canvas.width, canvas.height);
            await page.render({ canvasContext: context, viewport: viewport }).promise;
            images.push(canvas.toDataURL('image/png'));
        }

        return images;
    },
    exportElementToPdf: async function(elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error("Element not found:", elementId);
            return;
        }

        // Check if html2canvas is available
        if (!window.html2canvas) {
            console.error("html2canvas is not loaded. Ensure the library is included.");
            return;
        }

        // Initialize jsPDF if needed
        if (!window.jsPDF) {
            if (window.jspdf && window.jspdf.jsPDF) {
                window.jsPDF = window.jspdf.jsPDF;
            } else {
                console.error("jsPDF is not loaded. Ensure the library is included.");
                return;
            }
        }

        try {
            const canvas = await window.html2canvas(element, {
                scale: 1,
                useCORS: true,
                allowTaint: true
            });
            
            const imgData = canvas.toDataURL('image/png');
            const pdf = new window.jsPDF({
                orientation: 'landscape',
                unit: 'pt',
                format: [canvas.width, canvas.height]
            });
            
            pdf.addImage(imgData, 'PNG', 0, 0, canvas.width, canvas.height);
            pdf.save('capacity-chart.pdf');
        } catch (error) {
            console.error("Error generating PDF:", error);
        }
    },
    exportRoomsToPdf: function (rooms) {
        if (!rooms || rooms.length === 0) {
            console.error("No rooms provided for PDF export.");
            return;
        }

        if (!window.jsPDF) {
            if (window.jspdf && window.jspdf.jsPDF) {
                window.jsPDF = window.jspdf.jsPDF;
            } else {
                console.error("jsPDF is not loaded. Ensure the library is included.");
                return;
            }
        }

        const doc = new window.jsPDF();
        const lineHeight = 10;
        let y = 10;

        rooms.forEach(room => {
            const name = room.name || room.Name || "";
            const squareFeet = room.squareFeet ?? room.SquareFeet ?? "";
            const length = room.length ?? room.Length ?? "";
            const width = room.width ?? room.Width ?? "";
            const ceiling = room.ceilingHeight ?? room.CeilingHeight ?? "";
            const floor = room.floorLevel ?? room.FloorLevel ?? "";
            const natural = (room.hasNaturalLight ?? room.HasNaturalLight) ? "Yes" : "No";
            const pillars = (room.hasPillars ?? room.HasPillars) ? "Yes" : "No";

            const capacities = room.capacities || room.Capacities || {};
            const banquet = capacities.banquet ?? capacities.Banquet ?? 0;
            const conference = capacities.conference ?? capacities.Conference ?? 0;
            const square = capacities.square ?? capacities.Square ?? 0;
            const reception = capacities.reception ?? capacities.Reception ?? 0;
            const schoolRoom = capacities.schoolRoom ?? capacities.SchoolRoom ?? 0;
            const theatre = capacities.theatre ?? capacities.Theatre ?? 0;
            const uShape = capacities.uShape ?? capacities.UShape ?? 0;
            const hollowSquare = capacities.hollowSquare ?? capacities.HollowSquare ?? 0;
            const boardroom = capacities.boardroom ?? capacities.Boardroom ?? 0;
            const crescentRounds = capacities.crescentRounds ?? capacities.CrescentRounds ?? 0;

            const headerLines = doc.splitTextToSize(`Name: ${name}`, 180);
            doc.text(headerLines, 10, y);
            y += headerLines.length * lineHeight;

            const detailText = `Square Feet: ${squareFeet} Length: ${length} Width: ${width} Ceiling Height: ${ceiling} Floor Level: ${floor} Natural Light: ${natural} Has Pillars: ${pillars}`;
            const detailLines = doc.splitTextToSize(detailText, 180);
            doc.text(detailLines, 10, y);
            y += detailLines.length * lineHeight;

            const capacityText = `Banquet: ${banquet} Conference: ${conference} Square: ${square} Reception: ${reception} School Room: ${schoolRoom} Theatre: ${theatre} U-Shape: ${uShape} Hollow Square: ${hollowSquare} Boardroom: ${boardroom} Crescent Rounds: ${crescentRounds}`;
            const capacityLines = doc.splitTextToSize(capacityText, 180);
            doc.text(capacityLines, 10, y);
            y += capacityLines.length * lineHeight + lineHeight;

            if (y > 280) {
                doc.addPage();
                y = 10;
            }
        });

        doc.save('parent-rooms.pdf');
    }
    ,
    downloadRoomsCsv: function (rooms) {
        if (!rooms || rooms.length === 0) {
            console.error("No rooms provided for CSV export.");
            return;
        }

        const headers = [
            'Name','SquareFeet','Length','Width','CeilingHeight','FloorLevel','NaturalLight','HasPillars',
            'Banquet','Conference','Square','Reception','SchoolRoom','Theatre','UShape','HollowSquare','Boardroom','CrescentRounds'
        ];

        const rows = [headers.join(',')];

        rooms.forEach(room => {
            const capacities = room.capacities || room.Capacities || {};
            const row = [
                room.name || room.Name || '',
                room.squareFeet ?? room.SquareFeet ?? '',
                room.length ?? room.Length ?? '',
                room.width ?? room.Width ?? '',
                room.ceilingHeight ?? room.CeilingHeight ?? '',
                room.floorLevel ?? room.FloorLevel ?? '',
                (room.hasNaturalLight ?? room.HasNaturalLight) ? 'Yes' : 'No',
                (room.hasPillars ?? room.HasPillars) ? 'Yes' : 'No',
                capacities.banquet ?? capacities.Banquet ?? 0,
                capacities.conference ?? capacities.Conference ?? 0,
                capacities.square ?? capacities.Square ?? 0,
                capacities.reception ?? capacities.Reception ?? 0,
                capacities.schoolRoom ?? capacities.SchoolRoom ?? 0,
                capacities.theatre ?? capacities.Theatre ?? 0,
                capacities.uShape ?? capacities.UShape ?? 0,
                capacities.hollowSquare ?? capacities.HollowSquare ?? 0,
                capacities.boardroom ?? capacities.Boardroom ?? 0,
                capacities.crescentRounds ?? capacities.CrescentRounds ?? 0
            ];
            rows.push(row.join(','));
        });

        const csvContent = rows.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', 'rooms.csv');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
};