﻿using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using ReportAppAPI.Models;
using ReportAppAPI.Services;
using ScottPlot.Drawing.Colormaps;

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
        private readonly EmailPDFService _emailPDFService;
        public PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromDays(1));

        public ChartController(JsonDbService jsonDbService)
        {
            _chartService = new ChartService();
            _pdfbuildService = new PDFbuildService();
            _jsonDbService = jsonDbService;
            _emailService = new EmailService();
            _emailPDFService = new EmailPDFService();
        }

        [HttpPost("saveJSON")]
        public async Task<IActionResult> SaveJSON([FromBody] List<Module> modules)
        {
            string jsonString = JsonConvert.SerializeObject(modules);
            string name = "report"; //placeholder
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
            if (jsonString == null)
            {
                return NotFound("The requested Id does not exist");
            }
            else
            {
                List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonString);
                foreach (var module in modules)
                {
                    _chartService.PlotChart(module);
                }
                byte[] pdf = _pdfbuildService.buildPdf(modules);
                return File(pdf, "application/pdf", "report.pdf");
            }
        }

        [HttpDelete("deleteSingleJSON/{id}")]
        public async Task<IActionResult> DeleteSingleJSON(int id)
        {
            await _jsonDbService.DeleteJsonFileByIdAsync(id);
            return Ok("Deleted");
        }

        [HttpPost("emailReport/{id}")]
        public async Task<IActionResult> AutoSendReport([FromBody] AutoReport autoReport, int id)
        {
            int time = autoReport.Frequency;
            _timer = new PeriodicTimer(TimeSpan.FromDays(time));
            while (await _timer.WaitForNextTickAsync())
            {
                var jsonString = await _jsonDbService.GetJsonFileByIdAsync(id);
                List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonString);
                foreach (var module in modules)
                {
                    _emailService.PlotAutoChart(module);
                }
                byte[] pdf = _emailPDFService.buildPdf(modules);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ReportApp", "reportApp-autoUpdate@emaxmple.com"));
                message.To.Add(new MailboxAddress("User", autoReport.Email));
                message.Subject = "Weekly Report";
                var builder = new BodyBuilder();
                builder.TextBody = "Weekly report";
                builder.Attachments.Add("report.pdf", pdf, ContentType.Parse("application/pdf"));
                message.Body = builder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("email", "password");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            return Ok("Automated Updates enabled");
        }

        [HttpGet("stopAutoUpdates")]
        public async Task<IActionResult> StopTimer()
        {
            _timer.Dispose();
            return Ok("Stopped automated reports");
        }
    }
}

//https://localhost:7095/api/chart/saveJSON
//https://localhost:7095/api/chart/getAllJSON
//https://localhost:7095/api/chart/getSingleJSON/{id}
//https://localhost:7095/api/chart/deleteSingleJSON/{id}
//https://localhost:7095/api/chart/emailReport/{id}