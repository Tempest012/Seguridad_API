using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Seguridad_API.Data;
using Seguridad_API.Models;
using Seguridad_API.Services;
using Xunit;

namespace Seguridad_API.Tests;

public class UsuarioServiceTests
{
    private static AppDbContext CrearContextoEnMemoria(string nombreDb)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: nombreDb)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UsuarioExiste_DevuelveFalse_SiNoHayUsuarios()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        var service = new UsuarioService(context);

        var existe = await service.UsuarioExiste("nadie");

        Assert.False(existe);
    }

    [Fact]
    public async Task UsuarioExiste_DevuelveTrue_SiElUsuarioExiste()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        context.Usuarios.Add(new Usuario { Id = 1, NombreUsuario = "test", PasswordHash = "hash" });
        await context.SaveChangesAsync();
        var service = new UsuarioService(context);

        var existe = await service.UsuarioExiste("test");

        Assert.True(existe);
    }

    [Fact]
    public async Task GetUsuario_DevuelveNull_SiNoExiste()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        var service = new UsuarioService(context);

        var user = await service.GetUsuario("inexistente");

        Assert.Null(user);
    }

    [Fact]
    public async Task GetUsuario_DevuelveUsuario_SiExiste()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        context.Usuarios.Add(new Usuario { Id = 1, NombreUsuario = "juan", PasswordHash = "abc" });
        await context.SaveChangesAsync();
        var service = new UsuarioService(context);

        var user = await service.GetUsuario("juan");

        Assert.NotNull(user);
        Assert.Equal("juan", user.NombreUsuario);
        Assert.Equal("abc", user.PasswordHash);
    }

    [Fact]
    public async Task CrearUsuario_GuardaUsuario_YVerificarPasswordFunciona()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        var service = new UsuarioService(context);
        const string password = "MiPassword123";

        var usuario = await service.CrearUsuario("nuevo", password);

        Assert.NotNull(usuario);
        Assert.Equal("nuevo", usuario.NombreUsuario);
        Assert.NotEqual(password, usuario.PasswordHash);
        Assert.True(service.VerificarPassword(password, usuario.PasswordHash));
        Assert.False(service.VerificarPassword("otra", usuario.PasswordHash));
    }

    [Fact]
    public async Task VerificarPassword_ConHashInvalido_NoDevuelveTrue()
    {
        await using var context = CrearContextoEnMemoria(Guid.NewGuid().ToString());
        var service = new UsuarioService(context);

        // BCrypt lanza SaltParseException con hash inválido; no devuelve false
        Assert.Throws<BCrypt.Net.SaltParseException>(() =>
            service.VerificarPassword("cualquiera", "no_es_bcrypt"));
    }
}
