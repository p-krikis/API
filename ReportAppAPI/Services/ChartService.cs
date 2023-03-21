using ScottPlot;
using ReportAppAPI.Models;

namespace ReportAppAPI.Services
{
    public class ChartService
    {
        public void PlotChart(Module module)
        {
            var plt = new Plot();
            //string chartPath = string.Empty; //for testing
            if (module.Type == "line")
            {
                double[] xAxisData = module.Labels.Select(dateString => DateTime.Parse(dateString).ToOADate()).ToArray();
                foreach (var dataset in module.Datasets)
                {
                    plt.PlotScatter(xAxisData, dataset.Data.ToArray(), markerSize: 4, lineWidth: 1);
                    plt.XAxis.TickLabelFormat("dd/mm/yyyy", dateTimeFormat: true);
                }
                //cartPath = "C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\line_chart.png"; //for testing
                plt.SaveFig("C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\line_chart.png");
            }
            else if (module.Type == "bar")
            {
                double[] xAxisData = GenerateXAxisData(module.Labels.Count);
                foreach (var dataset in module.Datasets)
                {
                    plt.PlotBar(xAxisData, dataset.Data.ToArray());
                }
                //chartPath = "C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\bar_chart.png"; //for testing
                plt.SaveFig("C:\\Users\\praktiki1\\Desktop\\APIdump\\PNGs\\bar_chart.png");
            }
            //return chartPath;
            //plt.SaveFig($"chart_{module.Type}.png");
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
