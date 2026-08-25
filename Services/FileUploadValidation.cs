using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Server-side file upload validation helper (Phase 4 - File Upload Security).
    /// Validates the file extension against an explicit allow-list and verifies the
    /// file signature (magic bytes) so a file cannot impersonate a safe type while
    /// actually being executable/script content. The original client-supplied
    /// filename is never used as the storage name; callers store the file using a
    /// GUID plus the validated extension.
    /// </summary>
    public static class FileUploadValidator
    {
        public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        public static readonly string[] DocumentExtensions = { ".pdf", ".xlsx", ".xls", ".docx", ".doc" };
        public static readonly string[] SpreadsheetExtensions = { ".xlsx" };

        /// <summary>Returns true if the file is a real image matching its magic bytes.
        /// safeExtension is the validated lower-cased extension.</summary>
        public static bool IsValidImage(IFormFile? file, out string? safeExtension)
        {
            safeExtension = null;
            if (file == null || file.Length == 0) return false;

            string? ext = GetSafeExtension(file.FileName);
            if (ext == null || !AllowListContains(ImageExtensions, ext)) return false;
            if (!MatchesMagicNumber(file, ext)) return false;

            safeExtension = ext;
            return true;
        }

        /// <summary>PDF or Office document with matching magic bytes.</summary>
        public static bool IsValidDocument(IFormFile? file, out string? safeExtension)
        {
            safeExtension = null;
            if (file == null || file.Length == 0) return false;

            string? ext = GetSafeExtension(file.FileName);
            if (ext == null || !AllowListContains(DocumentExtensions, ext)) return false;
            if (!MatchesMagicNumber(file, ext)) return false;

            safeExtension = ext;
            return true;
        }

        public static bool IsValidSpreadsheet(IFormFile? file, out string? safeExtension)
        {
            safeExtension = null;
            if (file == null || file.Length == 0) return false;

            string? ext = GetSafeExtension(file.FileName);
            if (ext == null || !AllowListContains(SpreadsheetExtensions, ext)) return false;
            if (!MatchesMagicNumber(file, ext)) return false;

            safeExtension = ext;
            return true;
        }
/// <summary>
        /// Extracts a single extension from the filename, rejecting double extensions
        /// and embedded path separators. Returns the lower-cased extension including
        /// the leading dot (e.g. ".jpg"), or null if the name is dangerous/multi-part.
        /// </summary>
        public static string? GetSafeExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            string name = fileName.Replace('\\', '/');
            if (name.Contains("..")) return null; // reject path traversal

            name = Path.GetFileName(name);

            int lastDot = name.LastIndexOf('.');
            if (lastDot < 0 || lastDot == name.Length - 1) return null;

            string candidate = name.Substring(lastDot).ToLowerInvariant();
            if (candidate.IndexOf('.') != 0) return null; // reject multi-dot (double ext)
            if (candidate.Length < 2 || candidate.IndexOfAny(new[] { '/', '\\', ':' }) >= 0) return null;
            return candidate;
        }

        private static bool AllowListContains(string[] list, string ext)
        {
            foreach (var e in list)
                if (e.Equals(ext, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static bool MatchesMagicNumber(IFormFile file, string ext)
        {
            using var stream = file.OpenReadStream();
            byte[] header = new byte[12];
            int read = stream.Read(header, 0, header.Length);
            if (read <= 0) return false;

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                case ".png":
                    return read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E
                        && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
                case ".gif":
                    return read >= 4 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38;
                case ".webp":
                    return read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                        && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
                case ".bmp":
                    return read >= 2 && header[0] == 0x42 && header[1] == 0x4D;
                case ".ico":
                    return read >= 4 && header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00;
                case ".pdf":
                    return read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;
                case ".xlsx":
                case ".xls":
                case ".docx":
                case ".doc":
                    // ZIP-based Office documents start with PK\x03\x04 (or empty PK\x05\x06).
                    return read >= 4 && header[0] == 0x50 && header[1] == 0x4B
                        && (header[2] == 0x03 || header[2] == 0x05 || header[2] == 0x07) && header[3] == 0x04;
                default:
                    return false;
            }
        }
    }
}