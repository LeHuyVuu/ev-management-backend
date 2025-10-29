using DotNetEnv;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using CustomerService.Infrastructure.Services;
using UtilityService.Infrastructure.Repositories;
using UtilityService.Infrastructure.Services;
using UtilityService.Messaging;
using UtilityService.Models;
using UtilityService.Worker;

var builder = WebApplication.CreateBuilder(args);

// Load env
Env.Load();

// ===== Kết nối DB từ ENV =====
var connectionString =
    $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASS")};";



builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
// Repo đọc "Pg" hoặc "DefaultConnection" → map luôn cho chắc
builder.Configuration["ConnectionStrings:Pg"] = builder.Configuration["ConnectionStrings:DefaultConnection"];

// Map GROQ từ ENV → config (tuỳ chọn nhưng tiện)
builder.Configuration["Groq:ApiKey"] = Environment.GetEnvironmentVariable("GROQ_API_KEY");
builder.Configuration["Groq:BaseUrl"] = Environment.GetEnvironmentVariable("GROQ_BASE_URL") ?? "https://api.groq.com/openai/v1";
builder.Configuration["Groq:Model"]  = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.1-70b-versatile";

// Load cấu hình
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// CORS cho FE
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Repo + Service (KHÔNG dùng interface)
builder.Services.AddScoped<EmailService>();     // nếu bạn dùng
builder.Services.AddScoped<EmailRepository>();     // nếu bạn dùng
builder.Services.AddScoped<ForecastRepository>();
builder.Services.AddScoped<ForecastService>();
// ===== AWS S3 Config =====
builder.Services.AddDefaultAWSOptions(new AWSOptions
{
    Region = RegionEndpoint.GetBySystemName(
        Environment.GetEnvironmentVariable("AWS_REGION") ?? "ap-southeast-2"
    )
});
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<S3StorageService>();

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<S3StorageService>(); 
// HttpClient cho Groq
builder.Services.AddHttpClient("groq", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(cfg["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1");
});

// HostedService (nếu có dùng)
builder.Services.AddHostedService<ForecastAutoRefreshWorker>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Utility Service", Version = "v1" });
});

var app = builder.Build();

var pathBase = "/utility-service";
app.UsePathBase(pathBase);

// Swagger UI dưới PathBase
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "Utility Service v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
