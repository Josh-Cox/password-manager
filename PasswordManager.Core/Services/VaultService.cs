using PasswordManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace PasswordManager.Core.Services
{
    public class VaultService
    {
        private readonly CryptoService _crypto;
        private readonly IVaultStore _store;
        private readonly VaultFormatCodec _codec;

        // define file format
        private const int HeaderSize = 4; // "PMGR"
        private const int SaltSize = 16; // 128-bit salt
        private const int NonceSize = 12;

        public VaultService(CryptoService crypto, IVaultStore store, VaultFormatCodec codec)
        {
        
            _crypto = crypto;
            _store = store;
            _codec = codec;
        }

        private static readonly byte[] Header = Encoding.ASCII.GetBytes("PMGR");


        public async Task SaveAsync(string userID, VaultSession session)
        {
            string json = JsonSerializer.Serialize(session.Entries);

            byte[] key = _crypto.DeriveKey(session.MasterPassword, session.Salt);

            byte[] nonce = RandomNumberGenerator.GetBytes(12);

            byte[] plaintext = Encoding.UTF8.GetBytes(json);

            byte[] encrypted = _crypto.EncryptGcm(plaintext, key, nonce);

            // create fullBytes = header + salt + encrypted

            byte[] fullBytes = _codec.Write(session.Salt, encrypted);

            // write to file
            await _store.WriteAllAsync(userID, fullBytes);
        }

        public async Task<VaultSession> LoadAsync(string userID, string masterPassword)
        {

            if (!await _store.ExistsAsync(userID))
            {

                var session = new VaultSession
                {
                    MasterPassword = masterPassword,
                    Salt = _crypto.GenerateSalt(),
                    Entries = new ObservableCollection<PasswordEntry>()
                };

                // save it immediately
                await SaveAsync(userID, session);

                return session;
            }

            byte[] fileBytes = await _store.ReadAllAsync(userID);

            var (salt, encrypted) = _codec.Read(fileBytes);

            byte[] key = _crypto.DeriveKey(masterPassword, salt);

            try
            {
                byte[] decryptedBytes = _crypto.DecryptGcm(encrypted, key);

                string json = Encoding.UTF8.GetString(decryptedBytes);

                var entries = JsonSerializer.Deserialize<ObservableCollection<PasswordEntry>>(json)
                       ?? new ObservableCollection<PasswordEntry>();

                return new VaultSession
                {
                    MasterPassword = masterPassword,
                    Salt = salt,
                    Entries = entries
                };
            }
            catch
            {
                throw new Exception("Invalid master password or corrupted file.");
            }
        }

    }
}
