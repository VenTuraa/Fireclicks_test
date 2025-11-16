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
        private const string DEFAULT_COUNT_LABEL = "-";
        private const string STATUS_WAITING = "Waiting for request";
        private const string STATUS_API_UNAVAILABLE = "API service is unavailable";
        private const string STATUS_SENDING = "Sending request...";
        private const string STATUS_EMPTY_RESPONSE = "Empty response from server";
 

        [SerializeField] private Button _sendRequestButton;
        [SerializeField] private TextMeshProUGUI _requestCountText;
        [SerializeField] private TextMeshProUGUI _statusText;

        private bool _isSending;

        private void Awake()
        {
            if (_sendRequestButton)
                _sendRequestButton.onClick.AddListener(OnSendRequestClicked);

            UpdateCountLabel(DEFAULT_COUNT_LABEL);
            UpdateStatus(STATUS_WAITING);
        }

        private void OnDestroy()
        {
            if (_sendRequestButton)
                _sendRequestButton.onClick.RemoveListener(OnSendRequestClicked);
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
                UpdateStatus(STATUS_API_UNAVAILABLE);
                return;
            }

            _isSending = true;
            UpdateStatus(STATUS_SENDING);

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
                    UpdateStatus(STATUS_EMPTY_RESPONSE);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.LogError($"[RequestTabController] Request failed: {ex.Message}", this);
            }
            finally
            {
                _isSending = false;
            }
        }

        private void UpdateCountLabel(string value)
        {
            if (_requestCountText)
                _requestCountText.text = value;
        }

        private void UpdateStatus(string message)
        {
            if (_statusText)
                _statusText.SetText(message);
        }
    }
}