using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using Npgsql;
using ProductService.Context;
using ProductService.Extensions.Mapper;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Services;
using ProductService.Context;
using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Load env
Env.Load();

// Create connection string
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS")};";

// Load cấu hình
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Trung thu gan lai connected string
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// Add controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfiles).Assembly);


// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// DbContext
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString));

// DI các Repository và Service
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ContractRepository>();
builder.Services.AddScoped<ContractService>();

// Repositories

// Đăng ký Amazon S3 client
builder.Services.AddAWSService<IAmazonS3>();

// Đăng ký custom service
builder.Services.AddScoped<S3StorageService>();
// builder.Services.AddHostedService<ProductStockUpdateConsumer>();

// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
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
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"status\":401,\"message\":\"Unauthorized: Invalid token\"}"
                );
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"status\":401,\"message\":\"Unauthorized: Token is missing or expired\"}"
                );
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


// ✅ THÊM: nếu bạn reverse proxy dưới sub-path (ví dụ: /product-service)
var pathBase = "/dealer-service"; // 🧠 sửa theo sub-path của service bạn
app.UsePathBase(pathBase);


app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        // Nếu là production thì luôn luôn https://evm.webredirect.org/dealer-service
        if (builder.Environment.IsProduction())
        {
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = $"https://evm.webredirect.org{pathBase}" }
            };
        }
        else
        {
            // Local thì tự lấy scheme + host (http://localhost:5000/dealer-service)
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{pathBase}" }
            };
        }
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger";
});



app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger";
});



// ✅ Giữ nguyên toàn bộ logic cũ bên dưới
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
