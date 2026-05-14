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

        public VaultService(CryptoService crypto, IVaultStore store, VaultFormatCodec codec)
        {

            _crypto = crypto;
            _store = store;
            _codec = codec;
        }

        public async Task SaveAsync(string userID, VaultSession session)
        {
            string json = JsonSerializer.Serialize(session.Entries);

            byte[] key = _crypto.DeriveKey(session.MasterPassword, session.Salt);
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] plaintext = Encoding.UTF8.GetBytes(json);

            byte[] encrypted = _crypto.EncryptGcm(plaintext, key, nonce);

            byte[] fullBytes = _codec.Write(session.Salt, encrypted);

            await _store.WriteAllAsync(userID, fullBytes);
        }

        public async Task<VaultSession> LoadAsync(string userID, string masterPassword)
        {
            byte[]? fileBytes = await _store.ReadAllAsync(userID);

            // CASE 1: No vault exists → create new
            if (fileBytes == null || fileBytes.Length == 0)
            {
                var newSession = new VaultSession
                {
                    MasterPassword = masterPassword,
                    Salt = _crypto.GenerateSalt(),
                    Entries = new ObservableCollection<PasswordEntry>()
                };

                await SaveAsync(userID, newSession);
                return newSession;
            }

            byte[] salt;
            byte[] encrypted;

            // STEP 1: Decode file structure (header + salt + encrypted blob)
            try
            {
                (salt, encrypted) = _codec.Read(fileBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Vault file is corrupted or invalid format.",
                    ex);
            }

            // STEP 2: Derive key (cheap operation)
            byte[] key = _crypto.DeriveKey(masterPassword, salt);

            // STEP 3: Decrypt vault (expensive + failure-prone
            if (!_crypto.TryDecryptGcm(encrypted, key, out var decrypted))
            {
                return null; // WRONG PASSWORD (no exception)
            }

            // STEP 4: Deserialize
            string json = Encoding.UTF8.GetString(decrypted);

            var entries =
                JsonSerializer.Deserialize<ObservableCollection<PasswordEntry>>(json)
                ?? new ObservableCollection<PasswordEntry>();

            return new VaultSession
            {
                MasterPassword = masterPassword,
                Salt = salt,
                Entries = entries
            };
        }

    //public async Task SaveAsync(string userID, VaultSession session)
    //{
    //    string json = JsonSerializer.Serialize(session.Entries);

    //    byte[] key = await Task.Run(() =>
    //        _crypto.DeriveKey(session.MasterPassword, session.Salt));

    //    byte[] nonce = RandomNumberGenerator.GetBytes(12);

    //    byte[] plaintext = Encoding.UTF8.GetBytes(json);

    //    byte[] encrypted = await Task.Run(() =>
    //        _crypto.EncryptGcm(plaintext, key, nonce));

    //    // create fullBytes = header + salt + encrypted

    //    byte[] fullBytes = _codec.Write(session.Salt, encrypted);

    //    // write to file
    //    await _store.WriteAllAsync(userID, fullBytes);
    //}

    //public async Task<VaultSession> LoadAsync(string userID, string masterPassword)
    //{

    //    byte[]? fileBytes = await _store.ReadAllAsync(userID);

    //    if (fileBytes == null)
    //    {
    //        var session = new VaultSession
    //        {
    //            MasterPassword = masterPassword,
    //            Salt = _crypto.GenerateSalt(),
    //            Entries = new ObservableCollection<PasswordEntry>()
    //        };

    //        await SaveAsync(userID, session);

    //        return session;
    //    }


    //    var (salt, encrypted) = _codec.Read(fileBytes);

    //    byte[] key = await Task.Run(() =>
    //        _crypto.DeriveKey(masterPassword, salt));

    //    try
    //    {
    //        byte[] decryptedBytes = await Task.Run(() =>
    //            _crypto.DecryptGcm(encrypted, key));

    //        string json = Encoding.UTF8.GetString(decryptedBytes);

    //        var entries = JsonSerializer.Deserialize<ObservableCollection<PasswordEntry>>(json)
    //               ?? new ObservableCollection<PasswordEntry>();

    //        return new VaultSession
    //        {
    //            MasterPassword = masterPassword,
    //            Salt = salt,
    //            Entries = entries
    //        };
    //    }
    //    catch (CryptographicException ex)
    //    {
    //        throw new Exception(
    //"Invalid master password or corrupted file.",
    //ex);
    //    }
    //}


}
}
