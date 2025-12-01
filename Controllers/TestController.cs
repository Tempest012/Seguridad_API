using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Seguridad_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // 🔒 Este endpoint requiere un JWT válido
        [Authorize]
        [HttpGet("protegido")]
        public IActionResult GetProtegido()
        {
            return Ok(new
            {
                mensaje = "Acceso correcto, tu token es válido",
                usuario = User.Identity?.Name
            });
        }

        // 🌐 Endpoint público
        [AllowAnonymous]
        [HttpGet("publico")]
        public IActionResult GetPublico()
        {
            return Ok("Cualquiera puede ver esto.");
        }
    }
}
