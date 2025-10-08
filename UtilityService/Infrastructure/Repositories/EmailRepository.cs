using System.Net.Mail;
using FinancialService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace FinancialService.Repositories
{
    public class EmailRepository
    {
        private readonly EmailSettings _settings;

        public EmailRepository(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_settings.SenderEmail);
                email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body,
                    TextBody = "Your email client does not support HTML content."
                };

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}