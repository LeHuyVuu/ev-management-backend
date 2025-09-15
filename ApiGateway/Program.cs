var builder = WebApplication.CreateBuilder(args);

//Cấu hình gateway yarp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

app.UseHttpsRedirection();

app.MapReverseProxy();

// Logging middleware (for debug)
app.Use(async (context, next) =>
{
    Console.WriteLine($"[Gateway] → {context.Request.Method} {context.Request.Path}");
    await next();
});



app.Run();