namespace ReportAppAPI.Models
{
    public class Module
    {
        public string Type { get; set; }
        public string[]? Labels { get; set; }
        public Dataset[] Datasets { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Device Device { get; set; }
        public string? Aggregate { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
    }
}