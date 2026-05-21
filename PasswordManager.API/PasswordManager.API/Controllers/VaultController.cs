using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.API.Services;
using System.Security.Claims;

namespace PasswordManager.API.Controllers
{
    [ApiController]
    [Route("vault")]
    [Authorize]
    public class VaultController : ControllerBase
    {
        private readonly VaultStorageService _storage;

        public VaultController(VaultStorageService storage)
        {
            _storage = storage;
        }

        [HttpGet("{userId}/exists")]
        public async Task<IActionResult> CheckVaultExists(string userId)
        {
            if (!IsCurrentUser(userId))
                return Forbid();

            return await _storage.VaultExistsAsync(userId)
                ? Ok()
                : NotFound();
        }

        [HttpGet("me/exists")]
        public async Task<IActionResult> CheckCurrentUserVaultExists()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            return await _storage.VaultExistsAsync(userId)
                ? Ok()
                : NotFound();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetVault(string userId)
        {
            if (!IsCurrentUser(userId))
                return Forbid();

            var bytes = await _storage.GetVaultAsync(userId);

            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserVault()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            var bytes = await _storage.GetVaultAsync(userId);

            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream");
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> SaveVault(string userId)
        {
            if (!IsCurrentUser(userId))
                return Forbid();

            using var ms = new MemoryStream();

            await Request.Body.CopyToAsync(ms);

            var data = ms.ToArray();

            if (data.Length == 0)
                return BadRequest("Empty vault upload.");

            await _storage.SaveVaultAsync(userId, data);

            return Ok();
        }

        [HttpPost("me")]
        public async Task<IActionResult> SaveCurrentUserVault()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            using var ms = new MemoryStream();

            await Request.Body.CopyToAsync(ms);

            var data = ms.ToArray();

            if (data.Length == 0)
                return BadRequest("Empty vault upload.");

            await _storage.SaveVaultAsync(userId, data);

            return Ok();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUserVault()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Forbid();

            await _storage.DeleteVaultAsync(userId);
            return Ok();
        }

        private bool IsCurrentUser(string requestedUserId)
        {
            var authenticatedUserId = GetCurrentUserId();

            return string.Equals(
                authenticatedUserId,
                requestedUserId,
                StringComparison.OrdinalIgnoreCase);
        }

        private string? GetCurrentUserId() =>
            User.FindFirstValue("oid")
            ?? User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
