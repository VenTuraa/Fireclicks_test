using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Fireclicks.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        private const int CANVAS_SORTING_ORDER = 1000;
        private const float PROGRESS_MIN = 0f;
        private const float PROGRESS_MAX = 100f;

        [SerializeField] private GameObject _loadingScreenPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;

        private readonly StringBuilder _progressTextBuilder = new StringBuilder(20);

        private void Awake()
        {
            if (_loadingScreenPanel == null)
            {
                CreateLoadingScreen();
            }
        }

        private void CreateLoadingScreen()
        {
            var canvasObj = new GameObject("LoadingCanvas");
            if (canvasObj == null)
            {
                Debug.LogError("[LoadingScreenController] Failed to create canvas!", this);
                return;
            }

            var canvas = canvasObj.AddComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[LoadingScreenController] Failed to add Canvas component!", this);
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CANVAS_SORTING_ORDER;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            _loadingScreenPanel = new GameObject("LoadingPanel");
            if (_loadingScreenPanel == null)
            {
                Debug.LogError("[LoadingScreenController] Failed to create panel!", this);
                return;
            }

            _loadingScreenPanel.transform.SetParent(canvasObj.transform, false);
            var panelRect = _loadingScreenPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            var panelImage = _loadingScreenPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            if (!CreateProgressBar())
                return;

            if (!CreateProgressText())
                return;

            _loadingScreenPanel.SetActive(false);
        }

        private bool CreateProgressBar()
        {
            if (_loadingScreenPanel == null)
                return false;

            var progressBarObj = new GameObject("ProgressBar");
            if (progressBarObj == null)
                return false;

            progressBarObj.transform.SetParent(_loadingScreenPanel.transform, false);
            var progressBarRect = progressBarObj.AddComponent<RectTransform>();
            progressBarRect.anchorMin = new Vector2(0.2f, 0.5f);
            progressBarRect.anchorMax = new Vector2(0.8f, 0.6f);
            progressBarRect.sizeDelta = Vector2.zero;
            progressBarRect.anchoredPosition = Vector2.zero;

            var progressBarBg = progressBarObj.AddComponent<Image>();
            progressBarBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            var progressBarFillArea = new GameObject("Fill Area");
            progressBarFillArea.transform.SetParent(progressBarObj.transform, false);
            var fillAreaRect = progressBarFillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;
            fillAreaRect.anchoredPosition = Vector2.zero;

            var progressBarFill = new GameObject("Fill");
            progressBarFill.transform.SetParent(progressBarFillArea.transform, false);
            var fillRect = progressBarFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            var fillImage = progressBarFill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            _progressBar = progressBarObj.AddComponent<Slider>();
            _progressBar.fillRect = fillRect;
            _progressBar.targetGraphic = fillImage;

            return true;
        }

        private bool CreateProgressText()
        {
            if (_loadingScreenPanel == null)
                return false;

            var progressTextObj = new GameObject("ProgressText");
            if (progressTextObj == null)
                return false;

            progressTextObj.transform.SetParent(_loadingScreenPanel.transform, false);
            var progressTextRect = progressTextObj.AddComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(0.2f, 0.4f);
            progressTextRect.anchorMax = new Vector2(0.8f, 0.5f);
            progressTextRect.sizeDelta = Vector2.zero;
            progressTextRect.anchoredPosition = Vector2.zero;

            _progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            if (_progressText == null)
                return false;

            _progressText.text = "Loading... 0%";
            _progressText.alignment = TextAlignmentOptions.Center;
            _progressText.fontSize = 24;
            _progressText.color = Color.white;

            return true;
        }

        public void Show()
        {
            if (IsPanelValid())
            {
                _loadingScreenPanel.SetActive(true);
                SetProgress(0f);
            }
        }

        public void Hide()
        {
            if (IsPanelValid())
            {
                _loadingScreenPanel.SetActive(false);
            }
        }

        private bool IsPanelValid()
        {
            return _loadingScreenPanel != null;
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp(progress, PROGRESS_MIN, PROGRESS_MAX);

            if (_progressBar != null)
            {
                _progressBar.value = progress / PROGRESS_MAX;
            }

            if (_progressText != null)
            {
                _progressTextBuilder.Clear();
                _progressTextBuilder.Append("Loading... ");
                _progressTextBuilder.Append((int)progress);
                _progressTextBuilder.Append('%');
                _progressText.SetText(_progressTextBuilder);
            }
        }
    }
}
