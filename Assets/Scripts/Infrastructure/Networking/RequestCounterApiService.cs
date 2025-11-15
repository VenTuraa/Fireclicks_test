using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Services;
using Fireclicks.Infrastructure;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Fireclicks.Infrastructure.Logging.Logger;

namespace Fireclicks.Infrastructure.Networking
{
    public sealed class RequestCounterApiService
    {
        private readonly EncryptedTokenStorage _tokenStorage;
        private readonly Logger _logger;

        public static RequestCounterApiService Instance { get; private set; }

        public RequestCounterApiService(EncryptedTokenStorage tokenStorage, Logger logger)
        {
            _tokenStorage = tokenStorage;
            _logger = logger;
            Instance = this;
        }

        public async UniTask<RequestCountResponse> SendRequestAsync(
            CancellationToken cancellationToken = default,
            bool allowTokenRefresh = true)
        {
            string token = _tokenStorage.GetEncryptedToken();
            var payload = new RequestCountRequest { token = token };
            string json = JsonUtility.ToJson(payload);
            string url = $"{GameConfig.ServerBaseUrl}/api/request-count";

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API request failed: {ex.Message}");
                throw;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler?.text;

                if (allowTokenRefresh &&
                    request.responseCode == 400 &&
                    !string.IsNullOrEmpty(responseBody) &&
                    responseBody.Contains("TokenInvalid"))
                {
                    _logger.Log("API reported invalid token. Regenerating token and retrying...");
                    _tokenStorage.ForceRefreshToken();
                    return await SendRequestAsync(cancellationToken, false);
                }

                string errorMessage = string.IsNullOrWhiteSpace(request.error) ? "Unknown error" : request.error;
                _logger.LogError($"API request error: {errorMessage}");
                throw new InvalidOperationException(errorMessage);
            }

            string responseText = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(responseText))
            {
                throw new InvalidOperationException("Empty response from server");
            }

            var response = JsonUtility.FromJson<RequestCountResponse>(responseText);
            if (response == null)
            {
                throw new InvalidOperationException("Failed to parse server response");
            }

            return response;
        }

        [Serializable]
        private struct RequestCountRequest
        {
            public string token;
        }

        [Serializable]
        public sealed class RequestCountResponse
        {
            public string token;
            public int requestCount;
        }
    }
}
