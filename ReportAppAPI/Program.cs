using ReportAppAPI.Models;
using ReportAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel(options => options.Configure(builder.Configuration.GetSection("Kestrel")));

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<JsonDbService>();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAllOrigins",
//               builder =>
//               {
//            builder.AllowAnyOrigin()
//                   .AllowAnyMethod()
//                   .AllowAnyHeader();
//        });
//});

var app = builder.Build();
//app.UseCors("AllowAllOrigins");
//app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
app.MapControllers();
app.Run();