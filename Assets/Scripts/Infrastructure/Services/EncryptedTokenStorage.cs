using System;
using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Security;
using UnityEngine;

namespace Fireclicks.Infrastructure.Services
{
    public sealed class EncryptedTokenStorage
    {
        private const string PlayerPrefsKey = "FC_ENCRYPTED_TOKEN";
        private string _encryptedToken = string.Empty;
        private bool _initialized;

        public static EncryptedTokenStorage Instance { get; private set; }

        public EncryptedTokenStorage()
        {
            Instance = this;
        }

        public UniTask InitializeAsync()
        {
            if (_initialized)
                return UniTask.CompletedTask;

            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                _encryptedToken = PlayerPrefs.GetString(PlayerPrefsKey);
            }
            else
            {
                _encryptedToken = GenerateAndStoreNewToken();
            }

            _initialized = true;
            return UniTask.CompletedTask;
        }

        public string GetEncryptedToken()
        {
            if (!_initialized)
                throw new InvalidOperationException("Token storage is not initialized.");

            return _encryptedToken;
        }

        public void ForceRefreshToken()
        {
            _encryptedToken = GenerateAndStoreNewToken();
        }

        private static string GenerateAndStoreNewToken()
        {
            string rawToken = Guid.NewGuid().ToString("N");
            string encryptedToken = TokenEncryption.Encrypt(rawToken);
            PlayerPrefs.SetString(PlayerPrefsKey, encryptedToken);
            PlayerPrefs.Save();
            return encryptedToken;
        }
    }
}
