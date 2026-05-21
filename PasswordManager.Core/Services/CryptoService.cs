using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace PasswordManager.Core.Services
{
    public class CryptoService
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 600_000;
        private static readonly byte[] VaultMagic = Encoding.UTF8.GetBytes("PMGR1");

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        public byte[] DeriveKey(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );
        }

        public bool TryDecryptGcm(byte[] encryptedData, byte[] key, out byte[] plaintext)
        {
            plaintext = Array.Empty<byte>();

            if (encryptedData == null || encryptedData.Length < NonceSize + TagSize)
                return false;

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[encryptedData.Length - NonceSize - TagSize];

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedData, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedData, NonceSize + TagSize, ciphertext, 0, ciphertext.Length);

            byte[] temp = new byte[ciphertext.Length];

            using var aes = new AesGcm(key, TagSize);

            try
            {
                aes.Decrypt(nonce, ciphertext, tag, temp);
            }
            catch
            {
                return false; // wrong password OR corruption
            }

            // FAST VALIDATION (important)
            if (!temp.AsSpan(0, VaultMagic.Length).SequenceEqual(VaultMagic))
                return false;

            plaintext = temp[VaultMagic.Length..];
            return true;
        }

        public byte[] EncryptGcm(byte[] plaintext, byte[] key, byte[] nonce)
        {
            if (nonce.Length != NonceSize)
                throw new ArgumentException("Invalid nonce size");

            // ADD THIS
            byte[] payload = new byte[VaultMagic.Length + plaintext.Length];
            Buffer.BlockCopy(VaultMagic, 0, payload, 0, VaultMagic.Length);
            Buffer.BlockCopy(plaintext, 0, payload, VaultMagic.Length, plaintext.Length);

            byte[] ciphertext = new byte[payload.Length];
            byte[] tag = new byte[TagSize];

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, payload, ciphertext, tag);

            byte[] result = new byte[NonceSize + TagSize + ciphertext.Length];

            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

            return result;
        }

    }
}
