namespace ReportAppAPI.Models
{
    public class AutoReport
    {
        public int ReportId { get; set; }
        public string Email { get; set; }
        public int DataFrequency { get; set; }
        public int ReportFrequency { get; set; }
    }
}
