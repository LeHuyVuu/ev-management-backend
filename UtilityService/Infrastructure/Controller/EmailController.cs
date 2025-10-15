using FinancialService.Models;
using FinancialService.Services;
using Microsoft.AspNetCore.Mvc;
using UtilityService.Models;

namespace FinancialService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ToEmail))
                return BadRequest(ApiResponse<string>.Fail(400, "Invalid request"));

            var result = await _emailService.SendEmailAsync(dto);
            return StatusCode(result.Status, result);
        }
    }
}