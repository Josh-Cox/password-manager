using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Services
{
    public class VaultFormatCodec
    {
        private const int HeaderSize = 4;
        private const int SaltSize = 16;

        private static readonly byte[] Header = Encoding.ASCII.GetBytes("PMGR");

        public (byte[] Salt, byte[] EncryptedData) Read(byte[] fileBytes)
        {
            const int MinimumEncryptedSize = 12 + 16; // nonce + tag
            const int MinimumFileSize =
                HeaderSize + SaltSize + MinimumEncryptedSize;

            if (fileBytes == null || fileBytes.Length < MinimumFileSize)
            {
                throw new Exception(
                    $"Vault file too small. Length: {fileBytes?.Length ?? 0}");
            }

            int offset = 0;

            byte[] header = fileBytes[..HeaderSize];
            offset += HeaderSize;

            if (!header.SequenceEqual(Header))
                throw new Exception("Invalid vault format.");

            byte[] salt = fileBytes[offset..(offset + SaltSize)];
            offset += SaltSize;

            byte[] encrypted = fileBytes[offset..];

            if (encrypted.Length < MinimumEncryptedSize)
            {
                throw new Exception(
                    $"Encrypted payload too small. Length: {encrypted.Length}");
            }

            return (salt, encrypted);
        }

        public byte[] Write(byte[] salt, byte[] encryptedData)
        {
            byte[] full = new byte[HeaderSize + salt.Length + encryptedData.Length];

            int offset = 0;

            Buffer.BlockCopy(Header, 0, full, offset, HeaderSize);
            offset += HeaderSize;

            Buffer.BlockCopy(salt, 0, full, offset, salt.Length);
            offset += salt.Length;

            Buffer.BlockCopy(encryptedData, 0, full, offset, encryptedData.Length);

            return full;
        }
    }
}
