namespace Seguridad_API.DTOs
{
    public class UsuarioRegistroDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UsuarioLoginDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
