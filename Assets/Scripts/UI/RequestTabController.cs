using System;
using Cysharp.Threading.Tasks;
using Fireclicks.Infrastructure.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fireclicks.UI
{
    public class RequestTabController : MonoBehaviour
    {
        [SerializeField] private Button _sendRequestButton;
        [SerializeField] private TextMeshProUGUI _requestCountText;
        [SerializeField] private TextMeshProUGUI _statusText;

        private bool _isSending;

        private void Awake()
        {
            if (_sendRequestButton)
            {
                _sendRequestButton.onClick.AddListener(OnSendRequestClicked);
            }

            UpdateCountLabel("-");
            UpdateStatus("Waiting for request");
        }

        private void OnDestroy()
        {
            if (_sendRequestButton)
            {
                _sendRequestButton.onClick.RemoveListener(OnSendRequestClicked);
            }
        }

        private void OnSendRequestClicked()
        {
            if (_isSending)
                return;

            SendRequestAsync().Forget();
        }

        private async UniTaskVoid SendRequestAsync()
        {
            if (RequestCounterApiService.Instance == null)
            {
                UpdateStatus("API service is unavailable");
                return;
            }

            _isSending = true;
            UpdateStatus("Sending request...");

            try
            {
                var response = await RequestCounterApiService.Instance.SendRequestAsync();
                if (response != null)
                {
                    UpdateCountLabel(response.requestCount.ToString());
                    UpdateStatus($"Requests sent: {response.requestCount}");
                }
                else
                {
                    UpdateStatus("Empty response from server");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.LogError(ex.Message);
            }
            finally
            {
                _isSending = false;
            }
        }

        private void UpdateCountLabel(string value)
        {
            if (_requestCountText)
            {
                _requestCountText.text = value;
            }
        }

        private void UpdateStatus(string message)
        {
            if (_statusText)
            {
                _statusText.text = message;
            }
        }
    }
}