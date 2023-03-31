using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ReportAppAPI.Models;
using System.Data;

namespace ReportAppAPI.Services
{
    public class PDFbuildService
    {
        private void CreateTable(Module module, Document document)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Labels");
            foreach (var dataset in module.Datasets)
            {
                dataTable.Columns.Add(dataset.Label);
            }
            for (int i = 0; i < module.Labels.Length; i++)
            {
                var newRow = dataTable.NewRow();
                newRow["Labels"] = module.Labels[i];
                for (int j = 0; j < module.Datasets.Length; j++)
                {
                    newRow[module.Datasets[j].Label] = module.Datasets[j].Data[i];
                }
                dataTable.Rows.Add(newRow);
            }
            Table pdfTable = new Table(dataTable.Columns.Count);
            pdfTable.SetWidth(UnitValue.CreatePercentValue(100));
            foreach (DataColumn column in dataTable.Columns)
            {
                Cell headerCell = new Cell();
                headerCell.Add(new Paragraph(column.ColumnName));
                pdfTable.AddHeaderCell(headerCell);
            }
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Cell dataCell = new Cell();
                    dataCell.Add(new Paragraph(item.ToString()));
                    pdfTable.AddCell(dataCell);
                }
            }
            document.Add(pdfTable);
        }
        private void CreatePanelTable(Module module, Document document)
        {
            int numColumns = 1;
            Table panelTable = new Table(numColumns, true);
            panelTable.AddHeaderCell(new Cell().Add(new Paragraph("The ").Add(module.Aggregate).Add(" value of ").Add(module.Datasets[0].Label)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            panelTable.AddCell(new Cell().Add(new Paragraph("The ").Add(module.Aggregate).Add(" value of ").Add(module.Datasets[0].Label).Add(" from ").Add(module.From).Add(" to ").Add(module.To).Add(" was ").Add(module.Datasets[0].Data[0].ToString())).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
            document.Add(panelTable);
        }
        public void buildPdf(List<Module> modules)
        {
            string pngFolderPath = @"C:\Users\praktiki1\Desktop\APIdump\PNGs"; //image sauce
            string pdfPath = @"C:\Users\praktiki1\Desktop\APIdump\PDFs\report.pdf"; //pdf dump loc
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                Document document = new Document(pdfDocument);
                foreach (var module in modules)
                {
                    if (module.Type == "table")
                    {
                        CreateTable(module, document);
                        document.Add(new AreaBreak());
                    }
                    else if (module.Type == "panel")
                    {
                        CreatePanelTable(module, document);
                    }
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(pngFolderPath);
                FileInfo[] graphImages = directoryInfo.GetFiles("*.png");
                int imageCounter = 0;
                foreach (FileInfo graphImage in graphImages)
                {
                    ImageData imageData = ImageDataFactory.Create(graphImage.FullName);
                    Image pdfImage = new Image(imageData);
                    pdfImage.SetAutoScale(true);
                    document.Add(pdfImage);
                    imageCounter++;
                    if (imageCounter % 2 == 0) //2 images per page
                    {
                        document.Add(new AreaBreak());
                    }
                }
                document.Close();
            }
        }
    }
}
