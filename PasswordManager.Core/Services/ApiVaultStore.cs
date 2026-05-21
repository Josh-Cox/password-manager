namespace PasswordManager.Core.Services
{
    public class ApiVaultStore : IVaultStore
    {
        private readonly HttpClient _client;

        public ApiVaultStore(HttpClient client)
        {
            _client = client;
        }

        public async Task<bool> ExistsAsync(string userId)
        {
            using var response = await _client.GetAsync("/vault/me/exists");

            System.Diagnostics.Debug.WriteLine($"HTTP Response: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<byte[]?> ReadAllAsync(string userId)
        {
            var response = await _client.GetAsync("/vault/me");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task WriteAllAsync(string userId, byte[] data)
        {
            var content = new ByteArrayContent(data);

            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/octet-stream");

            var response = await _client.PostAsync("/vault/me", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string userId)
        {
            var response = await _client.DeleteAsync("/vault/me");
            response.EnsureSuccessStatusCode();
        }
    }
}
