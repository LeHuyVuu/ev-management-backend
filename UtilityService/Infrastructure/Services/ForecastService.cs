using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UtilityService.Infrastructure.Repositories;
using UtilityService.Models;

namespace UtilityService.Infrastructure.Services;

public class ForecastService
{
    private readonly IHttpClientFactory _http;
    private readonly ForecastRepository _repo;
    private readonly IConfiguration _cfg;

    // Bạn có thể cấu hình BaseAddress cho named client "groq" là:
    // https://api.groq.com/openai/v1/
    // Khi đó, ENDPOINT dưới đây chỉ cần là "chat/completions".
    private const string ENDPOINT = "chat/completions";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public ForecastService(IHttpClientFactory http, ForecastRepository repo, IConfiguration cfg)
    {
        _http = http;
        _repo = repo;
        _cfg = cfg;
    }

    // ============================================================
    // 1️⃣ SEARCH FORECAST (DỮ LIỆU CƠ SỞ)
    // ============================================================
    public async Task<PagedResult<ForecastRow>> SearchAsync(ForecastQuery q, CancellationToken ct)
    {
        var (rows, total) = await _repo.GetForecastAsync(q, ct);
        return new PagedResult<ForecastRow>
        {
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize,
            Rows = rows
        };
    }

    // ============================================================
    // 2️⃣ ASK AI (LLM PHÂN TÍCH / GỢI Ý – HỘI THOẠI TỰ NHIÊN)
    // ============================================================
    public async Task<string> AskAsync(string question, IReadOnlyList<ForecastRow>? rows, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question is required.", nameof(question));

        // --- Lấy API key ---
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? _cfg["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "❌ Missing GROQ_API_KEY. Set environment variable or configuration key Groq:ApiKey.");

        // --- Chuẩn bị dữ liệu gửi ---
        var sampleRows = (rows ?? Array.Empty<ForecastRow>()).Take(100).ToList();

        // Hướng dẫn hội thoại được bọc trong system prompt
        var systemPrompt = BuildSystemPrompt();

        // Nhắc nhở ngắn để model luôn giữ phong cách chat tự nhiên, hỏi lại khi mơ hồ
        var userEnvelope = new
        {
            instruction = "Trả lời tự nhiên như chat. Nếu chưa rõ, hỏi lại 1–3 câu ngắn rồi dừng.",
            question,
            sample = sampleRows
        };

        // --- Payload OpenAI-compatible ---
        var body = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = JsonSerializer.Serialize(userEnvelope, JsonOpts) }
            },
            // Nâng nhẹ temperature cho giọng tự nhiên, tăng max_tokens để đủ chỗ cho insight
            temperature = 0.35,
            max_tokens = 1200
        };

        var json = JsonSerializer.Serialize(body, JsonOpts);

        // --- HttpClient ---
        var client = _http.CreateClient("groq");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var req = new HttpRequestMessage(HttpMethod.Post, ENDPOINT)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var res = await client.SendAsync(req, ct).ConfigureAwait(false);
        var resBody = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
        {
            // Trả về thông tin lỗi rõ ràng để dễ debug cấu hình/endpoint
            throw new HttpRequestException($"Groq HTTP {(int)res.StatusCode} {res.StatusCode}: {resBody}");
        }

        // --- Parse kết quả ---
        try
        {
            using var doc = JsonDocument.Parse(resBody);
            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                return "⚠️ Không có câu trả lời từ mô hình.";

            var msg = choices[0].GetProperty("message");
            var content = msg.GetProperty("content").GetString();
            return (content ?? string.Empty).Trim();
        }
        catch (Exception ex)
        {
            // Nếu response đổi schema hoặc JSON lỗi, ném nguyên body để dễ xác định
            throw new InvalidOperationException($"❌ Lỗi parse JSON từ Groq: {ex.Message}\nRaw: {resBody}");
        }
    }

    // ============================================================
    // 3️⃣ SYSTEM PROMPT – “TRỢ LÝ DỮ LIỆU EVM” (HỘI THOẠI TỰ NHIÊN + HỎI LẠI KHI MƠ HỒ)
    // ============================================================
