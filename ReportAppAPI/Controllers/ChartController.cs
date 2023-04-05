using Microsoft.AspNetCore.Mvc;
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
        public ChartController()
        {
            _chartService = new ChartService();
            _pdfbuildService = new PDFbuildService();
        }
        [HttpPost]
        public IActionResult CreateCharts([FromBody] List<Module> modules)
        {
            foreach (var module in modules)
            {
                _chartService.PlotChart(module);
            }
            _pdfbuildService.buildPdf(modules);
            return Ok("PDF created");
        }
    }
}
