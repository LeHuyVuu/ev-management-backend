using FinancialService.Models;
using FinancialService.Repositories;
using UtilityService.Models;

namespace FinancialService.Services
{
    public class EmailService
    {
        private readonly EmailRepository _emailRepo;

        public EmailService(EmailRepository emailRepo)
        {
            _emailRepo = emailRepo;
        }

        public async Task<ApiResponse<string>> SendEmailAsync(EmailRequestDto dto)
        {
            try
            {
                // Format HTML body đẹp
                string htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', sans-serif;
                            background-color: #f6f8fa;
                            padding: 20px;
                        }}
                        .container {{
                            background: white;
                            padding: 25px;
                            border-radius: 12px;
                            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                        }}
                        h2 {{
                            color: #4F46E5;
                            border-bottom: 2px solid #4F46E5;
                            padding-bottom: 8px;
                        }}
                        .footer {{
                            margin-top: 20px;
                            font-size: 12px;
                            color: #888;
                            text-align: center;
                        }}
                        .button {{
                            background-color: #4F46E5;
                            color: white;
                            padding: 10px 20px;
                            border-radius: 6px;
                            text-decoration: none;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>{dto.Subject}</h2>
                        <p>{dto.Content}</p>
                        <p><a class='button' href='https://evm.vn'>Truy cập hệ thống</a></p>
                        <div class='footer'>
                            © {DateTime.UtcNow.Year} EV Management System. All rights reserved.
                        </div>
                    </div>
                </body>
                </html>";

                var success = await _emailRepo.SendEmailAsync(dto.ToEmail, dto.Subject, htmlBody);
                if (!success)
                    return ApiResponse<string>.Fail(500, "Failed to send email");

                return ApiResponse<string>.Success("Email sent successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(500, "Error sending email", ex.Message);
            }
        }
    }
}
