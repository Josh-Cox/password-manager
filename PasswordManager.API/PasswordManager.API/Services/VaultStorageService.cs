using Azure.Identity;
using Azure.Storage.Blobs;

namespace PasswordManager.API.Services
{
    public class VaultStorageService
    {
        private readonly BlobContainerClient _container;

        public VaultStorageService(string storageAccountUrl)
        {
            var credential = new DefaultAzureCredential();
            var client = new BlobServiceClient(new Uri(storageAccountUrl), credential);
            _container = client.GetBlobContainerClient("vaults");
            _container.CreateIfNotExists();
        }

        public async Task<bool> VaultExistsAsync(string userId)
        {
            var blob = _container.GetBlobClient($"{userId}.dat");
            return await blob.ExistsAsync();
        }

        public async Task<byte[]?> GetVaultAsync(string userId)
        {
            var blob = _container.GetBlobClient($"{userId}.dat");

            if (!await blob.ExistsAsync())
                return null;

            var response = await blob.DownloadContentAsync();
            return response.Value.Content.ToArray();
        }

        public async Task SaveVaultAsync(string userId, byte[] data)
        {
            var blob = _container.GetBlobClient($"{userId}.dat");

            using var ms = new MemoryStream(data);
            await blob.UploadAsync(ms, overwrite: true);
        }

        public async Task DeleteVaultAsync(string userId)
        {
            var blob = _container.GetBlobClient($"{userId}.dat");
            await blob.DeleteIfExistsAsync();
        }
    }
}