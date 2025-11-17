using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Fireclicks.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        private const float PROGRESS_MIN = 0f;
        private const float PROGRESS_MAX = 1f;
        private const string PROGRESS_TEXT_PREFIX = "Loading... ";

        [SerializeField] private GameObject _loadingScreenPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _progressText;

        private readonly StringBuilder _progressTextBuilder = new(20);
        private float _currentProgress;
        private float _targetProgress;

        public void Show()
        {
            if (_loadingScreenPanel)
                _loadingScreenPanel.SetActive(true);
            _currentProgress = 0f;
            _targetProgress = 0f;
            SetProgressImmediate(0f);
        }

        public void Hide()
        {
            if (_loadingScreenPanel)
                _loadingScreenPanel.SetActive(false);
        }
        
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp(progress, PROGRESS_MIN, PROGRESS_MAX);
        }

        public void SetProgressImmediate(float progress)
        {
            progress = Mathf.Clamp(progress, PROGRESS_MIN, PROGRESS_MAX);
            _currentProgress = progress;
            _targetProgress = progress;
            UpdateProgressBar(progress);
        }

        private void Update()
        {
            if (Mathf.Abs(_currentProgress - _targetProgress) > 0.01f)
            {
                _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * 5f);
            }
            else if (Mathf.Abs(_currentProgress - _targetProgress) > 0.001f)
            {
                _currentProgress = _targetProgress;
            }
            
            UpdateProgressBar(_currentProgress);
        }

        private void UpdateProgressBar(float progress)
        {
            progress = Mathf.Clamp(progress, PROGRESS_MIN, PROGRESS_MAX);
            
            if (_progressBar )
            {
                float normalizedValue = progress / PROGRESS_MAX;
                _progressBar.value = normalizedValue;
                
            }

            if (_progressText)
            {
                _progressTextBuilder.Clear();
                _progressTextBuilder.Append(PROGRESS_TEXT_PREFIX);
                _progressTextBuilder.Append((int)progress);
                _progressTextBuilder.Append('%');
                _progressText.SetText(_progressTextBuilder);
            }
        }
    }
}
