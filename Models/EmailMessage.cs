using System.Net.Mail;

namespace WebMTTQ.Models
{
    /// <summary>
    /// Represents an email message to be sent.
    /// Supports multiple recipients, CC, BCC, and attachments.
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Primary recipient email addresses.
        /// </summary>
        public List<string> To { get; set; } = new();

        /// <summary>
        /// Carbon copy recipient email addresses.
        /// </summary>
        public List<string> Cc { get; set; } = new();

        /// <summary>
        /// Blind carbon copy recipient email addresses.
        /// </summary>
        public List<string> Bcc { get; set; } = new();

        /// <summary>
        /// Email subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Email body content.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Whether the body is HTML or plain text.
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// Optional attachments to include in the email.
        /// </summary>
        public List<EmailAttachment> Attachments { get; set; } = new();

        /// <summary>
        /// Optional custom From address. If null, uses the configured SMTP From.
        /// </summary>
        public string? FromEmail { get; set; }

        /// <summary>
        /// Optional custom From display name. If null, uses the configured SMTP From name.
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// Creates a simple email message with a single recipient.
        /// </summary>
        public static EmailMessage Create(string toEmail, string subject, string body, bool isHtml = true)
        {
            return new EmailMessage
            {
                To = new List<string> { toEmail },
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };
        }

        /// <summary>
        /// Creates an email message with multiple recipients.
        /// </summary>
        public static EmailMessage Create(IEnumerable<string> toEmails, string subject, string body, bool isHtml = true)
        {
            return new EmailMessage
            {
                To = toEmails.ToList(),
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };
        }
    }

    /// <summary>
    /// Represents an email attachment.
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// File name to display in the email.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File content as bytes.
        /// </summary>
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// MIME content type. Defaults to application/octet-stream.
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// Creates an attachment from bytes.
        /// </summary>
        public static EmailAttachment FromBytes(string fileName, byte[] content, string contentType = "application/octet-stream")
        {
            return new EmailAttachment
            {
                FileName = fileName,
                Content = content,
                ContentType = contentType
            };
        }
    }
}