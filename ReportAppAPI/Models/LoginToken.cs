using Newtonsoft.Json;

namespace ReportAppAPI.Models
{
    public class LoginToken
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
    }
}
