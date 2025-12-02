using System.Security.Cryptography;
using System.Text;

namespace Seguridad_API.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration config)
        {
            _key = Encoding.UTF8.GetBytes(config["Encryption:Key"]);

            if (_key.Length != 32)
                throw new Exception("La llave debe tener exactamente 32 caracteres para AES-256.");
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // ⚠ Genera IV aleatorio

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var cipherBytes = ms.ToArray();

            // ✔ Devolver IV + Cipher en una sola cadena Base64
            var result = aes.IV.Concat(cipherBytes).ToArray();

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherTextBase64)
        {
            var fullCipher = Convert.FromBase64String(cipherTextBase64);

            // ✔ Recuperar IV (primeros 16 bytes)
            var iv = fullCipher.Take(16).ToArray();

            // ✔ Recuperar cipherText (resto)
            var cipher = fullCipher.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
