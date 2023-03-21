namespace ReportAppAPI.Models
{
    public class Module
    {
        public string Type { get; set; }
        public List<string> Labels { get; set; }
        public List<Dataset> Datasets { get; set; }
    }
}
