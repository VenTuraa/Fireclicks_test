using System;
using System.Security.Cryptography;
using System.Text;

namespace Fireclicks.Infrastructure.Security
{
    public static class TokenEncryption
    {
        private static readonly byte[] Key = DeriveKey("FireclicksTokenKeySeed2024");
        private static readonly byte[] Iv = DeriveIV("FireclicksTokenIVSeed2024");

        private static byte[] DeriveKey(string seed)
        {
            using var sha256 = SHA256.Create();
            byte[] seedBytes = Encoding.UTF8.GetBytes(seed);
            byte[] hash = sha256.ComputeHash(seedBytes);
            byte[] key = new byte[32];
            Array.Copy(hash, 0, key, 0, 32);
            return key;
        }

        private static byte[] DeriveIV(string seed)
        {
            using var sha256 = SHA256.Create();
            byte[] seedBytes = Encoding.UTF8.GetBytes(seed);
            byte[] hash = sha256.ComputeHash(seedBytes);
            byte[] iv = new byte[16];
            Array.Copy(hash, 16, iv, 0, 16);
            return iv;
        }

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
