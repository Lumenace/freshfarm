using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace freshfarm.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration configuration)
        {
            // ✅ Securely load encryption key from appsettings.json
            string base64Key = configuration["EncryptionSettings:EncryptionKey"];
            if (string.IsNullOrEmpty(base64Key))
                throw new InvalidOperationException("Encryption key is missing in configuration.");

            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption key must be a 32-byte Base64 string.");
        }

        public byte[] Encrypt(string data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // ✅ Generates a new IV for each encryption
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedData = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);

            // ✅ Return IV + encrypted data
            return iv.Concat(encryptedData).ToArray();
        }

        public string Decrypt(byte[] encryptedData)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            var iv = encryptedData.Take(16).ToArray();
            var cipherText = encryptedData.Skip(16).ToArray();

            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] decryptedData = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
