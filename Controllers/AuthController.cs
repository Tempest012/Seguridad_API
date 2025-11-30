using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seguridad_API.Data;
using Seguridad_API.DTOs;
using Seguridad_API.Models;
using Seguridad_API.Services; // <- asegurarse

namespace Seguridad_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly IConfiguration _config;

        public AuthController(UsuarioService usuarioService, IConfiguration config)
        {
            _usuarioService = usuarioService;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UsuarioRegistroDTO req)
        {
            if (await _usuarioService.UsuarioExiste(req.NombreUsuario))
                return BadRequest(new { message = "El usuario ya existe" });

            await _usuarioService.CrearUsuario(req.NombreUsuario, req.Password);
            return Ok(new { message = "Usuario registrado correctamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UsuarioLoginDTO req)
        {
            var user = await _usuarioService.GetUsuario(req.NombreUsuario);
            if (user == null) return BadRequest(new { message = "Usuario no encontrado" });

            if (!_usuarioService.VerificarPassword(req.Password, user.PasswordHash))
                return BadRequest(new { message = "Contraseña incorrecta" });

            // Aquí generas token (mismo CreateToken que ya tenías)
            string token = CreateToken(user);
            return Ok(new { token });
        }

        private string CreateToken(Usuario user)
        {
            var claims = new List<System.Security.Claims.Claim> {
                new(System.Security.Claims.ClaimTypes.Name, user.NombreUsuario)
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512Signature);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                claims: claims, expires: DateTime.Now.AddHours(5), signingCredentials: creds);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        // Endpoint protegido
        [Authorize]
        [HttpGet("perfil")]
        public ActionResult<string> Perfil()
        {
            var username = User.Identity?.Name;

            return Ok($"Usuario autenticado: {username}");
        }

    }
}
