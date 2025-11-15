using System;
using System.Security.Cryptography;
using System.Text;

namespace Fireclicks.Infrastructure.Security
{
    public static class TokenEncryption
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("FireclicksSecretKey1234567890AB!");
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("FireclicksVector");

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentException("Value is empty", nameof(plainText));

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                throw new ArgumentException("Value is empty", nameof(cipherText));

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] buffer = Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
