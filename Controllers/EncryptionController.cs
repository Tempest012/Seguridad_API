using Microsoft.AspNetCore.Mvc;
using Seguridad_API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Seguridad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly EncryptionService _encryptService;

        public EncryptionController(EncryptionService encryptService)
        {
            _encryptService = encryptService;
        }

        [Authorize]
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] string text)
        {
            var encrypted = _encryptService.Encrypt(text);
            return Ok(new { original = text, encrypted });
        }

        [Authorize]
        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] string encryptedText)
        {
            var decrypted = _encryptService.Decrypt(encryptedText);
            return Ok(new { encrypted = encryptedText, decrypted });
        }
    }
}
