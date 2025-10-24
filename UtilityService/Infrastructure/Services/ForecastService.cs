using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UtilityService.Infrastructure.Repositories;
using UtilityService.Models;

namespace UtilityService.Infrastructure.Services;

public class ForecastService
{
    private readonly IHttpClientFactory _http;
    private readonly ForecastRepository _repo;
    private readonly IConfiguration _cfg;

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
    // 2️⃣ ASK AI (LLM PHÂN TÍCH / GỢI Ý)
    // ============================================================
    public async Task<string> AskAsync(string question, IReadOnlyList<ForecastRow>? rows, CancellationToken ct)
    {
        // --- Lấy API key ---
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? _cfg["Groq:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("❌ Missing GROQ_API_KEY environment variable");

        // --- Chuẩn bị dữ liệu gửi ---
        var sampleRows = (rows ?? Array.Empty<ForecastRow>()).Take(100).ToList();
        var prompt = BuildSystemPrompt();
        var userPayload = new { question, sample = sampleRows };

        // --- Tạo payload chuẩn OpenAI-compatible ---
        var body = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new object[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = JsonSerializer.Serialize(userPayload) }
            },
            temperature = 0.2,
            max_tokens = 700
        };

        var json = JsonSerializer.Serialize(body);
        var client = _http.CreateClient("groq");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // --- Gọi API ---
        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var res = await client.SendAsync(req, ct);
        var resBody = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Groq error {res.StatusCode}: {resBody}");

        // --- Parse kết quả ---
        using var doc = JsonDocument.Parse(resBody);
        var choices = doc.RootElement.GetProperty("choices");

        if (choices.GetArrayLength() == 0)
            return "⚠️ Không có câu trả lời từ mô hình.";

        var answer = choices[0].GetProperty("message").GetProperty("content").GetString();
        return answer?.Trim() ?? "";
    }

    // ============================================================
    // 3️⃣ SYSTEM PROMPT – “TRỢ LÝ DỮ LIỆU EVM” (CẤU TRÚC RÕ RÀNG)
    // ============================================================
    private static string BuildSystemPrompt()
    {
        return @"
Bạn là **Trợ lý Dữ liệu EVM**, hoạt động trong hệ thống phân tích nhu cầu và dự báo bán hàng xe điện.

---

## 🎯 Mục tiêu
- Phân tích **xu hướng forecast** dựa trên các trường dữ liệu:
  `dealer_name`, `version_name`, `week_start`, `baseline_ema`, `discount_rate_active`, `has_promo`, `forecast_units`.
- Trả lời **ngắn gọn, chính xác, dễ hiểu**, có **giải thích hợp lý** (vì sao tăng/giảm).
- Khi người dùng yêu cầu, hãy **xuất dữ liệu biểu đồ (chart)** để hiển thị trực quan.

---

## 🧠 Yêu cầu phản hồi
1. Luôn trả lời **bằng tiếng Việt**, rõ ràng, chuyên nghiệp.
2. Bắt đầu bằng **tóm tắt nhanh (1–2 câu)**.
3. Sau đó nêu **insight chính** (bullet points) — mô tả xu hướng, nguyên nhân, ảnh hưởng của discount/promo.
4. Kết thúc bằng **gợi ý hành động** (bullet points, ngắn, rõ ràng, có điều kiện hoặc ngưỡng đề xuất).
5. Nếu thiếu dữ liệu hoặc chưa chắc chắn, nói rõ:  
   *“Cần thêm dữ liệu về X để xác định chính xác hơn.”*
6. **Không bịa số liệu, không tạo ID giả, không suy luận thiếu dữ kiện.**
7. **Không hiển thị chuỗi suy nghĩ nội bộ** — chỉ đưa ra kết luận, insight, và hành động.

---

## 📊 Gợi ý phân tích
- So sánh tuần theo `week_start` để tính xu hướng tăng/giảm (%).
- Giải thích nguyên nhân khi `discount_rate_active` hoặc `has_promo` thay đổi.
- Nhận diện pattern theo `dealer_name`, `region`, `version_name`.
- Nếu giảm liên tục ≥ 2 tuần → cảnh báo, gợi ý tăng discount hoặc chạy promo.
- Nếu `forecast_units` tăng mạnh mà `discount_rate_active` không đổi → suy ra nhu cầu thật đang tăng.
- Khi có nhiều dealer/version → nêu ai tăng mạnh nhất, ai giảm rõ nhất, và vì sao.

---

## 💬 Giọng văn
- Ngắn gọn, chuyên nghiệp, hướng hành động.
- Không dùng ngôn ngữ marketing hay khoa trương.
- Khi người dùng hỏi “vì sao” → trả lời bằng lập luận logic, dựa vào dữ liệu.

---

## 📦 Cấu trúc kết quả trả về
1. **Tóm tắt nhanh:** 1–2 câu tổng quan.
2. **Insight chính:** 3–6 gạch đầu dòng (dữ kiện + giải thích).
3. **Khuyến nghị hành động:** 3–5 gợi ý cụ thể, có điều kiện.
4. **Hạn chế / cần thêm dữ liệu:** nếu có, liệt kê rõ.

---

## 🪄 Khi người dùng yêu cầu *vẽ biểu đồ*
- **Không tạo hình ảnh**, nhưng hãy sinh **JSON cấu trúc biểu đồ** theo mẫu sau và đặt ở **cuối câu trả lời**:
```json
{
  ""chart_type"": ""line"" | ""bar"" | ""pie"",
  ""x"": [""label1"", ""label2"", ...],
  ""series"": [
    { ""name"": ""Tên nhóm (vd: dealer/version)"", ""data"": [giá trị1, giá trị2, ...] }
  ]
}";

    }
}
