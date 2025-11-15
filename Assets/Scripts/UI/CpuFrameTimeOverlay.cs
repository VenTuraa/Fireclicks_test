using System.Text;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Debug = UnityEngine.Debug;

namespace Fireclicks.UI
{
    public class CpuFrameTimeOverlay : MonoBehaviour
    {
        private const float MAX_FRAME_TIME_MS = 9999.99f;
        private const int CANVAS_SORTING_ORDER = 9999;
        private const int TEXT_FONT_SIZE = 20;
        private const float TEXT_OUTLINE_WIDTH = 0.2f;

        private GameObject _overlayCanvas;
        private TextMeshProUGUI _frameTimeText;
        private StringBuilder _stringBuilder;
        private Stopwatch _stopwatch;
        private long _lastFrameTime;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            DontDestroyOnLoad(this);

            _overlayCanvas = new GameObject("CpuFrameTimeCanvas");
            if (_overlayCanvas == null)
            {
                Debug.LogError("[CpuFrameTimeOverlay] Failed to create canvas!", this);
                return;
            }

            DontDestroyOnLoad(_overlayCanvas);

            if (!SetupCanvas())
                return;

            if (!SetupText())
                return;

            _stringBuilder = new StringBuilder(30);

            _stopwatch = Stopwatch.StartNew();
            _lastFrameTime = _stopwatch.ElapsedTicks;

            _initialized = true;
        }

        private bool SetupCanvas()
        {
            if (_overlayCanvas == null)
                return false;

            var canvas = _overlayCanvas.AddComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[CpuFrameTimeOverlay] Failed to add Canvas component!", this);
                return false;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CANVAS_SORTING_ORDER;

            _overlayCanvas.AddComponent<CanvasScaler>();
            _overlayCanvas.AddComponent<GraphicRaycaster>();

            return true;
        }

        private bool SetupText()
        {
            if (_overlayCanvas == null)
                return false;

            var textObj = new GameObject("FrameTimeText");
            if (textObj == null)
            {
                Debug.LogError("[CpuFrameTimeOverlay] Failed to create text object!", this);
                return false;
            }

            textObj.transform.SetParent(_overlayCanvas.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(1f, 1f);
            textRect.anchoredPosition = new Vector2(-10f, -10f);
            textRect.sizeDelta = new Vector2(300f, 50f);

            _frameTimeText = textObj.AddComponent<TextMeshProUGUI>();
            if (_frameTimeText == null)
            {
                Debug.LogError("[CpuFrameTimeOverlay] Failed to add TextMeshProUGUI component!", this);
                return false;
            }

            _frameTimeText.text = "CPU Frame Time: 0.00 ms";
            _frameTimeText.alignment = TextAlignmentOptions.TopRight;
            _frameTimeText.fontSize = TEXT_FONT_SIZE;
            _frameTimeText.color = Color.white;

            _frameTimeText.outlineWidth = TEXT_OUTLINE_WIDTH;
            _frameTimeText.outlineColor = Color.black;

            return true;
        }

        private void Update()
        {
            if (!_initialized)
                return;

            if (_frameTimeText == null || _stringBuilder == null || _stopwatch == null)
                return;

            long currentTime = _stopwatch.ElapsedTicks;
            long frameTicks = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;

            double frameTimeMs = (frameTicks * 1000.0) / Stopwatch.Frequency;

            frameTimeMs = System.Math.Min(frameTimeMs, MAX_FRAME_TIME_MS);
            int wholePart = (int)frameTimeMs;
            int fractionalPart = (int)((frameTimeMs - wholePart) * 100);

            _stringBuilder.Clear();
            _stringBuilder.Append("CPU Frame Time: ");
            _stringBuilder.Append(wholePart);
            _stringBuilder.Append('.');
            if (fractionalPart < 10)
            {
                _stringBuilder.Append('0');
            }
            _stringBuilder.Append(fractionalPart);
            _stringBuilder.Append(" ms");

            _frameTimeText.SetText(_stringBuilder);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnDisable()
        {
        }

        private void Cleanup()
        {
            if (_overlayCanvas != null)
            {
                Destroy(_overlayCanvas);
                _overlayCanvas = null;
            }

            _frameTimeText = null;
            _stringBuilder = null;
            _stopwatch = null;
            _initialized = false;
        }
    }
}
