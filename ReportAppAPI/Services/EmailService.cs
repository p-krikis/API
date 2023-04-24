using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using ReportAppAPI.Models;
using ScottPlot;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportAppAPI.Services
{
    public class EmailService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly AutoReport _autoReport = new AutoReport();

        public int GetResolution()
        {
            var res = _autoReport.Resolution;
            return res;
        }
        public int GetReportFrequency()
        {
            var freq = _autoReport.ReportFrequency;
            return freq;
        }
        public async Task<string> PostCredentials()
        {
            var loginRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://auth.iot.prismasense.com/connect/token"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", "admin.client" },
                    { "client_secret", "hp7OAVlbSVGo8pHUK52m" },
                    { "grant_type", "password" },
                    { "username", "aqs_mobile" },
                    { "password", "Password123!" }
                })
            };
            var loginResponse = await _httpClient.SendAsync(loginRequest);
            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                dynamic tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(loginResponseContent);
                string authToken = tokenResponse.Access_token;
                return authToken;
            }
            else
            {
                Console.WriteLine("Failed with status code: {0}", loginResponse.StatusCode);
                return null;
            }
        }
        public async Task<(List<DateTime> dateTimes, List<double> actualValues)> PostParamValues(Module module, int? paramId)
        {
            var authToken = await PostCredentials();
            var startDate = DateTime.UtcNow.AddDays(-GetReportFrequency()).ToString("O");
            var endDate = DateTime.UtcNow.ToString("O");
            var resolution = GetResolution();
            List<double> actualValues = new List<double>();
            List<DateTime> dateTimes = new List<DateTime>();
            var url = $"https://api.dei.prismasense.com/energy/v1/parameters/{module.Device.Site}/{paramId}/values/";
            var payload = new
            {
                from = startDate,
                to = endDate,
                resolution = resolution
            };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var paramValuesResponse = await _httpClient.PostAsync(url, content);
            if (paramValuesResponse.IsSuccessStatusCode)
            {
                var paramValuesResponseContent = await paramValuesResponse.Content.ReadAsStringAsync();
                dynamic paramValuesList = JsonConvert.DeserializeObject<List<ParametersValueResponse>>(paramValuesResponseContent);
                foreach (var paramValue in paramValuesList)
                {
                    if (paramValue.Value != null)
                    {
                        actualValues.Add(paramValue.Value);
                        dateTimes.Add(paramValue.Datetime);
                    }
                }
                return (dateTimes, actualValues);
            }
            else
            {
                return (null, null);
            }
        }

        public void PlotWeeklyChart(Module module)
        {
            int newWidth = (int)Math.Round((double)module.ParentWidth * ((double)module.Width / 100));
            int newHeight = (int)Math.Round((double)module.ParentHeight * ((double)module.Height / 100));
            var plt = new Plot(newWidth, newHeight);
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
            else if (string.IsNullOrEmpty(module.Type))
            {
                return;
            }
            else
            {
                Console.WriteLine($"Unsupported Chart type: {module.Type}");
            }

            string imagesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string targetFolderPath = Path.Combine(imagesPath, "API", "Data", "images");
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }
            int fileCounter = 0;
            if (System.IO.File.Exists(Path.Combine(targetFolderPath, $"{module.Aggregate}_{module.Type}_chart{fileCounter}.png")))
            {
                fileCounter++;
            }
            plt.SaveFig(Path.Combine(targetFolderPath, $"{module.Aggregate}_{module.Type}_chart{fileCounter}.png"));
        }
        public string GetChartTitle(Module module)
        {
            if (string.IsNullOrEmpty(module.Aggregate))
            {
                return string.Format("{0}", module.Device.Name, module.Aggregate); //placeholder
            }
            else
            {
                return string.Format("{1}, {0}", module.Device.Name, module.Aggregate);
            }
        }
        private void PlotLineChart(Module module, Plot plt)
        {
            var paramId = module.Datasets[0].ParameterId;
            List<DateTime> dateTimes = PostParamValues(module, paramId).Result.dateTimes;
            string[] dateTimeArray = dateTimes.Select(x => x.ToString("dd/MM/yyyy, HH:mm")).ToArray();
            double[] xAxisData = module.Labels.Take(dateTimeArray.Length).Select(dateString => DateTime.ParseExact(dateString, "dd/MM/yyyy, HH:mm", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            string chartTitle = GetChartTitle(module);
            foreach (var dataset in module.Datasets)
            {
                paramId = dataset.ParameterId;
                var actualValues = PostParamValues(module, paramId ).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                var colorLine = GetColorFromJToken(dataset.BorderColor);
                var backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                plt.AddScatter(xAxisData, actualValuesArray, markerSize: 5, lineWidth: 1, label: dataset.Label, color: colorLine); //changed values to actualValuesArray, using new system
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(actualValuesArray[i].ToString(), x: xAxisData[i] - 0.3, y: ((double)actualValuesArray[i]) - 0.4, color: System.Drawing.Color.Black, size: 9);
                }
            }
            plt.Title(chartTitle, size: 11);
            plt.XTicks(xAxisData, dateTimeArray);
            var legend = plt.Legend(location: Alignment.UpperRight);
            legend.Orientation = Orientation.Horizontal;
            legend.FontSize = 9;
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 10);
        } //seems OK
        private void PlotBarChart(Module module, Plot plt)
        {
            var paramId = module.Datasets[0].ParameterId;
            List<DateTime> dateTimes = PostParamValues(module, paramId).Result.dateTimes;
            string[] dateTimeArray = dateTimes.Select(x => x.ToString("dd/MM/yyyy, HH:mm")).ToArray();
            string chartTitle = GetChartTitle(module);
            foreach (var dataset in module.Datasets)
            {
                var actualValues = PostParamValues(module, paramId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                System.Drawing.Color backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                var bar = plt.AddBar(actualValuesArray);
                bar.Font.Size = 9;
                bar.FillColor = backgroundColor;
                bar.ShowValuesAboveBars = true;
            }
            plt.Title(chartTitle, size: 11);
            plt.XTicks(dateTimeArray);
            plt.SetAxisLimits(yMin: 0);
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
        }
        private void PlotPieChart(Module module, Plot plt)
        {
            string chartTitle = GetChartTitle(module);
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
        } //NOT DONE YET// STUFF TO FIGURE OUT: How to get paramId properly for all parameters
        private void PlotAggregatedBarChart(Module module, Plot plt)
        {
            string chartTitle = GetChartTitle(module);
            string[] labels = module.Labels.ToArray();
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();
            System.Drawing.Color backgroundColor = GetColorFromJToken(module.Datasets[0].BackgroundColor);
            var bar = plt.AddBar(values);
            plt.Title(chartTitle, size: 11);
            plt.XTicks(labels);
            bar.ShowValuesAboveBars = true;
            bar.FillColor = backgroundColor;
            bar.Font.Size = 9;
            plt.SetAxisLimits(yMin: 0);
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
        }
        private void PlotScatterChart(Module module, Plot plt)
        {
            var paramId = module.Datasets[0].ParameterId;
            List<DateTime> datesScatter = PostParamValues(module, paramId).Result.dateTimes;
            string[] dateTimeArray = datesScatter.Select(x => x.ToString("dd/MM/yyyy, HH:mm")).ToArray();
            string chartTitle = GetChartTitle(module);
            plt.Title(chartTitle, size: 11);
            foreach (var dataset in module.Datasets)
            {
                paramId = dataset.ParameterId;
                var actualValues = PostParamValues(module, paramId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                var color = GetColorFromJToken(dataset.BorderColor);
                //double[] xValues = dataset.ScatterData.Take(dateTimeArray.Length).Select(scatterData => scatterData.X.Value).ToArray();
                double[] xAxisData = dateTimeArray.Select(dateString => DateTime.ParseExact(dateString, "dd/MM/yyyy, HH:mm", CultureInfo.InvariantCulture).ToOADate()).ToArray();
                plt.AddScatter(xAxisData, actualValuesArray, markerSize: 5, lineWidth: 0, label: dataset.Label, color: color);
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(actualValuesArray[i].ToString(), x: xAxisData[i] - 0.5, y: actualValuesArray[i] - 0.5, color: System.Drawing.Color.Black, size: 9);
                }
                var legend = plt.Legend(location: Alignment.UpperRight);
                legend.Orientation = Orientation.Horizontal;
                legend.FontSize = 9;
                plt.XTicks(xAxisData, dateTimeArray);
                plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
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
        private double? AggregatedChart(Module module)
        {
            if (module.Aggregate == "sum")
            {
                var actualValues = PostParamValues(module, module.Datasets[0].ParameterId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                double sum = actualValuesArray.Sum();
                return sum;
            }
            else if(module.Aggregate == "avg")
            {
                var actualValues = PostParamValues(module, module.Datasets[0].ParameterId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                double avg = actualValuesArray.Average();
                return avg;
            }
            else if(module.Aggregate == "min")
            {
                var actualValues = PostParamValues(module, module.Datasets[0].ParameterId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                double min = actualValuesArray.Min();
                return min;
            }
            else if(module.Aggregate == "max")
            {
                var actualValues = PostParamValues(module, module.Datasets[0].ParameterId).Result.actualValues;
                double[] actualValuesArray = actualValues.Select(x => (double)x).ToArray();
                double max = actualValuesArray.Max();
                return max;
            }
            else
            {
                return null;
            }
        } //template
    }
}