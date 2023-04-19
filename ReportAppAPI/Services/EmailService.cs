using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using ReportAppAPI.Models;
using System.Net.Http.Headers;

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
                dynamic tokenResponse = JsonConvert.SerializeObject(loginResponseContent.ToString());
                string authToken = tokenResponse.Substring(21, 1018);
                return authToken;
            }
            else
            {
                Console.WriteLine("Failed with status code: {0}", loginResponse.StatusCode);
                return null;
            }
        }
    }
}
//public class BackgroundWorkerService : BackgroundService
//{
//    public async Task<string> PostCreds()
//    {
//        var httpClient = new HttpClient();
//        var reqRequest = new HttpRequestMessage
//        {
//            Method = HttpMethod.Post,
//            RequestUri = new Uri("https://auth.iot.prismasense.com/connect/token"),
//            Content = new FormUrlEncodedContent(new Dictionary<string, string>
//            {
//                { "client_id", "admin.client" },
//                { "client_secret", "hp7OAVlbSVGo8pHUK52m" },
//                { "grant_type", "password" },
//                { "username", "aqs_mobile" },
//                { "password", "Password123!" }
//            })
//        };
//        var reqResponse = await httpClient.SendAsync(reqRequest);
//        if (reqResponse.IsSuccessStatusCode)
//        {
//            var reqResponseContent = await reqResponse.Content.ReadAsStringAsync();
//            dynamic tokenResponse = JsonConvert.SerializeObject(reqResponseContent.ToString());
//            string authToken = tokenResponse.Substring(21, 1018);
//            return authToken;
//        }
//
//        else
//        {
//            Console.WriteLine("Failed with status code: {0}", reqResponse.StatusCode);
//            return null;
//        }
//
//    } 
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