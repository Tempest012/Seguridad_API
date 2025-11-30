using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seguridad_API.Data;
using Seguridad_API.DTOs;
using Seguridad_API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Seguridad_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // -------------------------------
        //  REGISTRO
        // -------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(UsuarioRegistroDTO request)
        {
            // Verificar si ya existe
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario))
                return BadRequest("El usuario ya existe");

            // Convertir password a Hash seguro
            string passwordHash = HashPassword(request.Password);

            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario,
                PasswordHash = passwordHash
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente");
        }

        // -------------------------------
        //  LOGIN
        // -------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(UsuarioLoginDTO request)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);

            if (user == null)
                return BadRequest("Usuario no encontrado");

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return BadRequest("Contraseña incorrecta");

            string token = CreateToken(user);

            return Ok(new
            {
                mensaje = "Login correcto",
                token
            });
        }

        // -------------------------------
        //  GENERAR HASH
        // -------------------------------
        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string hash)
        {
            using var hmac = new HMACSHA256();
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(computed) == hash;
        }

        // -------------------------------
        //  CREAR JWT
        // -------------------------------
        private string CreateToken(Usuario user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.NombreUsuario)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
