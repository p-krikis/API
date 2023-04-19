namespace ReportAppAPI.Models
{
    public class ParametersResponse
    {
        public int? parameterId { get; set; }
        public int? siteId { get; set; }
        public string? name { get; set; }
    }
    public class DeviceListResponse
    {
        public string? deviceId { get; set; }
        public string? name { get; set; }
        public int? siteId { get; set; }
    }
    public class ParametersValueResponse
    {
        public int? parameterValId { get; set; }
        public int? parameterId { get; set; }
        public object? Value { get; set; }
        public DateTime? Datetime { get; set; }
    }
    public class TokenResponse
    {
        public string Access_token { get; set; }
    }
}
