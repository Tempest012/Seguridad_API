using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Seguridad_API.Controllers;
using Seguridad_API.Services;
using Xunit;

namespace Seguridad_API.Tests;

//Pruebas del EncryptionController.
//Incluye un caso que debe fallar hasta que se valide la entrada (Texto null).
public class EncryptionControllerTests
{
    private const string Clave32 = "12345678901234567890123456789012";

    private static EncryptionService CrearEncryptionService()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Encryption:Key"]).Returns(Clave32);
        return new EncryptionService(mockConfig.Object);
    }

    // Seguridad/robustez: no se debe llamar al servicio con texto null.
    // El controller debe validar y devolver BadRequest.
    // Falla hasta que EncryptionController valide request.Texto y devuelva 400.
    [Fact]
    public void Encrypt_ConTextoNull_DevuelveBadRequest()
    {
        var service = CrearEncryptionService();
        var controller = new EncryptionController(service);
        var request = new EncryptRequest { Texto = null! };

        var result = controller.Encrypt(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
