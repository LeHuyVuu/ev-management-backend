using Google.Apis.Auth.OAuth2;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UtilityService.Infrastructure.Services;

public class VertexAiService
{
    private readonly IHttpClientFactory _http;
    private readonly string _projectId;
    private readonly string _location;
    private readonly GoogleCredential _cred;

    public VertexAiService(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http;

        // ✅ Đọc chung từ ENV (ưu tiên) hoặc config
        _projectId = Environment.GetEnvironmentVariable("VERTEX_PROJECT_ID")
                     ?? cfg["VertexAI:ProjectId"]
                     ?? throw new Exception("Missing VertexAI:ProjectId");

        _location = Environment.GetEnvironmentVariable("VERTEX_LOCATION")
                     ?? cfg["VertexAI:Location"]
                     ?? "global";

        // ✅ Ưu tiên đọc từ GOOGLE_APPLICATION_CREDENTIALS (server)
        // nếu không có thì fallback về file local (dev)
        var keyPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                      ?? cfg["VertexAI:KeyFile"];

        if (string.IsNullOrEmpty(keyPath))
            throw new Exception("Missing VertexAI key credentials.");

        _cred = GoogleCredential.FromFile(keyPath)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    public async Task<string> GenerateAsync(string message, CancellationToken ct)
    {
        var token = await _cred.UnderlyingCredential.GetAccessTokenForRequestAsync();
        var client = _http.CreateClient();

        var url =
            $"https://aiplatform.googleapis.com/v1/projects/{_projectId}/locations/{_location}/publishers/google/models/gemini-2.5-pro:generateContent";

        var body = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = message } } }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var resp = await client.SendAsync(req, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Vertex AI error {(int)resp.StatusCode}: {raw}");

        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.GetProperty("candidates")[0]
            .GetProperty("content").GetProperty("parts")[0]
            .GetProperty("text").GetString()
            ?? "(Không có phản hồi)";
    }
}
