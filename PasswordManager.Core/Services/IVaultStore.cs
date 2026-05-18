using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Services
{
    public interface IVaultStore
    {
        Task<bool> ExistsAsync(string userId);
        Task<byte[]?> ReadAllAsync(string userId);
        Task WriteAllAsync(string userId, byte[] data);
    }
}
