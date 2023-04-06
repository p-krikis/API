namespace ReportAppAPI.Models
{
    public class Module
    {
        public string Title { get; set; }
        public string Header { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public string[]? Labels { get; set; }
        public Dataset[]? Datasets { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public float ParentWidth { get; set; }
        public float ParentHeight { get; set; }
        public Device? Device { get; set; }
        public string? Aggregate { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
    }
}