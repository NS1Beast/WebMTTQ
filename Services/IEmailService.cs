namespace WebMTTQ.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }
}