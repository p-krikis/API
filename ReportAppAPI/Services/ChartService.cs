using ScottPlot;
using ReportAppAPI.Models;
using System.Diagnostics;
using System.Globalization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;
using iText.Kernel.Geom;

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        private Document _pdfDocument;
        public void PlotChart(Module module)
        {
            try
            {
                var plt = new Plot();
                if (module.Type == "line")
                {
                    double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
                    foreach (var dataset in module.Datasets)
                    {
                        //System.Drawing.Color lineColor = dataset.BorderColor; //ColorTranslator.FromHtml(dataset.BorderColor);
                        plt.Title("Weather Report Line Chart");
                        plt.XLabel("Date (DD/MM/YYYY)");
                        plt.YLabel("Temp/Humidity/Breathability/IAQ");
                        plt.PlotScatter(xAxisData, dataset.Data.ToArray(), markerSize: 5, lineWidth: 1);
                        plt.PlotText(text: dataset.Label, x: xAxisData[0] + 0.3, y: dataset.Data[0] - 0.7, /*color: lineColor,*/ alignment: Alignment.MiddleCenter, fontSize: 10, bold: true);
                        plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                        for (int i = 0; i < xAxisData.Length; i++)
                        {
                            plt.PlotText(text: dataset.Data[i].ToString(), x: xAxisData[i], y: dataset.Data[i] + 0.4, alignment: Alignment.MiddleCenter, fontSize: 10, bold: true, color: System.Drawing.Color.Black);
                        }
                    }
                }
                else if (module.Type == "bar")
                {
                    double[] xAxisData = GenerateXAxisData(module.Labels);
                    foreach (var dataset in module.Datasets)
                    {
                        plt.PlotBar(xAxisData, dataset.Data.ToArray(), barWidth: 0.5, showValues: true);
                        plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    }
                }
                else if (module.Type == "pie")
                {
                    double[] values = module.Datasets[0].Data.ToArray();
                    string[] labels = module.Labels.ToArray();
                    var pie = plt.AddPie(values);
                    pie.Explode = true;
                    pie.ShowValues = true;
                    pie.SliceLabels = labels;
                    plt.Legend();
                }
                else if (module.Type == "polarArea") // currently no use, coxcomb_chart
                {
                    double[] values = module.Datasets[0].Data.ToArray();
                    string[] labels = module.Labels.ToArray();
                    var polarArea = plt.AddCoxcomb(values);
                    polarArea.FillColors = plt.Palette.GetColors(5, 0, 0.5);
                    polarArea.SliceLabels = labels;
                }
                //else if (module.Type == "scatter") // currently no use
                //{

                //}
                else
                {
                    Debug.WriteLine($"Unsupported Chart type: {module.Type}");
                }
                plt.SaveFig($"C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\{module.Type}_chart.png");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error plotting chart: {ex.Message}");
            }
        }
        private double[] GenerateXAxisData(List<string> labels)
        {
            double[] xAxisData = labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            return xAxisData;
        }
    }
}
