using Microsoft.AspNetCore.Mvc;
using PasswordManager.API.Services;

namespace PasswordManager.API.Controllers
{
    [ApiController]
    [Route("vault")]
    public class VaultController : ControllerBase
    {
        private readonly VaultStorageService _storage;

        public VaultController(VaultStorageService storage)
        {
            _storage = storage;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetVault(string userId)
        {
            var bytes = await _storage.GetVaultAsync(userId);

            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream");
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SaveVault(string userId)
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);

            await _storage.SaveVaultAsync(userId, ms.ToArray());

            return Ok();
        }
    }
}