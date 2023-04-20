using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReportAppAPI.Models;
using ScottPlot;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace ReportAppAPI.Services
{
    public class EmailService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        //private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

        public async Task<string> PostCreds()
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
        public async Task<List<string>> GetDeviceList()
        {
            List<string> deviceIdList = new List<string>();
            List<int> siteIdList = new List<int>();
            var authToken = await PostCreds();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var deviceListResponse = await _httpClient.GetAsync("https://api.dei.prismasense.com/energy/v1/devices");
            if (deviceListResponse.IsSuccessStatusCode)
            {
                var deviceListResponseContent = await deviceListResponse.Content.ReadAsStringAsync();
                dynamic deviceList = JsonConvert.DeserializeObject<List<DeviceListResponse>>(deviceListResponseContent);
                foreach (var device in deviceList)
                {
                    siteIdList.Add(device.SiteId);
                    deviceIdList.Add(device.DeviceId);
                }

                return deviceIdList;
            }
            else
            {
                Console.WriteLine("Failed with status code: {0}", deviceListResponse.StatusCode);
                return null;
            }
        }
        public async Task<List<string>> GetParamsByDevice()
        {
            List<string> paramsList = new List<string>();
            List<string> deviceIdList = await GetDeviceList();
            var authToken = await PostCreds();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var paramsListResponse = await _httpClient.GetAsync($"https://api.dei.prismasense.com/energy/v1/parameters/device/{deviceIdList.FirstOrDefault()}");
            if (paramsListResponse.IsSuccessStatusCode)
            {
                var paramsListResponseContent = await paramsListResponse.Content.ReadAsStringAsync();
                dynamic paramsListResponseContentJson = JsonConvert.DeserializeObject<List<ParametersResponse>>(paramsListResponseContent);
                foreach (var param in paramsListResponseContentJson)
                {
                    var paramsInfo = string.Format("{0}, {1}", param.ParameterId, param.Name);
                    paramsList.Add(paramsInfo);
                }
                return paramsList;
            }
            else
            {
                return null;
            }
        }
        public async Task<(List<DateTime> dateTimes, List<double> actualValues)> PostParamValues()
        {
            var authToken = await PostCreds();
            var startDate = DateTime.UtcNow.AddDays(-7).ToString("O");
            var endDate = DateTime.UtcNow.ToString("O");
            var resolution = 360; //to be replaced by method for api endpoint
            List<double> actualValues = new List<double>();
            List<DateTime> dateTimes = new List<DateTime>();
            var url = "https://api.dei.prismasense.com/energy/v1/parameters/10/1641/values/";
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







        public static DirectoryInfo CreateDirectory(string dirPathWeeklyPNG)
        {
            if (!Directory.Exists(dirPathWeeklyPNG))
            {
                Directory.CreateDirectory(dirPathWeeklyPNG);
            }
            return new DirectoryInfo(dirPathWeeklyPNG);
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
            double[] xAxisData = module.Labels.Select(dateString => DateTime.ParseExact(dateString, "dd/MM/yyyy, HH:mm", CultureInfo.InvariantCulture).ToOADate()).ToArray();
            string[] labels = module.Labels.ToArray();
            double[] values = module.Datasets[0].Data.Select(x => x.Value<double>()).ToArray();

            string chartTitle = GetChartTitle(module);
            Console.Write(xAxisData);
            foreach (var dataset in module.Datasets)
            {
                var colorLine = GetColorFromJToken(dataset.BorderColor);
                var backgroundColor = GetColorFromJToken(dataset.BackgroundColor);
                plt.AddScatter(xAxisData, dataset.Data.Select(x => x.Value<double>()).ToArray(), markerSize: 5, lineWidth: 1, label: dataset.Label, color: colorLine);
                for (int i = 0; i < xAxisData.Length; i++)
                {
                    plt.AddText(dataset.Data[i].ToString(), x: xAxisData[i] - 0.3, y: ((double)dataset.Data[i]) - 0.4, color: System.Drawing.Color.Black, size: 9);
                }
            }
            plt.Title(chartTitle, size: 11);
            plt.XTicks(xAxisData, labels);
            var legend = plt.Legend(location: Alignment.UpperRight);
            legend.Orientation = Orientation.Horizontal;
            legend.FontSize = 9;
            plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 10);
        }
        private void PlotBarChart(Module module, Plot plt)
        {
            string chartTitle = GetChartTitle(module);
            string[] labels = module.Labels.ToArray();
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
        }
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
            string chartTitle = GetChartTitle(module);
            string[] labels = module.Labels.ToArray();
            plt.Title(chartTitle, size: 11);
            foreach (var dataset in module.Datasets)
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
                plt.XTicks(xValues, labels);
                plt.XAxis.TickLabelStyle(rotation: 45, fontSize: 9);
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
//    //timer every 5 hours executes, to be switched by scheduler
//    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
//    //private readonly PeriodicTimer tokenRefreshTimer = new(TimeSpan.FromHours(7.5));
//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
//        {
//            var authToken = await PostCreds();
//            if (authToken != null)
//            {
//                await GetJsonFile(authToken);
//                authToken = null;
//            }
//        }
//    }
//    static async Task GetJsonFile(string authToken)
//    {
//        var httpClient = new HttpClient();
//        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
//        var response = await httpClient.GetAsync("https://api.dei.prismasense.com/energy/v1/devices");
//
//        if (response.IsSuccessStatusCode)
//        {
//            var content = await response.Content.ReadAsStringAsync();
//            var json = JsonConvert.DeserializeObject(content);
//            var filePath = Path.Combine("C:\\Users\\praktiki1\\Desktop\\JSONtest", "data.json");
//
//            using (var file = File.CreateText(filePath))
//            {
//                var serializer = new JsonSerializer();
//                serializer.Serialize(file, json);
//            }
//        }
//        else
//        {
//            Console.WriteLine($"Error: {response.StatusCode}");
//        }
//
//    }
//}