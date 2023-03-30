using ScottPlot;
using ReportAppAPI.Models;
using System.Globalization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;
using iText.Layout.Element;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using iText.Layout.Properties;

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        public void PlotChart(Module module) //some conflict
        {
            var plt = new Plot(module.Width, module.Height);
            if (module.Type == "line")
            {
                PlotLineChart(module, plt);
            }
            else if (module.Type == "bar")
            {
                if (string.IsNullOrEmpty(module.Aggregate))
                {
                    PlotBarChart(module, plt);
                }
                else
                {
                    PlotAggregatedBarChart(module, plt);
                }
            }
            else if (module.Type == "pie")
            {
                PlotPieChart(module, plt);
            }
            else if (module.Type == "scatter")
            {
                ExtractScatterData(module);
                PlotScatterChart(module, plt);
            }
            else if (module.Type == "table")
            {
                return;
            }
            else if (module.Type == "panel")
            {
                return;
            }
            else
            {
                Console.WriteLine($"Unsupported Chart type: {module.Type}");
            }
            plt.SaveFig($"C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\{module.Aggregate}_{module.Type}_chart.png");
        }
        private void PlotLineChart(Module module, Plot plt)
        {

            double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            string chartTitle = string.Format("{2}, {0}, {1}", module.Device.Name, module.Device.DeviceId, module.Aggregate);
            foreach (var dataset in module.Datasets)
            {
                var colorLine = GetColorFromJToken(dataset.BorderColor);
                var backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                plt.Title(chartTitle, size: 11);
                plt.AddScatter(xAxisData, dataset.Data.Select(x => x.Value<double>()).ToArray(), markerSize: 5, lineWidth: 1, label: dataset.Label, color: colorLine);
                plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                var legend = plt.Legend(location: Alignment.UpperRight);
                legend.Orientation = Orientation.Horizontal;
                legend.FontSize = 9;
                plt.XAxis.TickLabelStyle(rotation: 45, fontSize:10);
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(dataset.Data[i].ToString(), x: xAxisData[i] - 0.3, y: ((double)dataset.Data[i]) - 0.4, color: System.Drawing.Color.Black, size: 9);
                }
            }
        }
        private void PlotBarChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{2}, {0}, {1}", module.Device.Name, module.Device.DeviceId, module.Aggregate);
            string[] labels = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture)).Select(date => date.ToString("dd/MM/yyyy")).ToArray();
            foreach (var dataset in module.Datasets)
            {
                double[] values = dataset.Data.Select(x => x.Value<double>()).ToArray();
                System.Drawing.Color backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                var bar = plt.AddBar(values);
                bar.Font.Size = 9;
                bar.FillColor = backgroundColor;
                bar.ShowValuesAboveBars = true;
            }
            plt.Title(chartTitle, size: 11);
            plt.XTicks(labels);
            plt.SetAxisLimits(yMin: 0);
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
        }
        private void PlotPieChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{2}, {0}, {1}", module.Device.Name, module.Device.DeviceId, module.Aggregate);
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            string[] labels = module.Labels.ToArray();
            var pie = plt.AddPie(values);
            plt.Title(chartTitle, size: 11);
            pie.ShowValues = true;
            pie.SliceLabels = labels;
            pie.Explode = true;
            var legend = plt.Legend(true, Alignment.LowerCenter);
            legend.Orientation = Orientation.Horizontal;
            legend.FontSize = 9;
            int sliceCount = values.Length;
            System.Drawing.Color[] backgroundColors = new System.Drawing.Color[sliceCount];
            for (int i = 0; i < sliceCount; i++)
            {
                backgroundColors[i] = GetColorFromJToken(module.Datasets[0].BackgroundColor[i]);
            }
            pie.SliceFillColors = backgroundColors;
        }
        private void PlotAggregatedBarChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{2}, {0}, {1}", module.Device.Name, module.Device.DeviceId, module.Aggregate);
            string[] labels = module.Labels.ToArray();
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            System.Drawing.Color backgroundColor = GetColorFromJToken(module.Datasets[0].BackgroundColor);
            var bar = plt.AddBar(values);
            plt.Title(chartTitle, size:11);
            plt.XTicks(labels);
            bar.ShowValuesAboveBars = true;
            bar.FillColor = backgroundColor;
            bar.Font.Size = 9;
            plt.SetAxisLimits(yMin: 0);
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
        }
        private void PlotScatterChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{2}, {0}, {1}", module.Device.Name, module.Device.DeviceId, module.Aggregate);
            plt.Title(chartTitle, size: 11);
            foreach (var dataset in module.Datasets)
            {
                if (dataset.ScatterData != null) //may not be needed
                {
                    var color = GetColorFromJToken(dataset.BorderColor);
                    double[] xValues = dataset.ScatterData.Select(scatterData => scatterData.X.Value).ToArray();
                    double[] yValues = dataset.ScatterData.Select(scatterData => scatterData.Y.Value).ToArray();
                    plt.AddScatter(xValues, yValues, markerSize: 5, lineWidth: 0, label: dataset.Label, color: color);
                    for (int i = 0; i < xValues.Length; i++)
                    {
                        plt.AddText(yValues[i].ToString(), x: xValues[i] - 0.5, y: yValues[i] - 0.5, color: System.Drawing.Color.Black, size: 9);
                    }
                    var legend = plt.Legend(location: Alignment.UpperRight);
                    legend.Orientation = Orientation.Horizontal;
                    legend.FontSize = 9;
                    plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    var formattedLabels = module.Labels.Select(dateStr =>
                    {
                        DateTime date = DateTime.ParseExact(dateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                        return date.ToString("dd/MM/yyyy");
                    }).ToArray();
                    plt.XTicks(xValues, formattedLabels);
                    plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
                }
            }
        }
        private void ExtractScatterData(Module module)
        {
            foreach (var dataset in module.Datasets)
            {
                if (dataset.Data != null && dataset.Data.Length > 0 && dataset.Data[0].Type == JTokenType.Object)
                {
                    dataset.ScatterData = dataset.Data.Select(d => d.ToObject<ScatterData>()).ToList();
                }
            }
        }
        private void CreateTable(Module module, Document document)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Labels");
            foreach(var dataset in module.Datasets)
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
        private System.Drawing.Color GetColorFromJToken(JToken colorToken)
        {
            if (colorToken == null)
            {
                return System.Drawing.Color.Black;
            }
            else if (colorToken.Type == JTokenType.String)
            {
                return ParseRgbaColor(colorToken.Value<string>());
            }
            else if (colorToken.Type == JTokenType.Array)
            {
                List<System.Drawing.Color> colors = colorToken.Values<string>().Select(colorStr => ParseRgbaColor(colorStr)).ToList();
                return colors.FirstOrDefault();
            }
            else
            {
                return System.Drawing.Color.Black;
            }
        }
        private System.Drawing.Color ParseRgbaColor(string rgbaString)
        {
            var match = Regex.Match(rgbaString, @"rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d\.]+)\)");
            if (match.Success)
            {
                int r = int.Parse(match.Groups[1].Value);
                int g = int.Parse(match.Groups[2].Value);
                int b = int.Parse(match.Groups[3].Value);
                float a = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                return System.Drawing.Color.FromArgb((int)(a * 255), r, g, b);
            }
            else 
            { 
                return System.Drawing.Color.Black;
            }
        }
    }
}

