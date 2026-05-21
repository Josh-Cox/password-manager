using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Security.Claims;

namespace PasswordManager.API.Controllers
{
    [ApiController]
    [Route("account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly GraphServiceClient _graph;

        public AccountController(GraphServiceClient graph)
        {
            _graph = graph;
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (string.IsNullOrWhiteSpace(userId))
                    return Forbid();

                var user = await _graph.Users[userId].GetAsync();

                await _graph.Users[userId].DeleteAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return Content(ex.ToString(), "text/plain");
            }
        }

        private string? GetCurrentUserId() =>
            User.FindFirstValue("oid")
            ?? User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
