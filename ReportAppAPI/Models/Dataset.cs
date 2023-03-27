using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportAppAPI.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public List<double> Data { get; set; }
        public List<ScatterData>? ScatterData { get; set; }
        public JToken? BorderColor { get; set; }
        public JToken BackgroundColor { get; set;}
    }
}
//color string needs to be #RRGGBB or #AARRGGBB