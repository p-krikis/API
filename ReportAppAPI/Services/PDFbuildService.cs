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
        }

        private void CreatePanelTable(Module module, Document document)
        {
            int numColumns = 1;
            Table panelTable = new Table(numColumns, true);
            panelTable.AddHeaderCell(new Cell().Add(new Paragraph("The ").Add(module.Aggregate).Add(" value of ").Add(module.Datasets[0].Label)).SetTextAlignment(TextAlignment.CENTER));
            panelTable.AddCell(new Cell().Add(new Paragraph($"The {module.Aggregate} value of {module.Datasets[0].Label} from {module.From} to {module.To} was {module.Datasets[0].Data[0]}")).SetTextAlignment(TextAlignment.CENTER));
            document.Add(panelTable);
            document.Add(new AreaBreak());
        }

        private void CreateHeader(Module module, Document document)
        {
            document.Add(new Paragraph($"{module.Title}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SetBold());
            document.Add(new Paragraph($"{module.Text}").SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph(" "));
        }

        public string GetTargetFolderPath()
        {
            string imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string targetFolderPath = System.IO.Path.Combine(imagePath, "API", "Data", "images");
            return targetFolderPath;
        }

        public byte[] buildPdf(List<Module> modules)
        {
            string rootFolderPNG = GetTargetFolderPath(); //image sauce

            using (MemoryStream stream = new MemoryStream())
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
                    }
                    else if (module.Type == "panel")
                    {
                        CreatePanelTable(module, document);
                    }
                    else
                    {
                        string key = $"{module.Aggregate}_{module.Type}";
                        int fileCounter = fileCounters.ContainsKey(key) ? fileCounters[key] : 0;

                        string imagePath = System.IO.Path.Combine(rootFolderPNG, $"{module.Aggregate}_{module.Type}_chart{fileCounter}.png");

                        if (File.Exists(imagePath))
                        {
                            ImageData imageData = ImageDataFactory.Create(imagePath);
                            images.Add(new Tuple<ImageData, float, float>(imageData, module.Left, module.Top));
                            fileCounter++;
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
                int lastPageNumber = pdfDocument.GetNumberOfPages();
                foreach (var image in images)
                {
                    if (lastPageNumber == 0)
                    {
                        Image pdfImage = new(image.Item1);
                        pdfImage.SetFixedPosition(image.Item2, pdfDocument.GetPage(1).GetPageSize().GetHeight() - image.Item3 - pdfImage.GetImageHeight());
                        document.Add(pdfImage);
                    }
                    else
                    {
                        Image pdfImage = new(image.Item1);
                        pdfImage.SetFixedPosition(image.Item2, pdfDocument.GetPage(lastPageNumber).GetPageSize().GetHeight() - image.Item3 - pdfImage.GetImageHeight());
                        document.Add(pdfImage);
                    }
                }
                document.Close();
                string[] files = Directory.GetFiles(rootFolderPNG);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                return stream.ToArray();
            }
        }
    }
}