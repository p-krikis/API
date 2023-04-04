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
                dataTable.Columns.Add($"{module.Device.Name}");
                foreach (var dataset in module.Datasets)
                {
                    dataTable.Columns.Add(dataset.Label);
                }
                for (int i = 0; i < module.Labels.Length; i++)
                {
                    var newRow = dataTable.NewRow();
                    newRow[$"{module.Device.Name}"] = module.Labels[i];
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
                    Text headerText = new Text(column.ColumnName).SetBold();
                    headerCell.Add(new Paragraph(headerText));
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
                document.Add(new AreaBreak());
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
                    Text headerText = new Text(column.ColumnName).SetBold();
                    headerCell.Add(new Paragraph(headerText));
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
        private void CreateHeader(Module module, Document document)
        {
            document.Add(new Paragraph($"{module.Title}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SetBold());
            document.Add(new Paragraph($"{module.Text}").SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph(" "));
        }

        public void buildPdf(List<Module> modules)
        {
            string pngFolderPath = @"C:\Users\praktiki1\Desktop\APIdump\PNGs"; //image sauce
            string pdfPath = @"C:\Users\praktiki1\Desktop\APIdump\PDFs\report.pdf"; //pdf dump loc
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                float parentWidth = modules[0].ParentWidth;
                float parentHeight = modules[0].ParentHeight;
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                pdfDocument.SetDefaultPageSize(new PageSize(parentWidth, parentHeight));
                Document document = new Document(pdfDocument);
                List<Tuple<ImageData, float, float>> images = new List<Tuple<ImageData, float, float>>();
                Dictionary<string, int> fileCounters = new Dictionary<string, int>();
                foreach (var module in modules)
                {
                    if (string.IsNullOrEmpty(module.Type))
                    {
                        CreateHeader(module, document);
                    }
                    else if (module.Type == "table")
                    {
                        CreateTable(module, document);
                        //document.Add(new AreaBreak());
                    }
                    else if (module.Type == "panel")
                    {
                        CreatePanelTable(module, document);
                        //document.Add(new AreaBreak());
                    }
                    else
                    {
                        string key = $"{module.Aggregate}_{module.Type}";
                        int fileCounter = fileCounters.ContainsKey(key) ? fileCounters[key] : 0;

                        string imagePath = System.IO.Path.Combine(pngFolderPath, $"{module.Aggregate}_{module.Type}_chart{fileCounter}.png");

                        // Loop through all the files with the same prefix until you find one that doesn't exist
                        while (File.Exists(imagePath))
                        {
                            float x = module.Left;
                            float y = module.Top;
                            ImageData imageData = ImageDataFactory.Create(imagePath);
                            images.Add(new Tuple<ImageData, float, float>(imageData, x, y));

                            fileCounter++;
                            imagePath = System.IO.Path.Combine(pngFolderPath, $"{module.Aggregate}_{module.Type}_chart{fileCounter}.png");
                        }
                        if (fileCounters.ContainsKey(key))
                        {
                            fileCounters[key] = fileCounter;
                        }
                        else
                        {
                            fileCounters.Add(key, fileCounter);
                        }
                    }
                }
                document.Add(new AreaBreak());
                foreach (var image in images)
                {
                    int lastPageNumber = pdfDocument.GetNumberOfPages(); //used to always print images to last page
                    Image pdfImage = new(image.Item1);
                    pdfImage.SetFixedPosition(image.Item2, pdfDocument.GetPage(lastPageNumber).GetPageSize().GetHeight() - image.Item3 - pdfImage.GetImageHeight());
                    document.Add(pdfImage);
                }
                document.Close();
            }
        }
    }
}