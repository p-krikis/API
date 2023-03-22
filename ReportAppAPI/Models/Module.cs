namespace ReportAppAPI.Models
{
    public class Module
    {
        public string Type { get; set; }
        public List<string> Labels { get; set; }
        public List<Dataset> Datasets { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
