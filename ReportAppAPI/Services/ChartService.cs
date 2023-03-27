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

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        public void PlotChart(Module module)
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
                PlotScatterChart(module, plt);
            }
            else
            {
                Debug.WriteLine($"Unsupported Chart type: {module.Type}");
            }
            plt.SaveFig($"C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\{module.Type}_chart.png");
        }

        private void PlotLineChart(Module module, Plot plt)
        {
            double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            double[] values = module.Datasets[0].Data.ToArray();
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId); //takes first name it finds only, not dynamic per module, to fix
            foreach (var dataset in module.Datasets)
            {
                var colorLine = GetColorFromJToken(dataset.BorderColor);
                plt.Title(chartTitle);
                plt.AddScatter(xAxisData, dataset.Data.ToArray(), markerSize: 5, lineWidth: 1, label: dataset.Label, color: colorLine);
                plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                plt.Legend(location: Alignment.LowerLeft);
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(dataset.Data[i].ToString(), x: xAxisData[i] - 0.8, y: dataset.Data[i] - 0.4, color: System.Drawing.Color.Black);
                }
            }
        }

        private void PlotBarChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            string[] labels = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture)).Select(date => date.ToString("dd/MM/yyyy")).ToArray();
            double[] values = module.Datasets[0].Data.ToArray();
            System.Drawing.Color backgroundColor = GetColorFromJToken(module.Datasets[0].BackgroundColor);
            var bar = plt.AddBar(values);
            plt.Title(chartTitle);
            plt.XTicks(labels);
            bar.ShowValuesAboveBars = true;
            bar.FillColor = backgroundColor;
            plt.SetAxisLimits(yMin: 0);
        }

        private void PlotPieChart(Module module, Plot plt)
        {
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            double[] values = module.Datasets[0].Data.ToArray();
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

        private void PlotScatterChart(Module module, Plot plt)
        {
            double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
            plt.Title(chartTitle);

            foreach (var dataset in module.Datasets)
            {
                if (dataset.ScatterData != null)
                {
                    var scatterData = dataset.ScatterData
                        .Where(point => point.Y.HasValue)
                        .Select(point => point.Y.Value)
                        .ToArray();
                    var colorLine = GetColorFromJToken(dataset.BorderColor);
                    plt.AddScatter(xAxisData, scatterData, markerSize: 5, lineWidth: 0, label: dataset.Label, color: colorLine);
                    plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    plt.Legend(location: Alignment.LowerLeft);
                }
            }
        }
        public void buildPdf()
        {
            string pngFolderPath = @"C:\Users\praktiki1\Desktop\APIdump\PNGs";
            string pdfPath = @"C:\Users\praktiki1\Desktop\APIdump\PDFs\report.pdf";
            using (FileStream stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                Document document = new Document(pdfDocument);
                DirectoryInfo directoryInfo = new DirectoryInfo(pngFolderPath);
                FileInfo[] graphImages = directoryInfo.GetFiles("*.png");
                int imageCounter = 0;
                foreach (FileInfo graphImage in graphImages)
                {
                    ImageData imageData = ImageDataFactory.Create(graphImage.FullName);
                    iText.Layout.Element.Image pdfImage = new iText.Layout.Element.Image(imageData);
                    pdfImage.SetAutoScale(true);
                    document.Add(pdfImage);
                    imageCounter++;
                    if (imageCounter %2 == 0)
                    {
                        document.Add(new AreaBreak());
                    }
                }
                document.Close();
            }
        }
        private System.Drawing.Color GetColorFromJToken(JToken colorToken)
        {
            if (colorToken.Type == JTokenType.String)
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

//else if (module.Type == "polarArea") // currently no use, coxcomb_chart
//{
//    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
//    double[] values = module.Datasets[0].Data.ToArray();
//    string[] labels = module.Labels.ToArray();
//    var polarArea = plt.AddCoxcomb(values);
//    plt.Title(chartTitle);
//    polarArea.FillColors = plt.Palette.GetColors(5, 0, 0.5);
//    polarArea.SliceLabels = labels;
//}
//else if (module.Type == "scatter") // currently no use
//{
//    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
//    var dataset = module.ScatterData[0];

//    double[] xAxisData = dataset.XValues.ToArray();
//    double[] yAxisData = dataset.YValues.ToArray();
//    string[] labels = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture)).Select(date => date.ToString("dd/MM/yyyy")).ToArray();

//    plt.Title(chartTitle);
//    plt.AddScatter(xAxisData, yAxisData, markerSize: 5, lineWidth: 0);
//    plt.XTicks(xAxisData, labels);
//    plt.Legend(location: Alignment.LowerLeft);
//    for (int i = 0; i < xAxisData.Length; i++)
//    {
//        plt.AddText(yAxisData[i].ToString(), x: xAxisData[i], y: yAxisData[i] - 0.4, color: System.Drawing.Color.Black);
//    }
//}