using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Service for sending emails via SMTP.
    /// Supports OTP emails and general-purpose notification emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a simple email to a single recipient.
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Sends an email using a flexible EmailMessage object.
        /// Supports multiple recipients, CC, BCC, and attachments.
        /// </summary>
        Task<bool> SendEmailAsync(EmailMessage message);

        /// <summary>
        /// Sends an OTP verification email.
        /// </summary>
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode);
    }
}