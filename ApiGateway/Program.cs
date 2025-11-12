using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// 1) YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2) Rate Limiter (ASP.NET Core 7/8+)
builder.Services.AddRateLimiter(options =>
{
    // 100 req / phút / IP
    options.AddPolicy("per-ip-fixed-window", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// 3) Caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

var app = builder.Build();

/* ============================================================
 * A. Logging middleware (đẹp, rõ ràng, có màu; không icon)
 * - In Request/Response với thời gian xử lý và Correlation-Id
 * - Sample body text/json (giới hạn 2KB)
 * ============================================================ */
// Logging middleware to log IP Address
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    // Get the IP address
    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    
    // Correlation Id
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var cid)
        ? cid.ToString()
        : Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Correlation-Id"] = correlationId;

    var method = context.Request.Method;
    var path = context.Request.Path + context.Request.QueryString;
    var now = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

    // Header line
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("────────────────────────────────────────────────────────────────");
    Console.ResetColor();

    // Request line
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"[{now}] REQUEST");
    Console.ResetColor();
    Console.WriteLine($"  Method       : {method}");
    Console.WriteLine($"  Path         : {path}");
    Console.WriteLine($"  Correlation  : {correlationId}");
    Console.WriteLine($"  IP Address   : {remoteIp}");

    // Sample request body if it's text/json
    string? reqSample = null;
    if (context.Request.ContentLength > 0 &&
        (method == "POST" || method == "PUT" || method == "PATCH") &&
        (context.Request.ContentType?.StartsWith("application/json") == true ||
         context.Request.ContentType?.StartsWith("text/") == true))
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        char[] buffer = new char[2048];
        int read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
        reqSample = new string(buffer, 0, read);
        context.Request.Body.Position = 0;

        Console.WriteLine("  Body(sample) : " + TrimForLog(reqSample));
    }

    // Handle response logging
    var originalBody = context.Response.Body;
    await using var memStream = new MemoryStream();
    context.Response.Body = memStream;

    try
    {
        await next.Invoke();
    }
    finally
    {
        sw.Stop();

        memStream.Position = 0;
        string? respSample = null;
        var contentType = context.Response.ContentType ?? "";

        if (contentType.StartsWith("application/json") || contentType.StartsWith("text/"))
        {
            using var respReader = new StreamReader(memStream, Encoding.UTF8, leaveOpen: true);
            var text = await respReader.ReadToEndAsync();
            respSample = TrimForLog(text);
            memStream.Position = 0;
        }

        var status = context.Response.StatusCode;
        var elapsed = sw.ElapsedMilliseconds;

        // Response line
        var stamp2 = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        if (status >= 500) Console.ForegroundColor = ConsoleColor.Red;
        else if (status >= 400) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine($"[{stamp2}] RESPONSE");
        Console.ResetColor();
        Console.WriteLine($"  Status       : {status}");
        Console.WriteLine($"  Duration     : {elapsed} ms");
        Console.WriteLine($"  Content-Type : {contentType}");
        if (!string.IsNullOrEmpty(respSample))
        {
            Console.WriteLine($"  Body(sample) : {respSample}");
        }

        // Footer line
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("────────────────────────────────────────────────────────────────\n");
        Console.ResetColor();

        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    static string TrimForLog(string s) =>
        s.Length <= 2000 ? s : s.Substring(0, 2000) + "…(truncated)";
});

/* ============================================================
 * B. Rate Limiting
 * ============================================================ */
app.UseRateLimiter();

/* ============================================================
 * C. Response Caching header helper
 * - Đặt Cache-Control mặc định cho GET 2xx nếu endpoint chưa đặt
 * - Dùng OnStarting để đảm bảo header có trước khi gửi body
 * ============================================================ */
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method))
    {
        context.Response.OnStarting(() =>
        {
            // Chỉ set khi chưa có và status 2xx
            if (!context.Response.HasStarted &&
                (context.Response.StatusCode is >= 200 and < 300) &&
                !context.Response.Headers.ContainsKey("Cache-Control"))
            {
                // Cho phép public cache 30 giây
                context.Response.Headers["Cache-Control"] = "public,max-age=30";
                // context.Response.Headers["Vary"] = "Accept-Encoding";
            }
            return Task.CompletedTask;
        });
    }

    await next();
});

/* ============================================================
 * D. ResponseCaching (chuẩn HTTP)
 * ============================================================ */
app.UseResponseCaching();

/* ============================================================
 * E. Micro-cache in-memory cho GET
 * - Cache 2xx theo Path+Query trong 10 giây
 * - Giảm tải spike cho route GET nóng trước khi vào proxy
 * ============================================================ */
app.Use(async (context, next) =>
{
    if (!HttpMethods.IsGet(context.Request.Method))
    {
        await next();
        return;
    }

    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
    var key = $"microcache::{context.Request.Path}{context.Request.QueryString}";

    if (cache.TryGetValue<(int status, IHeaderDictionary headers, byte[] body)>(key, out var entry))
    {
        context.Response.StatusCode = entry.status;
        foreach (var h in entry.headers)
        {
            // Sao chép header đơn giản (tránh hop-by-hop nếu có)
            context.Response.Headers[h.Key] = h.Value;
        }
        await context.Response.Body.WriteAsync(entry.body, 0, entry.body.Length);
        return;
    }

    var original = context.Response.Body;
    await using var buffer = new MemoryStream();
    context.Response.Body = buffer;

    await next();

    if (context.Response.StatusCode is >= 200 and < 300)
    {
        var bytes = buffer.ToArray();

        // Clone headers
        var clonedHeaders = new HeaderDictionary();
        foreach (var h in context.Response.Headers)
            clonedHeaders[h.Key] = h.Value;

        cache.Set(key, (context.Response.StatusCode, clonedHeaders, bytes),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

        buffer.Position = 0;
        await buffer.CopyToAsync(original);
    }
    else
    {
        buffer.Position = 0;
        await buffer.CopyToAsync(original);
    }

    context.Response.Body = original;
});

/* ============================================================
 * F. HTTPS redirect (nếu cần)
 * ============================================================ */
app.UseHttpsRedirection();

/* ============================================================
 * G. Reverse Proxy
 * ============================================================ */
app.MapReverseProxy();

/* ============================================================
 * H. Demo endpoints (tùy chọn để bạn test nhanh; có thể xóa)
 *  - Nên đặt trước MapReverseProxy nếu muốn kiểm tra pipeline
 * ============================================================ */
// app.MapGet("/api/ping", () => Results.Ok(new { ok = true }));
// app.MapGet("/api/time", () => Results.Ok(new { serverTime = DateTimeOffset.UtcNow }));
// app.MapPost("/api/echo", async (HttpContext ctx) =>
// {
//     using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8);
//     var body = await reader.ReadToEndAsync();
//     return Results.Content(body, "application/json");
// });

app.Run();
