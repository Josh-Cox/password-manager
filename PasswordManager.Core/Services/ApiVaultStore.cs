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

        public async Task<byte[]?> ReadAllAsync(string userId)
        {
            var response = await _client.GetAsync($"/vault/{userId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task WriteAllAsync(string userId, byte[] data)
        {
            var content = new ByteArrayContent(data);
            await _client.PostAsync($"/vault/{userId}", content);
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            var response = await _client.GetAsync($"/vault/{userId}");
            return response.IsSuccessStatusCode;
        }
    }
}
