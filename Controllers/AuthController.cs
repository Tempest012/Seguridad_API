using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Seguridad_API.Data;
using Seguridad_API.DTOs;
using Seguridad_API.Models;
using Seguridad_API.Services; // <- asegurarse
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.NombreUsuario)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
