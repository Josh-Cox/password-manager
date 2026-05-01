using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Services
{
    public class ApiVaultStore : IVaultStore
    {
        private readonly HttpClient _client;

        public ApiVaultStore(HttpClient client)
        {
            _client = client;
        }

        public async Task<byte[]> ReadAllAsync()
        {
            var response = await _client.GetAsync("/vault");
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task WriteAllAsync(byte[] data)
        {
            var content = new ByteArrayContent(data);
            await _client.PostAsync("/vault", content);
        }

        public async Task<bool> ExistsAsync()
        {
            var response = await _client.GetAsync("/vault");
            return response.IsSuccessStatusCode;
        }
    }
}
