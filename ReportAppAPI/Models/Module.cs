using Newtonsoft.Json.Linq;

namespace ReportAppAPI.Models
{
    public class Module
    {
        public string Type { get; set; }
        public string[] Labels { get; set; }
        public Dataset[] Datasets { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Device Device { get; set; }
    }
}



//public class Rootobject
//{
//    public Class1[] Property1 { get; set; }
//}

//public class Class1
//{
//    public string title { get; set; }
//    public string header { get; set; }
//    public int left { get; set; }
//    public int top { get; set; }
//    public int width { get; set; }
//    public int height { get; set; }
//    public object labels { get; set; }
//    public object datasets { get; set; }
//    public string type { get; set; }
//    public string from { get; set; }
//    public Device device { get; set; }
//}

//public class Device
//{
//    public string name { get; set; }
//    public int site { get; set; }
//    public string deviceId { get; set; }
//}
