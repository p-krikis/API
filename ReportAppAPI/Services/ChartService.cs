using ScottPlot;
using ReportAppAPI.Models;
using System.Diagnostics;
using System.Globalization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;
using iText.Layout.Element;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using iText.Layout.Properties;
using System.Linq;

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        public void PlotChart(Models.Module module) //some conflict
        {
            var plt = new Plot();
            if (module.Type == "line")
            {
                PlotLineChart(module, plt);
            }
            else if (module.Type == "bar")
            {
                PlotBarChart(module, plt);
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
            else
            {
                Console.WriteLine($"Unsupported Chart type: {module.Type}");
            }
            plt.SaveFig($"C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\{module.Type}_chart.png");
        }
        private void PlotLineChart(Models.Module module, Plot plt)
        {

            double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            foreach (var dataset in module.Datasets)
            {
                var colorLine = GetColorFromJToken(dataset.BorderColor);
                plt.Title(chartTitle);
                plt.AddScatter(xAxisData, dataset.Data.Select(x => x.Value<double>()).ToArray(), markerSize: 5, lineWidth: 1, label: dataset.Label, color: colorLine);
                plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                plt.Legend(location: Alignment.LowerLeft);
                plt.XAxis.TickLabelStyle(rotation: 45);
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(dataset.Data[i].ToString(), x: xAxisData[i] - 0.3, y: ((double)dataset.Data[i]) - 0.4, color: System.Drawing.Color.Black, size: 9);
                }
            }
        }
        private void PlotBarChart(Models.Module module, Plot plt)
        {
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            string[] labels = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture)).Select(date => date.ToString("dd/MM/yyyy")).ToArray();
            foreach (var dataset in module.Datasets)
            {
                double[] values = dataset.Data.Select(x => x.Value<double>()).ToArray();
                System.Drawing.Color backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                var bar = plt.AddBar(values);

                bar.FillColor = backgroundColor;
                bar.ShowValuesAboveBars = true;
            }
            plt.Title(chartTitle);
            plt.XTicks(labels);
            plt.SetAxisLimits(yMin: 0);
        }
        private void PlotPieChart(Models.Module module, Plot plt)
        {
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            string[] labels = module.Labels.ToArray();
            var pie = plt.AddPie(values);
            plt.Title(chartTitle);
            pie.ShowValues = true;
            pie.SliceLabels = labels;
            pie.Explode = true;
            plt.Legend();
            int sliceCount = values.Length;
            System.Drawing.Color[] backgroundColors = new System.Drawing.Color[sliceCount];
            for (int i = 0; i < sliceCount; i++)
            {
                backgroundColors[i] = GetColorFromJToken(module.Datasets[0].BackgroundColor[i]);
            }
            pie.SliceFillColors = backgroundColors;
        }
        private void PlotScatterChart(Models.Module module, Plot plt)
        {
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            plt.Title(chartTitle);
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
                    plt.Legend(location: Alignment.LowerLeft);
                    plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    var formattedLabels = module.Labels.Select(dateStr =>
                    {
                        DateTime date = DateTime.ParseExact(dateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                        return date.ToString("dd/MM/yyyy");
                    }).ToArray();
                    plt.XTicks(xValues, formattedLabels);
                    plt.XAxis.TickLabelStyle(rotation: 45);
                }
            }
        }
        private void ExtractScatterData(Models.Module module)
        {
            foreach (var dataset in module.Datasets)
            {
                if (dataset.Data != null && dataset.Data.Length > 0 && dataset.Data[0].Type == JTokenType.Object)
                {
                    dataset.ScatterData = dataset.Data.Select(d => d.ToObject<ScatterData>()).ToList();
                }
            }
        }
        private void CreateTable(Models.Module module, Document document)
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
        public void buildPdf(List<Models.Module> modules)
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