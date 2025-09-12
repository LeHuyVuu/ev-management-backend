var builder = WebApplication.CreateBuilder(args);

// Swagger chỉ cho Gateway nếu bạn muốn, không bắt buộc
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// YARP: đọc từ appsettings
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Proxy swagger của từng service qua gateway (tuỳ chọn)
    c.SwaggerEndpoint("/brand/swagger/v1/swagger.json", "BrandService");
    c.SwaggerEndpoint("/dealer/swagger/v1/swagger.json", "DealerService");
});

app.UseRouting();

// Đặt MapReverseProxy SAU UseRouting
app.MapReverseProxy();

app.Run();