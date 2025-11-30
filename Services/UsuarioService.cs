using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Seguridad_API.Data;
using Seguridad_API.Models;

namespace Seguridad_API.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UsuarioExiste(string nombreUsuario)
        {
            return await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<Usuario?> GetUsuario(string nombreUsuario)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<Usuario> CrearUsuario(string nombreUsuario, string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                PasswordHash = hash
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public bool VerificarPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
