using BrandService.Context;
using BrandService.Extensions.Mapper;
using BrandService.Infrastructure.Repositories;
using BrandService.Infrastructure.Services;
using BrandService.Kafka;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Application.ExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load .env
Env.Load();

// ✅ Connection string
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS")};";
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// ✅ Add Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ✅ Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // XML docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, true);

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Brand API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ✅ DbContext
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

// ✅ DI Repositories & Services
builder.Services.AddSingleton<BrandProducer>();
builder.Services.AddScoped<DealerRepository>();
builder.Services.AddScoped<DealerService>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserRepository>();

// ✅ AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

// ✅ Authentication + JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"status\":401,\"message\":\"Unauthorized: Invalid token\"}");
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"status\":401,\"message\":\"Unauthorized: Token is missing or expired\"}");
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ Exception handler (luôn bật, kể cả Dev/Prod)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// ✅ Path base (nếu chạy dưới sub-path)
var pathBase = "/brand-service";
app.UsePathBase(pathBase);

// ✅ Swagger
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer
            {
                Url = app.Environment.IsProduction()
                    ? $"https://evm.webredirect.org{pathBase}"
                    : $"{httpReq.Scheme}://{httpReq.Host.Value}{pathBase}"
            }
        };
    });
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "Brand API V1");
    c.RoutePrefix = "swagger";
});

// ✅ Middleware pipeline
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
