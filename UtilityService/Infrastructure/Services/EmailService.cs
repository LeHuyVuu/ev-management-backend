using UtilityService.Infrastructure.Repositories;
using UtilityService.Models;

namespace UtilityService.Infrastructure.Services;

public class EmailService
{
    private readonly EmailRepository _emailRepo;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailRepository emailRepo, ILogger<EmailService> logger)
    {
        _emailRepo = emailRepo;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> SendEmailAsync(EmailRequestDto dto)
    {
        try
        {
            // Template HTML
            string htmlBody = $@"
<html>
<head>
  <meta charset='utf-8' />
  <style>
    body {{ font-family: 'Segoe UI', Arial; background:#f6f8fa; padding:20px; }}
    .container {{ background:#fff; padding:25px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,.08); }}
    h2 {{ color:#4F46E5; border-bottom:2px solid #4F46E5; padding-bottom:8px; }}
    .footer {{ margin-top:20px; font-size:12px; color:#888; text-align:center; }}
    .button {{ background:#4F46E5; color:#fff; padding:10px 20px; border-radius:6px; text-decoration:none; }}
  </style>
</head>
<body>
  <div class='container'>
    <h2>{dto.Subject}</h2>
    <div>{dto.Content}</div>
    <p><a class='button' href='ev-management-frontend.vercel.app
' target='_blank' rel='noreferrer'>Truy cập hệ thống</a></p>
    <div class='footer'>© {DateTime.UtcNow.Year} EV Management System. All rights reserved.</div>
  </div>
</body>
</html>";

            var ok = await _emailRepo.SendEmailAsync(dto.ToEmail, dto.Subject, htmlBody);
            return ok
                ? ApiResponse<string>.Success("Email sent successfully")
                : ApiResponse<string>.Fail(500, "Failed to send email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmailService.SendEmailAsync error");
            return ApiResponse<string>.Fail(500, "Error sending email", ex.Message);
        }
    }
}