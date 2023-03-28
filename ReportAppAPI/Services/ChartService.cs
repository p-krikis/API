﻿using ScottPlot;
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

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        public void PlotChart(Models.Module module)
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
            double[] values = module.Datasets[0].Data.ToArray();
            string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
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

        private void PlotBarChart(Models.Module module, Plot plt)
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

        private void PlotPieChart(Models.Module module, Plot plt)
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

        private void PlotScatterChart(Models.Module module, Plot plt)
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
            string pngFolderPath = @"C:\Users\praktiki1\Desktop\APIdump\PNGs";
            string pdfPath = @"C:\Users\praktiki1\Desktop\APIdump\PDFs\report.pdf";
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
                    if (imageCounter % 2 == 0)
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


//public Table CreateTable(Module module)
//{
//    int numColumns = module.Datasets.Length + 1;
//    Table table = new Table(numColumns);
//    table.AddHeaderCell("Date)");
//    foreach (var dataset in module.Datasets)
//    {
//        table.AddHeaderCell(dataset.Label);
//    }
//    for (int i = 0; i < module.Labels.Length; i++)
//    {
//        table.AddCell(module.Labels[i]);
//        foreach (var dataset in module.Datasets)
//        {
//            table.AddCell(dataset.Data[i].ToString());
//        }
//    }
//    return table;
//}


//foreach (var module in modules)
//{
//    if (module.Type == "table" && module.Table != null)
//    {
//        document.Add(module.Table);
//    }
//}