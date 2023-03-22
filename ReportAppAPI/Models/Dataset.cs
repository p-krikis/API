using Newtonsoft.Json;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ReportAppAPI.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public List<double> Data { get; set; }

        //[JsonPropertyName("borderColor")]
       //public System.Drawing.Color BorderColor { get; set; }
    }
}
