namespace PasswordManager.Core.Services
{
    public class ApiAccountService
    {
        private readonly HttpClient _client;

        public ApiAccountService(HttpClient client)
        {
            _client = client;
        }

        public async Task DeleteAccountAsync(string userId)
        {
            var response = await _client.DeleteAsync("/account/me");

            var responseText = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("ACCOUNT DELETE RESPONSE:");
            System.Diagnostics.Debug.WriteLine(responseText);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"HTTP {(int)response.StatusCode} {response.StatusCode}\n\n{responseText}");
            }
        }
    }
}
