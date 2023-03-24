using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ReportAppAPI.Models
{
    public class Dataset
    {
        public string Label { get; set; }
        public List<double> Data { get; set; }
        //public object[] Data { get; set; }
        private object _backgroundColor;
        public object BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (value is string)
                {
                    _backgroundColor = value;
                }
                else if (value is List<string>)
                {
                    _backgroundColor = value;
                }
            }
        }
        [JsonProperty("borderColor")]
        public string BorderColor { get; set; }
    }
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