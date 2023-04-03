using iText.IO.Image;
using iText.Kernel.Geom;
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
            if (string.IsNullOrEmpty(module.Aggregate))
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Dates");
                foreach (var dataset in module.Datasets)
                {
                    dataTable.Columns.Add(dataset.Label);
                }
                for (int i = 0; i < module.Labels.Length; i++)
                {
                    var newRow = dataTable.NewRow();
                    newRow["Dates"] = module.Labels[i];
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
            else
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add($"{module.Aggregate}");
                foreach (var dataset in module.Datasets)
                {
                    dataTable.Columns.Add(dataset.Label);
                }
                for (int i = 0; i < module.Labels.Length; i++)
                {
                    var newRow = dataTable.NewRow();
                    newRow[$"{module.Aggregate}"] = module.Labels[i];
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
            float pageWidth = 861f; //hardcoded page width 2x images (2x368px) + 2x padding (2x20px) left-right
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                pdfDocument.SetDefaultPageSize(new PageSize(pageWidth, PageSize.A4.GetHeight())); //hardcoded custom page size
                Document document = new Document(pdfDocument);
                List<Tuple<ImageData, float, float>> images = new List<Tuple<ImageData, float, float>>();
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
                        //document.Add(new AreaBreak());
                    }
                    else
                    {
                        float x = module.Left;
                        float y = module.Top;
                        string imagePath = System.IO.Path.Combine(pngFolderPath, $"{module.Aggregate}_{module.Type}_chart.png");
                        ImageData imageData = ImageDataFactory.Create(imagePath);
                        images.Add(new Tuple<ImageData, float, float>(imageData, x, y));
                    }
                }
                foreach (var image in images)
                {
                    int lastPageNumber = pdfDocument.GetNumberOfPages();
                    Image pdfImage = new(image.Item1);
                    pdfImage.SetFixedPosition(image.Item2, pdfDocument.GetPage(lastPageNumber).GetPageSize().GetHeight() - image.Item3 - pdfImage.GetImageHeight());
                    document.Add(pdfImage);
                }
                document.Close();
            }
        }
    }
}