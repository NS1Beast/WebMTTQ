using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Text.RegularExpressions;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Implementation of ISystemSettingsService.
    /// Uses IDataProtector for encrypting sensitive configuration values.
    /// All settings are stored in the CauHinhHeThong table.
    /// </summary>
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly DataMTTQContext _context;
        private readonly IDataProtector _protector;

        // List of keys that should be encrypted
        private static readonly List<string> _encryptedKeys = new();

        public SystemSettingsService(DataMTTQContext context, IDataProtectionProvider protectionProvider)
        {
            _context = context;
            _protector = protectionProvider.CreateProtector("WebMTTQ.SystemSettings");
        }

        // ============ PLAIN TEXT ============

        public string GetValue(string key)
        {
            return _context.CauHinhHeThongs
                .AsNoTracking()
                .FirstOrDefault(c => c.MaCauHinh == key)?.GiaTriCauHinh ?? string.Empty;
        }

        public async Task<string> GetValueAsync(string key)
        {
            var config = await _context.CauHinhHeThongs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaCauHinh == key);
            return config?.GiaTriCauHinh ?? string.Empty;
        }

        public void SetValue(string key, string? value, string? description = null)
        {
            SetValueInternal(key, value, description);
            _context.SaveChanges();
        }

        public async Task SetValueAsync(string key, string? value, string? description = null)
        {
            SetValueInternal(key, value, description);
            await _context.SaveChangesAsync();
        }

        // ============ ENCRYPTED ============

        public string GetEncryptedValue(string key)
        {
            var encryptedValue = _context.CauHinhHeThongs
                .AsNoTracking()
                .FirstOrDefault(c => c.MaCauHinh == key)?.GiaTriCauHinh;

            if (string.IsNullOrEmpty(encryptedValue))
                return string.Empty;

            try
            {
                return _protector.Unprotect(encryptedValue);
            }
            catch
            {
                // If decryption fails, the value may already be plain text (backward compatibility)
                return encryptedValue;
            }
        }

        public async Task<string> GetEncryptedValueAsync(string key)
        {
            var config = await _context.CauHinhHeThongs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaCauHinh == key);

            if (config == null || string.IsNullOrEmpty(config.GiaTriCauHinh))
                return string.Empty;

            try
            {
                return _protector.Unprotect(config.GiaTriCauHinh);
            }
            catch
            {
                // If decryption fails, the value may already be plain text (backward compatibility)
                return config.GiaTriCauHinh;
            }
        }

        public void SetEncryptedValue(string key, string? value, string? description = null)
        {
            var encryptedValue = string.IsNullOrEmpty(value) ? null : _protector.Protect(value);
            SetValueInternal(key, encryptedValue, description);
            _context.SaveChanges();
        }

        public async Task SetEncryptedValueAsync(string key, string? value, string? description = null)
        {
            var encryptedValue = string.IsNullOrEmpty(value) ? null : _protector.Protect(value);
            SetValueInternal(key, encryptedValue, description);
            await _context.SaveChangesAsync();
        }

        // ============ UTILITY ============

        public bool Exists(string key)
        {
            return _context.CauHinhHeThongs.Any(c => c.MaCauHinh == key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _context.CauHinhHeThongs.AnyAsync(c => c.MaCauHinh == key);
        }

        public bool GetBoolean(string key)
        {
            var value = GetValue(key);
            if (string.IsNullOrEmpty(value)) return false;
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> GetBooleanAsync(string key)
        {
            var value = await GetValueAsync(key);
            if (string.IsNullOrEmpty(value)) return false;
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        public int GetInt(string key)
        {
            var value = GetValue(key);
            if (int.TryParse(value, out var result)) return result;
            return 0;
        }

        public async Task<int> GetIntAsync(string key)
        {
            var value = await GetValueAsync(key);
            if (int.TryParse(value, out var result)) return result;
            return 0;
        }

        public long GetLong(string key)
        {
            var value = GetValue(key);
            if (long.TryParse(value, out var result)) return result;
            return 0;
        }

        public async Task<long> GetLongAsync(string key)
        {
            var value = await GetValueAsync(key);
            if (long.TryParse(value, out var result)) return result;
            return 0;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var configs = await _context.CauHinhHeThongs
                .AsNoTracking()
                .ToListAsync();

            var result = new Dictionary<string, string>();
            foreach (var config in configs)
            {
                if (config.MaCauHinh != null)
                {
                    // Mask encrypted values
                    if (_encryptedKeys.Contains(config.MaCauHinh) && !string.IsNullOrEmpty(config.GiaTriCauHinh))
                    {
                        result[config.MaCauHinh] = "**************";
                    }
                    else
                    {
                        result[config.MaCauHinh] = config.GiaTriCauHinh ?? string.Empty;
                    }
                }
            }
            return result;
        }

        public List<string> GetEncryptedKeys()
        {
            return new List<string>(_encryptedKeys);
        }

        // ============ PRIVATE HELPERS ============

        private void SetValueInternal(string key, string? value, string? description)
        {
            var config = _context.CauHinhHeThongs.FirstOrDefault(c => c.MaCauHinh == key);
            if (config == null)
            {
                _context.CauHinhHeThongs.Add(new CauHinhHeThong
                {
                    MaCauHinh = key,
                    GiaTriCauHinh = value ?? string.Empty,
                    MoTa = description ?? string.Empty
                });
            }
            else
            {
                config.GiaTriCauHinh = value ?? string.Empty;
                if (description != null)
                {
                    config.MoTa = description;
                }
            }
        }
    }
}