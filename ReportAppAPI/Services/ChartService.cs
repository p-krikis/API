using System.Drawing;
using ScottPlot;
using ReportAppAPI.Models;
using System.Diagnostics;
using System.Globalization;

namespace ReportAppAPI.Services
{
    public class ChartService
    {
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
                        plt.PlotScatter(xAxisData, dataset.Data.ToArray(), markerSize: 5, lineWidth: 1);
                        plt.PlotText(text: dataset.Label, x: xAxisData[0] + 0.3, y: dataset.Data[0] - 0.7, /*color: lineColor,*/ alignment: Alignment.MiddleCenter, fontSize: 10, bold: true);
                        plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                        for (int i = 0; i < xAxisData.Length; i++)
                        {
                            plt.PlotText(text: dataset.Data[i].ToString(), x: xAxisData[i], y: dataset.Data[i] + 0.4, /*color: lineColor,*/ alignment: Alignment.MiddleCenter, fontSize: 10, bold: true);
                        }
                    }
                    plt.SaveFig("C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\line_chart.png");
                }
                else if (module.Type == "bar")
                {
                    double[] xAxisData = GenerateXAxisData(module.Labels.Count);
                    foreach (var dataset in module.Datasets)
                    {
                        plt.PlotBar(xAxisData, dataset.Data.ToArray());
                        plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    }
                    plt.SaveFig("C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\bar_chart.png");
                }
                else if (module.Type == "pie")
                {
                    double[] values = module.Datasets[0].Data.ToArray();
                    string[] labels = module.Labels.ToArray();
                    plt.PlotPie(values, labels);
                    plt.SaveFig("C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\pie_chart.png");
                }
                else
                {
                    Debug.WriteLine($"Unsupported Chart type: {module.Type}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error plotting chart: {ex.Message}");
            }
        }
        private double[] GenerateXAxisData(int count)
        {
            double[] xAxisData = new double[count];
            for (int i= 0; i < count; i++)
            {
                xAxisData[i] = i + 1;
            }
            return xAxisData;
        }
    }
}
