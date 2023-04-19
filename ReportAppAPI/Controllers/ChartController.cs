using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReportAppAPI.Models;
using ReportAppAPI.Services;

namespace ReportAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly ChartService _chartService;
        private readonly PDFbuildService _pdfbuildService;
        private readonly JsonDbService _jsonDbService;
        private readonly EmailService _emailService;
        public ChartController(JsonDbService jsonDbService)
        {
            _chartService = new ChartService();
            _pdfbuildService = new PDFbuildService();
            _jsonDbService = jsonDbService;
            _emailService = new EmailService();
        }
        [HttpPost("saveJSON")]
        public async Task<IActionResult> SaveJSON([FromBody] List<Module> modules)
        {
            string jsonString = JsonConvert.SerializeObject(modules);
            string name = "report";
            int id = await _jsonDbService.SaveFileAsync(name, jsonString);
            return Ok(new { Id = id, Name = name });
        }
        [HttpGet("getAllJSON")]
        public async Task<IActionResult> GetAllJSON()
        {
            var files = await _jsonDbService.GetAllJsonFilesAsync();
            return Ok(files);
        }
        [HttpGet("getSingleJSON/{id}")]
        public async Task<IActionResult> GetSingleJSON(int id)
        {
            var jsonString = await _jsonDbService.GetJsonFileByIdAsync(id);
            List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonString);
            foreach (var module in modules)
            {
                _chartService.PlotChart(module);
            }
            byte[] pdf = _pdfbuildService.buildPdf(modules);
            return File(pdf, "application/pdf", "report.pdf");
        }
        [HttpDelete("deleteSingleJSON/{id}")]
        public async Task<IActionResult> DeleteSingleJSON(int id)
        {
            await _jsonDbService.DeleteJsonFileByIdAsync(id);
            return Ok("Deleted");
        }
        [HttpGet("test1")]
        public async Task<IActionResult> GetToken()
        {
            var authToken = await _emailService.PostCreds();
            return Ok(authToken);
        }
    }
}

//https://localhost:7095/api/chart/saveJSON
//https://localhost:7095/api/chart/getAllJSON
//https://localhost:7095/api/chart/getSingleJSON/{id}
//https://localhost:7095/api/chart/deleteSingleJSON/{id}
//https://localhost:7095/api/chart/emailReport

//[HttpPost("emailReport")]
//public async Task<IActionResult> SendReport([FromBody] EmailStuff email)
//{
//    var jsonString = await _jsonDbService.GetJsonFileByIdAsync(email.Id);
//    List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonString);
//    foreach (var module in modules)
//    {
//        _chartService.PlotChart(module);
//    }
//    byte[] pdf = _pdfbuildService.buildPdf(modules);
//    _pdfbuildService.SendEmail(email, pdf);
//    return Ok("Email sent");
//}])