using ReportAppAPI.Models;
using ReportAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.Configure(builder.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<JsonDbService>();

var app = builder.Build();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();