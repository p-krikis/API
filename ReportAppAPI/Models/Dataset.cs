using Newtonsoft.Json.Linq;

namespace ReportAppAPI.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public JToken[]? Data { get; set; }
        public List<ScatterData>? ScatterData { get; set; }
        public JToken? BorderColor { get; set; }
        public JToken? BackgroundColor { get; set;}
        public int ParameterId { get; set; }
    }
}