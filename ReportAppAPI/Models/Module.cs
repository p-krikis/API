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
/*
 * 
public class Rootobject
{
public Class1[] Property1 { get; set; }
}

public class Class1
{
public string title { get; set; }
public string header { get; set; }
public int left { get; set; }
public int top { get; set; }
public int width { get; set; }
public int height { get; set; }
public string[] labels { get; set; }
public Dataset[] datasets { get; set; }
public string type { get; set; }
public string from { get; set; }
public Device device { get; set; }
}

public class Device
{
public string name { get; set; }
public int site { get; set; }
public string deviceId { get; set; }
}

public class Dataset
{
public string label { get; set; }
public object[] data { get; set; }
public object backgroundColor { get; set; }
public string[] hoverBackgroundColor { get; set; }
public object borderColor { get; set; }
public int borderWidth { get; set; }
public string[] hoverBorderColor { get; set; }
public bool fill { get; set; }
public string[] pointBackgroundColor { get; set; }
public string[] pointBorderColor { get; set; }
public int pointBorderWidth { get; set; }
}

*/