private static string BuildSystemPrompt()
{
    return @"
Bạn là Trợ lý Dữ liệu EVM cho hệ thống phân tích nhu cầu và dự báo bán hàng xe điện.

MỤC TIÊU
- Trả lời ngắn gọn, tự nhiên, thân thiện; hơi dí dỏm vừa phải.
- Mặc định tiếng Việt; nếu người dùng hỏi tiếng Anh thì trả lời tiếng Anh.
- Không dùng ký tự * hoặc markdown đậm/nhạt.
- Luôn mở đầu bằng một câu kết luận cực ngắn (≤ 18 từ) nếu và chỉ nếu người dùng đang yêu cầu phân tích.

PHÂN LOẠI INTENT (tự động)
- greeting: lời chào/xã giao (“hello”, “hi”, “chào”, “alo”, “yo”…). → Chỉ chào lại tự nhiên, KHÔNG phân tích.
- capability_query: hỏi “bạn/ai làm được gì”, “mày giúp gì được”, “how can you help”. → Trả lời ngắn gọn danh mục năng lực + ví dụ câu hỏi gợi ý, KHÔNG phân tích.
- thanks/ack: “cảm ơn”, “ok”, “ừ”, “yep”… → Đáp ngắn gọn; không phân tích.
- clarify_needed: câu mơ hồ (“xem tình hình tuần này đi”). → Hỏi lại tối đa 1–3 câu ngắn, rồi dừng.
- analysis/actionable: có dữ liệu hoặc yêu cầu rõ. → Phân tích theo cấu trúc dưới.
- smalltalk: chuyện phiếm ngắn. → Trả lời tự nhiên, không lôi dữ liệu vào.

ỨNG XỬ MẪU
- greeting → “Chào bạn! Mình ở đây, bạn muốn xem dự báo gì hôm nay?”
- capability_query → “Mình có thể: 1) tóm tắt 4 tuần gần nhất, 2) so sánh dealer/version, 3) bắt top tăng/giảm, 4) đánh giá tác động promo/discount, 5) xuất JSON chart/table. Bạn muốn bắt đầu với nội dung nào?”
- thanks/ack → “Rất vui được giúp bạn! Cần gì cứ gọi mình nha.”
- clarify_needed → Hỏi 1–3 câu ngắn:
  - Xem theo dealer nào hay tất cả?
  - Tuần gần nhất hay 4 tuần gần nhất?
  - So sánh theo version hay theo dealer?

DỮ LIỆU ĐẦU VÀO
- instruction, question, sample (tối đa 100 ForecastRow: dealer_name, version_name, week_start, baseline_ema, discount_rate_active, has_promo, forecast_units).

TỰ CHỦ DỮ LIỆU
- Nếu sample < 2 tuần: giả định hệ thống đã tự mở rộng phạm vi gần đây.
- Không nói “dữ liệu không đủ”. Thay vào đó nêu: “Đã tự mở rộng phạm vi dữ liệu gần đây để phân tích”, rồi phân tích ở mức thận trọng.
- Không phát minh số ngoài sample. Khi dữ liệu mỏng, dùng ngôn ngữ định tính (“tăng nhẹ”, “ổn định”, “chưa rõ xu hướng”).

QUY TẮC PHÂN TÍCH
- So sánh theo tuần (week_start). % thay đổi = (tuần_này - tuần_trước)/tuần_trước*100.
- Liên hệ discount_rate_active/has_promo với forecast_units.
- Bắt nhóm tăng mạnh/giảm rõ theo dealer_name/version_name.
- Cảnh báo nếu giảm ≥ 2 tuần liên tiếp.
- Nếu discount gần như không đổi mà forecast_units tăng mạnh → nhu cầu thật.
- Giới hạn kết luận trong phạm vi sample; nêu rõ thiếu gì.

MẶC ĐỊNH KHI THIẾU CHỈ ĐỊNH
- Phạm vi: 4 tuần gần nhất trong sample.
- So sánh theo version_name trong từng dealer_name nếu có.
- Ngưỡng biến động đáng chú ý: ±10% WoW.
- Top K: 5.

CẤU TRÚC KHI PHÂN TÍCH (analysis/actionable)
0) Kết luận một dòng (≤18 từ, chỉ dùng khi intent là phân tích).
1) Tóm tắt nhanh (1–2 câu).
2) Insight chính (3–6 dòng).
3) Khuyến nghị hành động (2–5 dòng, có ngưỡng/điều kiện).
4) Hạn chế/cần thêm (dữ liệu thiếu, giả định).
5) Nếu người dùng yêu cầu chart/table: JSON chuẩn ở cuối.

JSON CHUẨN (khi được yêu cầu)
Chart:
{
  ""chart_type"": ""line"" | ""bar"",
  ""x"": [""2025-09-29"", ""2025-10-06""],
  ""series"": [{ ""name"": ""Dealer/Version/All"", ""data"": [123,141] }],
  ""meta"": { ""granularity"": ""week"", ""dimension"": ""dealer_name|version_name|all"", ""notes"": ""Ngưỡng/giả định được dùng."" }
}
Table:
{
  ""columns"": [""dealer_name"", ""version_name"", ""week_start"", ""forecast_units"", ""discount_rate_active"", ""has_promo""],
  ""rows"": [[ ""A"", ""Standard"", ""2025-10-06"", 141, 0.05, true ]]
}

KIỂM TRA TRƯỚC KHI GỬI
- Có chọn đúng intent không? greeting/capability/clarify/smalltalk thì không phân tích.
- Không dùng ký tự * hoặc markdown?
- Không bịa số liệu? Câu ngắn, không lặp?
- Nếu phân tích: đã mở đầu bằng kết luận ≤ 18 từ?
";
}


}