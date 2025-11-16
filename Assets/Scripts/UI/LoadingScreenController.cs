using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Fireclicks.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        private const float PROGRESS_MIN = 0f;
        private const float PROGRESS_MAX = 100f;
        private const string PROGRESS_TEXT_PREFIX = "Loading... ";

        [SerializeField] private GameObject _loadingScreenPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _progressText;

        private readonly StringBuilder _progressTextBuilder = new(20);

        public void Show()
        {
            if (_loadingScreenPanel)
                _loadingScreenPanel.SetActive(true);
            SetProgress(0f);
        }

        public void Hide()
        {
            if (_loadingScreenPanel)
                _loadingScreenPanel.SetActive(false);
        }
        
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, PROGRESS_MIN, PROGRESS_MAX);

            if (_progressBar)
                _progressBar.value = progress / PROGRESS_MAX;

            if (!_progressText) return;
            _progressTextBuilder.Clear();
            _progressTextBuilder.Append(PROGRESS_TEXT_PREFIX);
            _progressTextBuilder.Append((int)progress);
            _progressTextBuilder.Append('%');
            _progressText.SetText(_progressTextBuilder);
        }
    }
}
