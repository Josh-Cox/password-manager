using PasswordManager.Core.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordManager.Core.Services
{
    public class VaultService
    {
        private readonly CryptoService _crypto;
        private readonly IVaultStore _store;
        private readonly VaultFormatCodec _codec;

        public VaultService(CryptoService crypto, IVaultStore store, VaultFormatCodec codec)
        {
            _crypto = crypto;
            _store = store;
            _codec = codec;
        }

        public Task<bool> VaultExistsAsync(string userID) =>
            _store.ExistsAsync(userID);

        // Creates a brand-new vault. Caller is responsible for having validated the password first.
        public async Task<VaultSession> CreateVaultAsync(string userID, string masterPassword)
        {
            byte[] salt = _crypto.GenerateSalt();
            byte[] key = await Task.Run(() => _crypto.DeriveKey(masterPassword, salt));

            var session = new VaultSession
            {
                MasterPassword = masterPassword,
                Salt = salt,
                CachedKey = key,
                Entries = new ObservableCollection<PasswordEntry>()
            };

            await SaveAsync(userID, session);
            return session;
        }

        // Unlocks an existing vault. Throws VaultNotFoundException if no vault exists.
        // Returns null if the master password is wrong.
        public async Task<VaultSession?> LoadAsync(string userID, string masterPassword)
        {
            byte[]? fileBytes = await _store.ReadAllAsync(userID);

            if (fileBytes == null || fileBytes.Length == 0)
                throw new VaultNotFoundException();

            byte[] salt;
            byte[] encrypted;

            try
            {
                (salt, encrypted) = _codec.Read(fileBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Vault file is corrupted or invalid format.", ex);
            }

            byte[] key = await Task.Run(() => _crypto.DeriveKey(masterPassword, salt));

            if (!_crypto.TryDecryptGcm(encrypted, key, out var decrypted))
                return null;

            string json = Encoding.UTF8.GetString(decrypted);

            var entries =
                JsonSerializer.Deserialize(json, VaultJsonContext.Default.ObservableCollectionPasswordEntry)
                ?? new ObservableCollection<PasswordEntry>();

            return new VaultSession
            {
                MasterPassword = masterPassword,
                Salt = salt,
                CachedKey = key,
                Entries = entries
            };
        }

        public async Task SaveAsync(string userID, VaultSession session)
        {
            string json = JsonSerializer.Serialize(session.Entries, VaultJsonContext.Default.ObservableCollectionPasswordEntry);

            byte[] key = session.CachedKey
                ?? await Task.Run(() => _crypto.DeriveKey(session.MasterPassword, session.Salt));

            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] plaintext = Encoding.UTF8.GetBytes(json);
            byte[] encrypted = _crypto.EncryptGcm(plaintext, key, nonce);
            byte[] fullBytes = _codec.Write(session.Salt, encrypted);

            await _store.WriteAllAsync(userID, fullBytes);
        }
    }
}
