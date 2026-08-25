using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Phase 5 - Security response headers middleware.
    /// Adds hardening headers to every response (HTML, static files, and errors).
    ///
    /// The Content-Security-Policy reflects the application's real, working resources:
    ///   - inline &lt;style&gt;/&lt;script&gt; (auth + admin layouts, upstream admin blocks),
    ///   - Google Fonts (fonts.googleapis.com / fonts.gstatic.com),
    ///   - Cloudflare CDN (Font Awesome + jQuery used on auth pages),
    ///   - media embeds (YouTube / Vimeo / Facebook iframes produced by the rich-text editor),
    ///   - uploaded images (same-origin 'self', plus data:/https:/blob: for legacy base64 & rich text).
    /// 'unsafe-inline' is required for script/style because the existing views ship inline
    /// JS/CSS blocks; a stricter nonce/hash-based CSP would require refactoring those views and is
    /// documented as a recommended follow-up in SECURITY_AUDIT_REPORT.txt (Phase 5, remaining risks).
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["Permissions-Policy"] = "camera=(), microphone=(), payment=(), usb=()";
            headers["Content-Security-Policy"] = BuildContentSecurityPolicy();

            // Strict-Transport-Security is already emitted by app.UseHsts() (non-development).
            await _next(context);
        }

        private static string BuildContentSecurityPolicy()
        {
            return string.Join("; ",
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com",
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com",
                "font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com https://cdnjs.cloudflare.com data:",
                "img-src 'self' data: https: blob:",
                "connect-src 'self'",
                "frame-src 'self' https://www.youtube.com https://youtube.com https://youtube-nocookie.com https://youtu.be https://vimeo.com https://player.vimeo.com https://www.facebook.com https://www.facebook.net https://facebook.com https://facebook.net",
                "object-src 'none'",
                "base-uri 'self'",
                "form-action 'self'",
                "frame-ancestors 'self'");
        }
    }

    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
            => app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}