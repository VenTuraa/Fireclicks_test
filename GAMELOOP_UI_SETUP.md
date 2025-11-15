# Налаштування UI для GameLoopScene

## Структура UI

Створіть наступну структуру в GameLoopScene:

```
Canvas (Screen Space - Overlay)
├── TabsController (GameObject з компонентом TabsController)
│   ├── ListTab (GameObject)
│   │   └── ScrollRect (GameObject з компонентом ScrollRect)
│   │       ├── Viewport (RectTransform)
│   │       │   └── Content (RectTransform) - прив'язати до VirtualizedList
│   │       ├── Scrollbar (опціонально)
│   │       └── VirtualizedList (компонент VirtualizedList)
│   │           - Scroll Rect → ScrollRect
│   │           - Content → Content RectTransform
│   │           - Item Prototype → ItemPrototype
│   │           - Total Count = 1000
│   │           - Item Height = 40
│   │       └── ItemPrototype (RectTransform) - приховати
│   │           └── Text (TextMeshProUGUI)
│   │
│   └── GroupedListTab (GameObject)
│       └── ScrollRect (GameObject з компонентом ScrollRect)
│           ├── Viewport (RectTransform)
│           │   └── Content (RectTransform) - прив'язати до VirtualizedList
│           ├── Scrollbar (опціонально)
│           ├── VirtualizedList (компонент VirtualizedList)
│           │   - Scroll Rect → ScrollRect
│           │   - Content → Content RectTransform
│           │   - Item Prototype → ItemPrototype
│           │   - Total Count = 18 (буде встановлено автоматично)
│           │   - Item Height = 40
│           └── GroupedList (компонент GroupedList)
│               - Virtualized List → VirtualizedList
│               - Green Color = (0.2, 0.8, 0.2)
│               - Purple Color = (0.6, 0.2, 0.8)
│               - Gold Color = (1, 0.84, 0)
│           └── ItemPrototype (RectTransform) - приховати
│               └── Text (TextMeshProUGUI)
│
└── TabButtons (GameObject)
    ├── ListTabButton (Button)
    │   └── OnClick → TabsController.ShowListTab()
    └── GroupedListTabButton (Button)
        └── OnClick → TabsController.ShowGroupedListTab()
```

## Покрокова інструкція

### 1. Створення Canvas

1. Створіть Canvas: **GameObject → UI → Canvas**
2. Налаштуйте Canvas:
   - Render Mode = Screen Space - Overlay
   - Canvas Scaler: UI Scale Mode = Scale With Screen Size

### 2. Створення TabsController

1. Створіть порожній GameObject під назвою "TabsController"
2. Додайте компонент `TabsController`
3. Створіть два дочірні GameObject:
   - `ListTab`
   - `GroupedListTab`

### 3. Налаштування ListTab (1000 елементів)

1. Створіть ScrollRect: **GameObject → UI → Scroll View**
2. Перейменуйте в "ListTabScrollRect"
3. Перемістіть під `ListTab`
4. Налаштуйте ScrollRect:
   - Viewport: anchorMin = (0,0), anchorMax = (1,1)
   - Content: anchorMin = (0,1), anchorMax = (1,1), pivot = (0.5,1)
5. Створіть ItemPrototype:
   - Порожній GameObject під назвою "ItemPrototype"
   - Додайте RectTransform
   - Anchor: Min = (0,1), Max = (1,1), Pivot = (0.5,1)
   - Size: Width = Content width, Height = 40
   - Додайте TextMeshProUGUI як дочірній об'єкт
6. Додайте компонент `VirtualizedList` на ScrollRect:
   - Scroll Rect → ScrollRect
   - Content → Content RectTransform
   - Item Prototype → ItemPrototype
   - Total Count = 1000
   - Item Height = 40
7. Приховайте ItemPrototype (SetActive = false)

### 4. Налаштування GroupedListTab (18 елементів)

1. Створіть ScrollRect: **GameObject → UI → Scroll View**
2. Перейменуйте в "GroupedListTabScrollRect"
3. Перемістіть під `GroupedListTab`
4. Налаштуйте ScrollRect (аналогічно ListTab)
5. Створіть ItemPrototype (аналогічно ListTab)
6. Додайте компонент `VirtualizedList` на ScrollRect:
   - Scroll Rect → ScrollRect
   - Content → Content RectTransform
   - Item Prototype → ItemPrototype
   - Total Count = 18 (буде встановлено автоматично через GroupedList)
   - Item Height = 40
7. Додайте компонент `GroupedList` на той самий ScrollRect:
   - Virtualized List → VirtualizedList
   - Green Color = (0.2, 0.8, 0.2)
   - Purple Color = (0.6, 0.2, 0.8)
   - Gold Color = (1, 0.84, 0)
8. Приховайте ItemPrototype

### 5. Налаштування кнопок перемикання

1. Створіть GameObject "TabButtons" під Canvas
2. Створіть Button: **GameObject → UI → Button - TextMeshPro**
3. Перейменуйте в "ListTabButton"
4. Налаштуйте текст: "List (1000)"
5. Додайте OnClick event:
   - Перетягніть TabsController в поле Object
   - Виберіть `TabsController.ShowListTab()`
6. Повторіть для "GroupedListTabButton" з текстом "Grouped List (18)"

### 6. Прив'язка в TabsController

1. Виберіть TabsController
2. В Inspector:
   - List Tab → перетягніть ListTab GameObject
   - Grouped List Tab → перетягніть GroupedListTab GameObject

## Важливі примітки

1. **Не використовуйте LayoutGroup** - позиціонування виконується вручну через код
2. **ItemPrototype має бути прихований** - він використовується тільки як шаблон
3. **Content anchor** - має бути закріплений зверху (anchorMin/Max = (0,1), pivot = (0.5,1))
4. **Item Height** - має відповідати висоті ItemPrototype
5. **Viewport** - має бути правильно налаштований для ScrollRect

## Перевірка роботи

### ListTab (1000 елементів):
- ✅ Прокрутка працює плавно
- ✅ Створюються тільки видимі елементи
- ✅ Кожен елемент показує свій індекс (0-999)

### GroupedListTab (18 елементів):
- ✅ Порядок: 1 2 3 1 2 3 1 2 1 2 1 2 1 2 1 1 1 1
- ✅ Кольори: зелений, фіолетовий, золотий
- ✅ Всього 10 зелених, 6 фіолетових, 2 золотих
- ✅ Прокрутка працює
- ✅ Одночасно видимі близько 3 елементів

