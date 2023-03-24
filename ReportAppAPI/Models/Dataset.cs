using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportAppAPI.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public List<double> Data { get; set; }
    }
    //public class ScatterData
    //{
    //    public List<double> XValues { get; set; }
    //    public List<double> YValues { get; set; }
    //    public List<string> Date { get; set; }
    //}
}
//[JsonProperty("backgroundColor", Required = Required.Default)]
//public JToken BackgroundColorToken { get; set; }
//[JsonIgnore]
//public System.Drawing.Color[] BackgroundColor
//{
//    get
//    {
//        if (BackgroundColorToken == null)
//            return null;

//        if (BackgroundColorToken.Type == JTokenType.Array)
//            return BackgroundColorToken.ToObject<System.Drawing.Color[]>();

//        if (BackgroundColorToken.Type == JTokenType.String)
//            return new[] { System.Drawing.ColorTranslator.FromHtml(BackgroundColorToken.ToObject<string>()) };

//        return null;
//    }
//}