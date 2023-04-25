namespace ReportAppAPI.Models
{
    public class ParametersResponse
    {
        public int[]? ParameterId { get; set; }
        public int? SiteId { get; set; }
        public string? Name { get; set; }
    }
    public class DeviceListResponse
    {
        public string? DeviceId { get; set; }
        public string? Name { get; set; }
        public int? SiteId { get; set; }
    }
    public class ParametersValueResponse
    {
        public int? ParameterValid { get; set; }
        public int? ParameterIdValue { get; set; }
        public double? Value { get; set; }
        public DateTime? Datetime { get; set; }
    }
    public class TokenResponse
    {
        public string Access_token { get; set; }
    }
}
