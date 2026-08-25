using System.Security.Cryptography;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Helper class để hash và verify mật khẩu.
    /// Dùng PBKDF2-HMAC-SHA256 (Rfc2898DeriveBytes).
    ///
    /// FORMAT HIỂN (format courant):
    ///     {iterations}:{salt_hex}:{hash_hex}
    ///     ex: 100000:{salt_hex}:{hash_hex}
    ///
    /// FORMAT CŪ (legacy, 2 parties): {salt_hex}:{hash_hex}
    ///     Créé à l'origine avec 10.000 iterations, sans compteur.
    ///     Toujours vérifiable via LegacyIterations = 10.000.
    ///     Re-hashé au login (AuthController) vers CurrentIterations = 100.000.
    ///
    /// Aucun plaintext n'est jamais accepté ni comparé.
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit

        /// <summary>Itérations utilisées pour tous les nouveaux mots de passe.</summary>
        public const int CurrentIterations = 100000;

        /// <summary>Itérations des hashs créés avant la mise à niveau.</summary>
        private const int LegacyIterations = 10000;

        /// <summary>
        /// Hash un nouveau mot de passe avec PBKDF2-HMAC-SHA256,
        /// CurrentIterations (100.000) et un salt aléatoire cryptographiquement sûr.
        /// Format: {iterations}:{salt_hex}:{hash_hex}
        /// </summary>
        public static string HashPassword(string password)
        {
            return HashPassword(password, CurrentIterations);
        }

        private static string HashPassword(string password, int iterations)
        {
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{iterations}:{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
        }

        /// <summary>
        /// Verify mật khẩu nhập vào avec le hash stocké. Hỗ trợ:
        ///   - Hash courant (3 parties, avec compteur) → PBKDF2 avec ce compteur.
        ///   - Hash cũ (2 parties, sans compteur) → PBKDF2 avec LegacyIterations (10.000).
        ///   - Toute autre valeur (invalid, null, plaintext) → false.
        /// Le plaintext n'est jamais comparé.
        /// </summary>
        public static bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            try
            {
                int iterations;
                byte[] salt;
                byte[] expectedHash;

                var parts = storedHash.Split(':');
                if (parts.Length == 3)
                {
                    // Format courant {iterations}:{salt_hex}:{hash_hex}
                    if (!int.TryParse(parts[0], out iterations) || iterations <= 0) return false;
                    salt = Convert.FromHexString(parts[1]);
                    expectedHash = Convert.FromHexString(parts[2]);
                }
                else if (parts.Length == 2)
                {
                    // Format cũ sans compteur → hash créé avec 10.000 iterations.
                    iterations = LegacyIterations;
                    salt = Convert.FromHexString(parts[0]);
                    expectedHash = Convert.FromHexString(parts[1]);
                }
                else
                {
                    // Format invalide (plaintext ou erroné inclus) → échec, jamais comparé.
                    return false;
                }

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// True si le hash stocké utilise des paramètres anciens
        /// (format legacy sans compteur, ou compteur != CurrentIterations).
        /// L'appelant doit vérifier que le mot de passe fourni est correct,
        /// puis appeler HashPassword pour produire un nouveau hash à CurrentIterations
        /// et le sauvegarder (re-hash automatique au login).
        /// </summary>
        public static bool NeedsRehash(string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            var parts = storedHash.Split(':');

            // Format legacy {salt_hex}:{hash_hex} → rehash nécessaire.
            if (parts.Length == 2) return true;

            // Format courant: rehash nécessaire si le compteur != CurrentIterations.
            if (parts.Length == 3)
            {
                int iterations;
                if (!int.TryParse(parts[0], out iterations) || iterations <= 0) return false;
                return iterations != CurrentIterations;
            }

            // Format invalide → aucune valeur utilisable; le verify échouera de toute façon.
            return false;
        }
    }
}