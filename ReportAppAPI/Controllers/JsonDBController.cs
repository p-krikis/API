using Microsoft.AspNetCore.Mvc;
using ReportAppAPI.Models;
using ReportAppAPI.Services;

namespace ReportAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JsonDBController : ControllerBase
    {
        private readonly JsonDbService _jsonDbService;
        public JsonDBController()
        {
            _jsonDbService = new JsonDbService();
        }
        [HttpGet]
        public IActionResult GetJson(List<Module> modules)
        {
            _jsonDbService.ThisATest(modules);
            return Ok("all good");
        }
    }
}
