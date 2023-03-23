using ScottPlot;
using ReportAppAPI.Models;
using System.Diagnostics;
using System.Globalization;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;
using iText.Kernel.Geom;
using System.Reflection.Emit;
using iText.Layout.Element;
using iText.Kernel.Colors;
using System.Data;

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
                    double[] values = module.Datasets[0].Data.ToArray();
                    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId); //takes first name it finds only, not dynamic per module, to fix
                    foreach (var dataset in module.Datasets)
                    {
                        plt.Title(chartTitle);
                        plt.AddScatter(xAxisData, dataset.Data.ToArray(), markerSize: 5, lineWidth: 1, label: dataset.Label);
                        plt.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                        plt.Legend(location: Alignment.LowerLeft);
                        for (int i = 0; i < xAxisData.Length; i++)
                        {
                            plt.AddText(dataset.Data[i].ToString(), x: xAxisData[i], y: dataset.Data[i] - 0.4, color: System.Drawing.Color.Black);
                        }
                    }
                }
                else if (module.Type == "bar")
                {
                    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
                    string[] labels = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "MM/dd/yyyy", CultureInfo.InvariantCulture)).Select(date => date.ToString("dd/MM/yyyy")).ToArray();
                    double[] values = module.Datasets[0].Data.ToArray();
                    //System.Drawing.Color[] color = module.Datasets[0].BackgroundColor;
                    var bar = plt.AddBar(values);
                    plt.Title(chartTitle);
                    plt.XTicks(labels);
                    bar.ShowValuesAboveBars = true;
                    //bar.FillColor = color[3];
                    plt.SetAxisLimits(yMin: 0);
                }
                else if (module.Type == "pie")
                {
                    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
                    double[] values = module.Datasets[0].Data.ToArray();
                    string[] labels = module.Labels.ToArray();
                    var pie = plt.AddPie(values);
                    plt.Title(chartTitle);
                    pie.ShowValues = true;
                    pie.SliceLabels = labels;
                    pie.OutlineSize = 1;
                    plt.Legend();
                }
                else if (module.Type == "polarArea") // currently no use, coxcomb_chart
                {
                    string chartTitle = string.Format("{0}, {1}", module.Device.Name, module.Device.DeviceId);
                    double[] values = module.Datasets[0].Data.ToArray();
                    string[] labels = module.Labels.ToArray();
                    var polarArea = plt.AddCoxcomb(values);
                    plt.Title(chartTitle);
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
                    Image pdfImage = new Image(imageData);
                    pdfImage.SetAutoScale(true);
                    //pdfImage.SetMargins(5,5,5,5);
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
    }
}
