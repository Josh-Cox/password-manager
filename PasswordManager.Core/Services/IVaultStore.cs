using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Services
{
    public interface IVaultStore
    {
        Task<byte[]> ReadAllAsync();
        Task WriteAllAsync(byte[] data);
        Task<bool> ExistsAsync();
    }
}
