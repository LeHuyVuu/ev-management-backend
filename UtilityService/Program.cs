using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using UtilityService.Infrastructure.Repositories;
using UtilityService.Infrastructure.Services;   // ✅ EmailService ở đây
using UtilityService.Messaging;
using UtilityService.Models;

var builder = WebApplication.CreateBuilder(args);

// Load env
Env.Load();

// Load cấu hình
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register repo + service
builder.Services.AddSingleton<EmailRepository>();
builder.Services.AddSingleton<EmailService>();

// ✅ Đăng ký Consumer (sau khi có EmailService)
builder.Services.AddHostedService<TestDriveCreatedConsumer>();

// Swagger (giữ nguyên phần còn lại của bạn) ...
// Auth (giữ nguyên) ...
// Cors (giữ nguyên) ...

var app = builder.Build();

var pathBase = "/utility-service";
app.UsePathBase(pathBase);

// Swagger UI (giữ nguyên) ...

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();