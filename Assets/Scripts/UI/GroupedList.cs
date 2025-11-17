using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Fireclicks.UI
{
    public enum ItemRarity
    {
        Green = 1,
        Purple = 2,
        Gold = 3
    }

    [System.Serializable]
    public class GroupedItem
    {
        public ItemRarity Rarity;

        public GroupedItem(int index, ItemRarity rarity)
        {
            Rarity = rarity;
        }
    }

    public class GroupedList : MonoBehaviour
    {
        private const int GREEN_COUNT = 10;
        private const int PURPLE_COUNT = 6;
        private const int GOLD_COUNT = 2;

        [SerializeField] private Color _greenColor = new(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _purpleColor = new(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _goldColor = new(1f, 0.84f, 0f);

        private List<GroupedItem> _items;
        private readonly Dictionary<RectTransform, TextMeshProUGUI> _cachedTextComponents = new();
        private bool _initialized;

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;

            _items = GenerateGroupedItems();

            _initialized = true;
        }

        private List<GroupedItem> GenerateGroupedItems()
        {
            var items = new List<GroupedItem>();

            int greenCount = 0;
            int purpleCount = 0;
            int goldCount = 0;
            int index = 0;
            int cyclePosition = 0;
            bool isThreeCycle = true;

            while (greenCount < GREEN_COUNT || purpleCount < PURPLE_COUNT || goldCount < GOLD_COUNT)
            {
                if (greenCount < GREEN_COUNT && purpleCount < PURPLE_COUNT && goldCount < GOLD_COUNT)
                {
                    if (cyclePosition % 3 == 0)
                    {
                        items.Add(new GroupedItem(index++, ItemRarity.Green));
                        greenCount++;
                    }
                    else if (cyclePosition % 3 == 1)
                    {
                        items.Add(new GroupedItem(index++, ItemRarity.Purple));
                        purpleCount++;
                    }
                    else
                    {
                        items.Add(new GroupedItem(index++, ItemRarity.Gold));
                        goldCount++;
                    }
                    cyclePosition++;
                }
                else if (greenCount < GREEN_COUNT && purpleCount < PURPLE_COUNT)
                {
                    if (isThreeCycle)
                    {
                        isThreeCycle = false;
                        cyclePosition = 0;
                    }
                    
                    if (cyclePosition % 2 == 0)
                    {
                        items.Add(new GroupedItem(index++, ItemRarity.Green));
                        greenCount++;
                    }
                    else
                    {
                        items.Add(new GroupedItem(index++, ItemRarity.Purple));
                        purpleCount++;
                    }
                    cyclePosition++;
                }
                else if (greenCount < GREEN_COUNT)
                {
                    items.Add(new GroupedItem(index++, ItemRarity.Green));
                    greenCount++;
                }
                else if (purpleCount < PURPLE_COUNT)
                {
                    items.Add(new GroupedItem(index++, ItemRarity.Purple));
                    purpleCount++;
                }
            }

            return items;
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Green:
                    return _greenColor;
                case ItemRarity.Purple:
                    return _purpleColor;
                case ItemRarity.Gold:
                    return _goldColor;
                default:
                    return Color.white;
            }
        }

        public int GetTotalCount()
        {
            return _items?.Count ?? 0;
        }

        public void UpdateItemVisual(RectTransform itemRect, int index)
        {
            if (!itemRect)
                return;

            if (_items == null || index < 0 || index >= _items.Count)
                return;

            GroupedItem item = _items[index];
            if (item == null)
                return;

            if (!_cachedTextComponents.TryGetValue(itemRect, out TextMeshProUGUI text))
            {
                text = itemRect.GetComponentInChildren<TextMeshProUGUI>();
                if (text)
                    _cachedTextComponents[itemRect] = text;
            }

            if (!text) return;
            text.text = ((int)item.Rarity).ToString();
            text.color = GetRarityColor(item.Rarity);
        }

        private void OnDestroy()
        {
            _cachedTextComponents.Clear();
        }
    }
}
