using Microsoft.AspNetCore.Http;
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
        public ChartController()
        {
            _chartService = new ChartService();
        }
        [HttpPost]
        public IActionResult CreateCharts([FromBody] List<Module> modules)
        {
            foreach (var module in modules)
            {
                _chartService.PlotChart(module);
            }
            return Ok("Chart(s) created");
        }
    }
}
