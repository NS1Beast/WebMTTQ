using System.Net;
using System.Net.Mail;
using WebMTTQ.Services;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Implementation of IEmailService.
    /// Uses SMTP settings stored in CauHinhHeThong table.
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

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = await _settings.GetValueAsync("SmtpHost");
                var smtpPortStr = await _settings.GetValueAsync("SmtpPort");
                var smtpUseSsl = await _settings.GetBooleanAsync("SmtpUseSsl");
                var smtpUsername = await _settings.GetValueAsync("SmtpUsername");
                var smtpPassword = await _settings.GetEncryptedValueAsync("SmtpPassword");
                var smtpFromEmail = await _settings.GetValueAsync("SmtpFromEmail");
                var smtpFromName = await _settings.GetValueAsync("SmtpFromName");

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpFromEmail))
                {
                    _logger.LogWarning("SMTP chưa được cấu hình đầy đủ.");
                    return false;
                }

                int smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = smtpUseSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    Timeout = 30000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFromEmail, string.IsNullOrEmpty(smtpFromName) ? "MTTQ Phường Tân Định" : smtpFromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email đã gửi thành công đến {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi gửi email đến {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "Mã xác thực OTP - MTTQ Phường Tân Định";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8' />
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

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}