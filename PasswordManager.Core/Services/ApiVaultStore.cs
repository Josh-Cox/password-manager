using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/octet-stream");

            var response =
                await _client.PostAsync($"/vault/{userId}", content);

            var text = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"UPLOAD STATUS: {response.StatusCode}");
            Debug.WriteLine($"UPLOAD RESPONSE: {text}");

            response.EnsureSuccessStatusCode();
        }

        //public async Task<bool> ExistsAsync(string userId)
        //{
        //    var response = await _client.GetAsync($"/vault/{userId}");
        //    return response.IsSuccessStatusCode;
        //}
    }
}
