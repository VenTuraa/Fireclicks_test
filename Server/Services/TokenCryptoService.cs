using System;
using System.Security.Cryptography;
using System.Text;

namespace FireclicksServer.Services;

public sealed class TokenCryptoService
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("FireclicksSecretKey1234567890AB!");
    private static readonly byte[] Iv = Encoding.UTF8.GetBytes("FireclicksVector");

    public string? TryDecryptToken(string encryptedToken)
    {
        if (string.IsNullOrWhiteSpace(encryptedToken))
            return null;

        try
        {
            byte[] buffer = Convert.FromBase64String(encryptedToken);
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return null;
        }
    }

    public string EncryptToken(string plainToken)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
            throw new ArgumentException("Token is empty", nameof(plainToken));

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] buffer = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainToken), 0, Encoding.UTF8.GetByteCount(plainToken));
        return Convert.ToBase64String(buffer);
    }
}
