using System.Security.Cryptography;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Helper class để hash và verify mật khẩu.
    /// Dùng PBKDF2 (Rfc2898DeriveBytes) - chuẩn bảo mật cao.
    /// Format lưu trữ: {salt_hex}:{hash_hex}
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 10000;

        /// <summary>
        /// Hash mật khẩu với salt ngẫu nhiên.
        /// </summary>
        public static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
        }

        /// <summary>
        /// Verify mật khẩu nhập vào với hash đã lưu.
        /// </summary>
        public static bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // Hỗ trợ backward compatibility cho mật khẩu plain text cũ
            if (!storedHash.Contains(':'))
            {
                return storedHash == password;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            try
            {
                byte[] salt = Convert.FromHexString(parts[0]);
                byte[] expectedHash = Convert.FromHexString(parts[1]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}