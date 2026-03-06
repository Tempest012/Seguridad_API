using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Seguridad_API.Controllers;
using Seguridad_API.Data;
using Seguridad_API.DTOs;
using Seguridad_API.Services;
using Xunit;

namespace Seguridad_API.Tests;

public class AuthControllerTests
{
    private static IConfiguration ConfigJwt()
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["Jwt:Key"]).Returns("superclaveultrasegura_para_firmar_tokens_jwt_que_tiene_mas_de_64_bytes_1234567890");
        mock.Setup(c => c["Jwt:Issuer"]).Returns("tuapp");
        mock.Setup(c => c["Jwt:Audience"]).Returns("tuapp");
        return mock.Object;
    }

    private static AppDbContext CrearContextoEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Register_DevuelveBadRequest_SiUsuarioYaExiste()
    {
        await using var context = CrearContextoEnMemoria();
        var service = new UsuarioService(context);
        await service.CrearUsuario("existente", "pass");
        var controller = new AuthController(service, ConfigJwt());
        var dto = new UsuarioRegistroDTO { NombreUsuario = "existente", Password = "otro" };

        var result = await controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Register_DevuelveOk_SiUsuarioNoExiste()
    {
        await using var context = CrearContextoEnMemoria();
        var service = new UsuarioService(context);
        var controller = new AuthController(service, ConfigJwt());
        var dto = new UsuarioRegistroDTO { NombreUsuario = "nuevo", Password = "pass" };

        var result = await controller.Register(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_DevuelveBadRequest_SiUsuarioNoExiste()
    {
        await using var context = CrearContextoEnMemoria();
        var service = new UsuarioService(context);
        var controller = new AuthController(service, ConfigJwt());
        var dto = new UsuarioLoginDTO { NombreUsuario = "nadie", Password = "pass" };

        var result = await controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Login_DevuelveBadRequest_SiPasswordIncorrecta()
    {
        await using var context = CrearContextoEnMemoria();
        var service = new UsuarioService(context);
        await service.CrearUsuario("u", "correcta");
        var controller = new AuthController(service, ConfigJwt());
        var dto = new UsuarioLoginDTO { NombreUsuario = "u", Password = "mal" };

        var result = await controller.Login(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Login_DevuelveOkConToken_SiCredencialesCorrectas()
    {
        await using var context = CrearContextoEnMemoria();
        var service = new UsuarioService(context);
        await service.CrearUsuario("ok", "good");
        var controller = new AuthController(service, ConfigJwt());
        var dto = new UsuarioLoginDTO { NombreUsuario = "ok", Password = "good" };

        var result = await controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        var token = ok.Value.GetType().GetProperty("token")?.GetValue(ok.Value) as string;
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
