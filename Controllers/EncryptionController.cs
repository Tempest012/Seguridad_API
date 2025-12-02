using Microsoft.AspNetCore.Mvc;
using Seguridad_API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Seguridad_API.Controllers
{
    [Route("api/crypto")]
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
        public IActionResult Encrypt([FromBody] EncryptRequest request)
        {
            var encrypted = _encryptService.Encrypt(request.Texto);
            return Ok(new { original = request.Texto, encrypted });
        }

        [Authorize]
        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] DecryptRequest request)
        {
            var decrypted = _encryptService.Decrypt(request.TextoEncriptado);
            return Ok(new { encrypted = request.TextoEncriptado, decrypted });
        }
    }

    public class EncryptRequest
    {
        public string Texto { get; set; }
    }

    public class DecryptRequest
    {
        public string TextoEncriptado { get; set; }
    }
}
