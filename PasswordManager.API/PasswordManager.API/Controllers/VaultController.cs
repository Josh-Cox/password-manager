using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.API.Controllers
{
    [ApiController]
    [Route("vault")]
    public class VaultController : ControllerBase
    {

        [HttpGet]
        public IActionResult GetVault()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "vault.dat");
            if (!System.IO.File.Exists(path))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(path);

            return File(bytes, "application/octet-stream");
        }

        [HttpPost]
        public async Task<ActionResult> SaveVault()
        {
            var path = "vault.dat";

            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);

            var bytes = ms.ToArray();

            System.IO.File.WriteAllBytes(path, bytes);

            return Ok();
        }
    }


}
