using Microsoft.AspNetCore.Mvc;
using UtilityService.Infrastructure.Services;

namespace UtilityService.Infrastructure.Controller;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly VertexAiService _vertexAiService;

    public ChatController(VertexAiService vertexAiService)
    {
        _vertexAiService = vertexAiService;
    }

    public record ChatRequest(
        string Message,
        string? SystemPrompt = """
                               Bạn là một trợ lý AI phong cách Gen Z, vui vẻ và hài hước.
                               Khi trả lời:
                               - Trả về CHUỖI THUẦN (plain text), KHÔNG dùng markdown, KHÔNG ký tự đặc biệt như *, **, \n, hoặc bullet.
                               - Giữ câu trả lời ngắn gọn (tối đa 4-6 câu), vui, dễ hiểu, thân thiện.
                               - Có thể thêm emoji nhẹ nếu hợp lý (😎😂).
                               """

    );

    public record ChatResponse(string Answer);

    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { error = "Message is required." });

        var prompt = string.IsNullOrWhiteSpace(req.SystemPrompt)
            ? req.Message.Trim()
            : $"{req.SystemPrompt.Trim()}\n\nUSER:\n{req.Message.Trim()}";

        var answer = await _vertexAiService.GenerateAsync(prompt, ct);

        return Ok(new ChatResponse(answer));
    }
}