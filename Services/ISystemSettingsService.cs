namespace WebMTTQ.Services
{
    /// <summary>
    /// Service interface for managing system settings stored in CauHinhHeThong table.
    /// Provides methods for reading/writing plain and encrypted configuration values.
    /// </summary>
    public interface ISystemSettingsService
    {
        /// <summary>
        /// Gets a plain text configuration value by key.
        /// </summary>
        string GetValue(string key);

        /// <summary>
        /// Gets a plain text configuration value by key (async).
        /// </summary>
        Task<string> GetValueAsync(string key);

        /// <summary>
        /// Sets a plain text configuration value.
        /// Creates the key if it doesn't exist, updates if it does.
        /// </summary>
        void SetValue(string key, string? value, string? description = null);

        /// <summary>
        /// Sets a plain text configuration value (async).
        /// </summary>
        Task SetValueAsync(string key, string? value, string? description = null);

        /// <summary>
        /// Gets an encrypted configuration value by key.
        /// Returns decrypted value.
        /// </summary>
        string GetEncryptedValue(string key);

        /// <summary>
        /// Gets an encrypted configuration value by key (async).
        /// Returns decrypted value.
        /// </summary>
        Task<string> GetEncryptedValueAsync(string key);

        /// <summary>
        /// Sets an encrypted configuration value by key.
        /// Encrypts the value before storing.
        /// </summary>
        void SetEncryptedValue(string key, string? value, string? description = null);

        /// <summary>
        /// Sets an encrypted configuration value by key (async).
        /// Encrypts the value before storing.
        /// </summary>
        Task SetEncryptedValueAsync(string key, string? value, string? description = null);

        /// <summary>
        /// Checks if a configuration key exists.
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Checks if a configuration key exists (async).
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets a boolean configuration value.
        /// Returns true if the value is "1", "true", "yes", or "on" (case-insensitive).
        /// </summary>
        bool GetBoolean(string key);

        /// <summary>
        /// Gets a boolean configuration value (async).
        /// </summary>
        Task<bool> GetBooleanAsync(string key);

        /// <summary>
        /// Gets an integer configuration value.
        /// Returns 0 if key doesn't exist or value is not a valid integer.
        /// </summary>
        int GetInt(string key);

        /// <summary>
        /// Gets an integer configuration value (async).
        /// </summary>
        Task<int> GetIntAsync(string key);

        /// <summary>
        /// Gets a long configuration value (for file sizes).
        /// Returns 0 if key doesn't exist or value is not a valid long.
        /// </summary>
        long GetLong(string key);

        /// <summary>
        /// Gets a long configuration value (async).
        /// </summary>
        Task<long> GetLongAsync(string key);

        /// <summary>
        /// Gets all configuration entries as a dictionary.
        /// Encrypted values are returned as masked values ("**************").
        /// </summary>
        Task<Dictionary<string, string>> GetAllAsync();

        /// <summary>
        /// Gets the list of encrypted configuration keys.
        /// </summary>
        List<string> GetEncryptedKeys();
    }
}