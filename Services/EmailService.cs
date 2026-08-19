using System.Net;
using System.Net.Mail;
using System.Text;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Implementation of IEmailService.
    /// Uses SMTP settings stored in CauHinhHeThong table.
    /// Supports OTP emails and general-purpose notification emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ISystemSettingsService _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ISystemSettingsService settings, ILogger<EmailService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        // ================================================
        // PUBLIC METHODS
        // ================================================

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var message = EmailMessage.Create(toEmail, subject, body, isHtml);
            return await SendEmailAsync(message);
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            try
            {
                // Validate message
                if (message.To == null || message.To.Count == 0)
                {
                    _logger.LogWarning("Không thể gửi email: thiếu người nhận.");
                    return false;
                }

                // Load SMTP settings
                var smtpConfig = await LoadSmtpConfigAsync();
                if (!smtpConfig.IsValid)
                {
                    _logger.LogWarning("SMTP chưa được cấu hình đầy đủ: {Reason}", smtpConfig.InvalidReason);
                    return false;
                }

                using var client = new SmtpClient(smtpConfig.Host, smtpConfig.Port)
                {
                    EnableSsl = smtpConfig.UseSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password),
                    Timeout = 30000
                };

                using var mailMessage = BuildMailMessage(message, smtpConfig);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email đã gửi thành công đến {Recipients}", string.Join(", ", message.To));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi email đến {Recipients}", string.Join(", ", message.To ?? new List<string>()));
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "Mã xác thực OTP - MTTQ Phường Tân Định";
            var body = BuildOtpEmailBody(toEmail, otpCode);
            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ================================================
        // PRIVATE HELPERS
        // ================================================

        private async Task<SmtpConfig> LoadSmtpConfigAsync()
        {
            var config = new SmtpConfig
            {
                Host = await _settings.GetValueAsync("SmtpHost"),
                Port = int.TryParse(await _settings.GetValueAsync("SmtpPort"), out var port) ? port : 587,
                UseSsl = await _settings.GetBooleanAsync("SmtpUseSsl"),
                Username = await _settings.GetValueAsync("SmtpUsername"),
                Password = await _settings.GetEncryptedValueAsync("SmtpPassword"),
                FromEmail = await _settings.GetValueAsync("SmtpFromEmail"),
                FromName = await _settings.GetValueAsync("SmtpFromName")
            };

            if (string.IsNullOrEmpty(config.FromName))
            {
                config.FromName = "MTTQ Phường Tân Định";
            }

            return config;
        }

        private MailMessage BuildMailMessage(EmailMessage message, SmtpConfig config)
        {
            var fromEmail = message.FromEmail ?? config.FromEmail;
            var fromName = message.FromName ?? config.FromName;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml,
                // QUAN TRỌNG: Đặt UTF-8 cho encoding để hỗ trợ tiếng Việt có dấu trong email HTML + subject
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8,
                // Chống spam: đánh dấu là email bình thường (không quảng cáo)
                Priority = MailPriority.Normal
            };

            // Chống spam: set ReplyTo = chính From, giúp mail server nhận ra là email hợp lệ
            try
            {
                mailMessage.ReplyToList.Add(new MailAddress(fromEmail));
            }
            catch { /* ignore invalid reply-to */ }

            // Chống spam: thêm header X-Mailer + X-Priority chuẩn
            mailMessage.Headers.Add("X-Mailer", "MTTQ Tan Dinh Mailer v1.0");
            mailMessage.Headers.Add("X-Priority", "3"); // 3 = Normal (1 = High, 2 = Urgent, 3 = Normal)
            mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

            // Add recipients
            foreach (var to in message.To)
            {
                if (!string.IsNullOrWhiteSpace(to))
                    mailMessage.To.Add(to);
            }

            // Add CC
            foreach (var cc in message.Cc ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(cc))
                    mailMessage.CC.Add(cc);
            }

            // Add BCC
            foreach (var bcc in message.Bcc ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(bcc))
                    mailMessage.Bcc.Add(bcc);
            }

            // Add attachments
            foreach (var attachment in message.Attachments ?? new List<EmailAttachment>())
            {
                if (attachment.Content != null && attachment.Content.Length > 0)
                {
                    var stream = new MemoryStream(attachment.Content);
                    var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                    mailMessage.Attachments.Add(mailAttachment);
                }
            }

            return mailMessage;
        }

        private string BuildOtpEmailBody(string toEmail, string otpCode)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f7f7f7; margin: 0; padding: 20px; }}
    .container {{ max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); }}
    .header {{ background: linear-gradient(135deg, #8b1a2b, #a31f34); padding: 20px 30px; text-align: center; }}
    .header h1 {{ color: #e8b84b; font-size: 20px; margin: 0; font-family: Georgia, serif; }}
    .header p {{ color: rgba(255,255,255,0.7); font-size: 12px; margin: 6px 0 0; }}
    .content {{ padding: 30px; }}
    .content p {{ font-size: 14px; color: #444; line-height: 1.6; }}
    .otp-box {{ background: #f8f4f0; border: 2px dashed #8b1a2b; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0; }}
    .otp-box .otp {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #8b1a2b; font-family: 'Courier New', monospace; }}
    .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px 16px; border-radius: 6px; font-size: 13px; color: #92400e; margin: 16px 0; }}
    .footer {{ background: #f8f8f8; padding: 15px 30px; text-align: center; font-size: 12px; color: #999; }}
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h1>Ủy ban MTTQ Việt Nam Phường Tân Định</h1>
        <p>Hệ thống quản trị nội dung</p>
    </div>
    <div class='content'>
        <p>Xin chào,</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản <strong>{toEmail}</strong>. Vui lòng sử dụng mã OTP dưới đây để xác thực:</p>
        <div class='otp-box'>
            <div class='otp'>{otpCode}</div>
        </div>
        <div class='warning'>
            <strong>Lưu ý:</strong> Mã OTP này chỉ có hiệu lực trong <strong>2 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.
        </div>
        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
    </div>
    <div class='footer'>
        © 2025 Ủy ban MTTQ Việt Nam Phường Tân Định. Email này được gửi tự động, vui lòng không trả lời.
    </div>
</div>
</body>
</html>";
        }

        // ================================================
        // SMTP CONFIG CLASS
        // ================================================

        private class SmtpConfig
        {
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 587;
            public bool UseSsl { get; set; } = true;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FromEmail { get; set; } = string.Empty;
            public string FromName { get; set; } = string.Empty;

            public bool IsValid
            {
                get
                {
                    return !string.IsNullOrEmpty(Host)
                        && !string.IsNullOrEmpty(FromEmail)
                        && !string.IsNullOrEmpty(Username)
                        && !string.IsNullOrEmpty(Password);
                }
            }

            public string InvalidReason
            {
                get
                {
                    if (string.IsNullOrEmpty(Host)) return "thiếu SmtpHost";
                    if (string.IsNullOrEmpty(FromEmail)) return "thiếu SmtpFromEmail";
                    if (string.IsNullOrEmpty(Username)) return "thiếu SmtpUsername";
                    if (string.IsNullOrEmpty(Password)) return "thiếu SmtpPassword";
                    return "không xác định";
                }
            }
        }
    }
}