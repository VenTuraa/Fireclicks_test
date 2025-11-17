using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Fireclicks.UI
{
    public class VirtualizedList : MonoBehaviour
    {
        private const int DEFAULT_BUFFER_SIZE = 1;
        private const float MIN_ITEM_HEIGHT = 10f;
        private const float SCROLL_UPDATE_THRESHOLD_MULTIPLIER = 0.3f;

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _itemPrototype;
        [SerializeField] private int _totalCount = 1000;
        [SerializeField] private float _itemHeight = 40f;

        private readonly Stack<RectTransform> _pooledItems = new();
        private readonly Dictionary<RectTransform, TextMeshProUGUI> _cachedTextComponents = new();
        private readonly Dictionary<RectTransform, string> _cachedTextValues = new();
        private readonly List<VisibleItem> _visibleItems = new();

        private int _desiredVisibleItemCount;
        private int _firstVisibleIndex = -1;
        private bool _initialized;
        private bool _heightLockedByVisibleCount;
        private bool _isDirty;
        private bool _forceRebuild;

        private float _lastViewportHeight;
        private float _lastContentWidth;
        private float _lastResolvedItemHeight;
        private float _lastScrollOffset = float.NaN;

        private System.Action<RectTransform, int> _onItemCreated;

        private sealed class VisibleItem
        {
            public RectTransform Rect;
            public int Index = -1;
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (_initialized)
            {
                SubscribeToScroll();
                MarkDirty(true);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromScroll();
        }

        private void Update()
        {
            if (!_initialized)
                return;

            TrackLayoutChanges();
        }

        private void LateUpdate()
        {
            if (!_initialized)
                return;

            if (_isDirty)
            {
                _isDirty = false;
                RefreshVisibleItems();
            }
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            if (!_scrollRect)
                _scrollRect = GetComponent<ScrollRect>();

            if (!_scrollRect)
            {
                Debug.LogError("[VirtualizedList] ScrollRect is not assigned.", this);
                return;
            }

            if (!_content)
                _content = _scrollRect.content;

            if (!_content)
            {
                Debug.LogError("[VirtualizedList] Content RectTransform is not assigned.", this);
                return;
            }

            if (_itemPrototype == null)
            {
                Debug.LogError("[VirtualizedList] Item prototype is not assigned.", this);
                return;
            }

            if (_scrollRect.viewport == null)
            {
                _scrollRect.viewport = _scrollRect.transform as RectTransform;
            }

            if (_itemPrototype.gameObject.activeSelf)
            {
                _itemPrototype.gameObject.SetActive(false);
            }

            SetupContentLayout();
            RefreshItemHeight();
            UpdateContentHeight();
            SubscribeToScroll();

            _initialized = true;
            MarkDirty(true);
        }

        private void SetupContentLayout()
        {
            if (_content == null)
                return;

            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
        }

        private void SubscribeToScroll()
        {
            if (_scrollRect == null)
                return;

            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void UnsubscribeFromScroll()
        {
            if (_scrollRect == null)
                return;

            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        private void OnScrollValueChanged(Vector2 position)
        {
            MarkDirty();
        }

        private void TrackLayoutChanges()
        {
            if (_scrollRect == null)
                return;

            RectTransform viewport = _scrollRect.viewport ?? _scrollRect.transform as RectTransform;
            if (viewport == null)
                return;

            float viewportHeight = viewport.rect.height;
            if (!Mathf.Approximately(viewportHeight, _lastViewportHeight))
            {
                _lastViewportHeight = viewportHeight;
                if (_heightLockedByVisibleCount)
                {
                    RefreshItemHeight();
                    UpdateContentHeight();
                }
                MarkDirty(true);
            }

            float currentContentWidth = _content ? _content.rect.width : 0f;
            if (!Mathf.Approximately(currentContentWidth, _lastContentWidth))
            {
                _lastContentWidth = currentContentWidth;
                MarkDirty(true);
            }

            if (!Mathf.Approximately(_lastResolvedItemHeight, _itemHeight))
            {
                _lastResolvedItemHeight = _itemHeight;
                MarkDirty(true);
            }
        }

        private void RefreshVisibleItems()
        {
            if (_scrollRect == null || _content == null || _itemPrototype == null)
                return;

            if (_totalCount <= 0)
            {
                ReleaseVisibleItems();
                UpdateContentHeight();
                return;
            }

            RectTransform viewport = _scrollRect.viewport ?? _scrollRect.transform as RectTransform;
            if (viewport == null)
                return;

            float resolvedItemHeight = Mathf.Max(MIN_ITEM_HEIGHT, _itemHeight);
            if (!Mathf.Approximately(resolvedItemHeight, _itemHeight))
            {
                _itemHeight = resolvedItemHeight;
                UpdateContentHeight();
            }

            float viewportHeight = Mathf.Max(0.0001f, viewport.rect.height);
            int itemsThatFit = Mathf.Max(1, Mathf.CeilToInt(viewportHeight / resolvedItemHeight));
            int desiredVisible = Mathf.Min(_totalCount, itemsThatFit + DEFAULT_BUFFER_SIZE * 2);
            desiredVisible = Mathf.Max(desiredVisible, 1);

            EnsureVisibleCapacity(desiredVisible);

            float scrollOffset = Mathf.Abs(_content.anchoredPosition.y);
            float maxOffset = Mathf.Max(0f, _content.sizeDelta.y - viewportHeight);
            scrollOffset = Mathf.Clamp(scrollOffset, 0f, maxOffset);

            float updateThreshold = resolvedItemHeight * SCROLL_UPDATE_THRESHOLD_MULTIPLIER;
            bool scrollOffsetChanged = float.IsNaN(_lastScrollOffset) || 
                Mathf.Abs(scrollOffset - _lastScrollOffset) >= updateThreshold;

            if (!scrollOffsetChanged && !_forceRebuild)
            {
                return; 
            }

            int computedFirstIndex = Mathf.FloorToInt(scrollOffset / resolvedItemHeight) - DEFAULT_BUFFER_SIZE;
            int maxFirstIndex = Mathf.Max(0, _totalCount - _visibleItems.Count);
            computedFirstIndex = Mathf.Clamp(computedFirstIndex, 0, maxFirstIndex);

            bool firstIndexChanged = computedFirstIndex != _firstVisibleIndex;
            bool forceLayout = firstIndexChanged || _forceRebuild;
            
            if (firstIndexChanged || scrollOffsetChanged)
            {
                _firstVisibleIndex = computedFirstIndex;
                _lastScrollOffset = scrollOffset;
            }
            
            _forceRebuild = false;

            for (int i = 0; i < _visibleItems.Count; i++)
            {
                int itemIndex = computedFirstIndex + i;
                VisibleItem visibleItem = _visibleItems[i];

                if (itemIndex >= _totalCount)
                {
                    HideVisibleItem(visibleItem);
                    continue;
                }

                bool needsUpdate = visibleItem.Index != itemIndex || forceLayout;
                if (needsUpdate)
                {
                    UpdateVisibleItem(visibleItem, itemIndex, resolvedItemHeight, forceLayout);
                }
            }
        }

        private void UpdateVisibleItem(VisibleItem item, int index, float itemHeight, bool forceLayout)
        {
            if (!item?.Rect || !_content)
                return;

            bool indexChanged = item.Index != index;
            bool wasInactive = !item.Rect.gameObject.activeSelf;

            if (wasInactive)
            {
                item.Rect.gameObject.SetActive(true);
                forceLayout = true;
            }

            if (indexChanged)
            {
                item.Index = index;
            }

            float targetY = -index * itemHeight;
            float targetWidth = Mathf.Abs(_content.rect.width);
            float targetHeight = itemHeight;

            item.Rect.anchoredPosition = new Vector2(0f, targetY);
            item.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            item.Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

            if (indexChanged)
            {
                string newText = index.ToString();
                
                if (!_cachedTextValues.TryGetValue(item.Rect, out string cachedText) || cachedText != newText)
                {
                    CacheTextComponent(item.Rect);
                    if (_cachedTextComponents.TryGetValue(item.Rect, out TextMeshProUGUI text))
                    {
                        text.SetText(newText);
                        _cachedTextValues[item.Rect] = newText;
                    }
                }

                _onItemCreated?.Invoke(item.Rect, index);
            }
        }

        private void EnsureVisibleCapacity(int targetCount)
        {
            if (targetCount < 0)
                targetCount = 0;

            while (_visibleItems.Count < targetCount)
            {
                RectTransform rect = CreateItem();
                if (rect == null)
                    break;

                _visibleItems.Add(new VisibleItem { Rect = rect, Index = -1 });
            }

            while (_visibleItems.Count > targetCount)
            {
                int lastIndex = _visibleItems.Count - 1;
                ReturnToPool(_visibleItems[lastIndex]);
                _visibleItems.RemoveAt(lastIndex);
            }
        }

        private RectTransform CreateItem()
        {
            RectTransform rect = null;

            while (_pooledItems.Count > 0 && !rect)
            {
                rect = _pooledItems.Pop();
            }

            if (!rect)
            {
                rect = Instantiate(_itemPrototype, _content);
            }

            rect.SetParent(_content, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.gameObject.SetActive(true);

            CacheTextComponent(rect);
            return rect;
        }

        private void HideVisibleItem(VisibleItem item)
        {
            if (!item?.Rect)
                return;

            item.Index = -1;
            if (item.Rect.gameObject.activeSelf)
            {
                item.Rect.gameObject.SetActive(false);
            }
        }

        private void ReturnToPool(VisibleItem item)
        {
            if (!item?.Rect)
                return;

            item.Index = -1;
            item.Rect.gameObject.SetActive(false);
            
            _cachedTextValues.Remove(item.Rect);
            
            _pooledItems.Push(item.Rect);
        }

        private void ReleaseVisibleItems()
        {
            for (int i = _visibleItems.Count - 1; i >= 0; i--)
            {
                ReturnToPool(_visibleItems[i]);
            }

            _visibleItems.Clear();
            _firstVisibleIndex = -1;
            _lastScrollOffset = float.NaN;
        }

        private void CacheTextComponent(RectTransform rect)
        {
            if (rect == null || _cachedTextComponents.ContainsKey(rect))
                return;

            TextMeshProUGUI text = rect.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text)
            {
                _cachedTextComponents[rect] = text;
            }
        }

        public void SetItemCreatedCallback(System.Action<RectTransform, int> callback)
        {
            _onItemCreated = callback;
        }

        public void SetTotalCount(int count)
        {
            _totalCount = Mathf.Max(0, count);

            if (_initialized)
            {
                UpdateContentHeight();
                MarkDirty(true);
            }
        }

        public void SetVisibleItemCount(int count)
        {
            _desiredVisibleItemCount = Mathf.Max(0, count);
            _heightLockedByVisibleCount = _desiredVisibleItemCount > 0;

            if (_initialized)
            {
                RefreshItemHeight();
                UpdateContentHeight();
                MarkDirty(true);
            }
        }

        private void RefreshItemHeight()
        {
            float previous = _itemHeight;

            if (_heightLockedByVisibleCount && _desiredVisibleItemCount > 0)
            {
                RectTransform viewport = _scrollRect
                    ? _scrollRect.viewport ?? _scrollRect.transform as RectTransform
                    : null;

                if (viewport)
                {
                    float viewportHeight = viewport.rect.height;
                    if (viewportHeight > 0f)
                    {
                        _itemHeight = Mathf.Max(MIN_ITEM_HEIGHT, viewportHeight / _desiredVisibleItemCount);
                    }
                }
            }
            else
            {
                float prototypeHeight = ResolvePrototypeHeight();
                _itemHeight = Mathf.Max(MIN_ITEM_HEIGHT, prototypeHeight > 0f ? prototypeHeight : _itemHeight);
            }

            if (!Mathf.Approximately(previous, _itemHeight))
            {
                MarkDirty(true);
            }
        }

        private float ResolvePrototypeHeight()
        {
            if (_itemPrototype == null)
                return 0f;

            float height = _itemPrototype.rect.height;

            if (height <= 0f)
            {
                height = LayoutUtility.GetPreferredHeight(_itemPrototype);
            }

            if (height <= 0f)
            {
                LayoutElement element = _itemPrototype.GetComponent<LayoutElement>();
                if (element != null && element.preferredHeight > 0f)
                {
                    height = element.preferredHeight;
                }
            }

            return Mathf.Abs(height);
        }

        private void UpdateContentHeight()
        {
            if (!_content)
                return;

            float contentHeight = _totalCount * Mathf.Max(MIN_ITEM_HEIGHT, _itemHeight);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);
        }

        private void MarkDirty(bool force = false)
        {
            _isDirty = true;
            if (force)
            {
                _forceRebuild = true;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromScroll();
            ReleaseVisibleItems();
            CleanupPooledItems();
            _cachedTextComponents.Clear();
            _cachedTextValues.Clear();
        }

        private void CleanupPooledItems()
        {
            while (_pooledItems.Count > 0)
            {
                RectTransform rect = _pooledItems.Pop();
                if (rect != null)
                {
                    Destroy(rect.gameObject);
                }
            }

            _pooledItems.Clear();
        }
    }
}
