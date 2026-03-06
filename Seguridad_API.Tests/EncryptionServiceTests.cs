using Microsoft.Extensions.Configuration;
using Moq;
using Seguridad_API.Services;
using Xunit;

namespace Seguridad_API.Tests;

// Pruebas de caja blanca con xunit, no use bd lo hice con configuración en memoriaa (clave de 32 caracteres)

public class EncryptionServiceTests
{
    private static IConfiguration ConfigConClave(string clave)
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["Encryption:Key"]).Returns(clave);
        return mock.Object;
    }

    private const string Clave32 = "12345678901234567890123456789012";

    [Fact]
    public void Constructor_LanzaSiLaClaveNoTiene32Caracteres()
    {
        var config = ConfigConClave("solo20caracteres!!");

        Assert.Throws<Exception>(() => new EncryptionService(config));
    }

    [Fact]
    public void Encrypt_Decrypt_TextoVuelveIgual()
    {
        var config = ConfigConClave(Clave32);
        var service = new EncryptionService(config);
        const string texto = "Hola mundo secreto 123";

        var encrypted = service.Encrypt(texto);
        var decrypted = service.Decrypt(encrypted);

        Assert.NotEqual(texto, encrypted);
        Assert.Equal(texto, decrypted);
    }

    [Fact]
    public void Encrypt_GeneraResultadosDistintos_PorIVAleatorio()
    {
        var config = ConfigConClave(Clave32);
        var service = new EncryptionService(config);
        const string texto = "mismo texto";

        var e1 = service.Encrypt(texto);
        var e2 = service.Encrypt(texto);

        Assert.NotEqual(e1, e2);
        Assert.Equal(texto, service.Decrypt(e1));
        Assert.Equal(texto, service.Decrypt(e2));
    }

    [Fact]
    public void Decrypt_TextoVacío_DevuelveVacío()
    {
        var config = ConfigConClave(Clave32);
        var service = new EncryptionService(config);

        var encrypted = service.Encrypt("");
        var decrypted = service.Decrypt(encrypted);

        Assert.Equal("", decrypted);
    }
}