//a4 page size template?

//public void buildPdf(List<Models.Module> modules)
//{
//    string pngFolderPath = @"C:\Users\praktiki1\Desktop\APIdump\PNGs"; //image sauce
//    string pdfPath = @"C:\Users\praktiki1\Desktop\APIdump\PDFs\report.pdf"; //pdf dump loc
//    using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
//    {
//        PdfWriter writer = new PdfWriter(stream);
//        PdfDocument pdfDocument = new PdfDocument(writer);
//        Document document = new Document(pdfDocument, PageSize.A4); // Set the PageSize to A4
//        foreach (var module in modules)
//        {
//            if (module.Type == "table")
//            {
//                CreateTable(module, document);
//                document.Add(new AreaBreak());
//            }
//        }
//        DirectoryInfo directoryInfo = new DirectoryInfo(pngFolderPath);
//        FileInfo[] graphImages = directoryInfo.GetFiles("*.png");
//        int imageCounter = 0;
//        foreach (FileInfo graphImage in graphImages)
//        {
//            ImageData imageData = ImageDataFactory.Create(graphImage.FullName);
//            Image pdfImage = new Image(imageData);
//            pdfImage.SetAutoScale(false);
//            document.Add(pdfImage);
//            imageCounter++;
//            if (imageCounter % 2 == 0) //2 images per page
//            {
//                document.Add(new AreaBreak());
//            }

//        }
//        document.Close();
//    }
//}