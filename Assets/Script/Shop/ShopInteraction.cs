using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Основной скрипт магазина. Управление торговлей между игроком и магазином.
/// </summary>
public class ShopInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("UI магазина")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopSlotsContainer;
    [SerializeField] private GameObject hintObject;
    [Tooltip("Префаб InventorySlot (Assets/PreFab/UI/Inventory/InventorySlot.prefab)")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [Tooltip("Префаб InventoryItem для создания предметов в слотах")]
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Настройки магазина")]
    [SerializeField] private int shopMoney = 100;
    [Tooltip("Максимальное количество слотов для продажи (дефолтных + проданных игроком)")]
    public int maxShopSlots = 8;
    [SerializeField] private List<Item> defaultItemsForSale = new List<Item>();
    [Tooltip("Цены для предметов. Количество элементов должно совпадать с defaultItemsForSale")]
    [SerializeField] private List<ShopItemPrice> itemPrices = new List<ShopItemPrice>();

    /// <summary>
    /// Сериализуемая структура для цен предметов (отображается в Inspector)
    /// </summary>
    [System.Serializable]
    public struct ShopItemPrice
    {
        public Item item;
        public int price;
    }

    private bool _isShopOpen = false;
    private Transform _playerTransform;
    private bool _isPlayerNearby = false;
    private bool _isInitialized = false;

    // Предметы в магазине (инвентарь магазина)
    private Dictionary<Item, int> _shopInventory = new Dictionary<Item, int>();

    void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        try
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("ShopInteraction: объект Player не найден!");
                enabled = false;
                return;
            }

            if (shopPanel == null)
            {
                Debug.LogError("ShopInteraction: shopPanel не назначен!");
                enabled = false;
                return;
            }

            if (shopPanel.activeSelf)
            {
                shopPanel.SetActive(false);
            }

            _isShopOpen = false;

            if (hintObject != null)
            {
                hintObject.SetActive(false);
            }

            // Инициализируем цены если не заданы
            if (itemPrices == null)
            {
                itemPrices = new List<ShopItemPrice>();
            }

            // Загружаем товары по умолчанию
            LoadDefaultItems();

            _isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка инициализации - {e.Message}");
            enabled = false;
        }
    }

    private void LoadDefaultItems()
    {
        if (defaultItemsForSale == null || defaultItemsForSale.Count == 0) return;

        int loadedCount = 0;
        foreach (Item item in defaultItemsForSale)
        {
            if (item == null) continue;
            if (loadedCount >= maxShopSlots)
            {
                Debug.LogWarning($"ShopInteraction: превышен лимит слотов ({maxShopSlots}). Загружено только {loadedCount} предметов.");
                break;
            }

            int price = 10; // Цена по умолчанию

            // Ищем цену в списке itemPrices
            foreach (var itemPrice in itemPrices)
            {
                if (itemPrice.item == item)
                {
                    price = itemPrice.price;
                    break;
                }
            }

            _shopInventory[item] = 5; // По умолчанию 5 штук

            // Если цены ещё нет в списке — добавляем
            bool priceExists = false;
            for (int i = 0; i < itemPrices.Count; i++)
            {
                if (itemPrices[i].item == item)
                {
                    var updated = itemPrices[i];
                    updated.price = price;
                    itemPrices[i] = updated;
                    priceExists = true;
                    break;
                }
            }
            if (!priceExists)
            {
                itemPrices.Add(new ShopItemPrice { item = item, price = price });
            }

            loadedCount++;
        }
    }

    void Update()
    {
        if (!_isInitialized || _playerTransform == null) return;

        try
        {
            float distance = Vector3.Distance(_playerTransform.position, transform.position);
            bool wasPlayerNearby = _isPlayerNearby;
            _isPlayerNearby = distance <= interactionDistance;

            // Автозакрытие при отходе
            if (_isShopOpen && !_isPlayerNearby)
            {
                CloseShop();
            }

            if (hintObject != null && hintObject.activeSelf != _isPlayerNearby)
            {
                hintObject.SetActive(_isPlayerNearby);
            }

            if (_isPlayerNearby && Input.GetKeyDown(interactionKey))
            {
                if (_isShopOpen)
                    CloseShop();
                else
                    OpenShop();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка в Update - {e.Message}");
        }
    }

    public void OpenShop()
    {
        try
        {
            if (shopPanel == null) return;
            if (_isShopOpen) return;

            shopPanel.SetActive(true);
            _isShopOpen = true;

            UpdateShopUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка открытия - {e.Message}\n{e.StackTrace}");
        }
    }

    public void CloseShop()
    {
        try
        {
            if (shopPanel == null) return;
            if (!_isShopOpen) return;

            // Удаляем предметы, которые игрок продал магазину (не бесконечные)
            RemovePlayerSoldItems();

            shopPanel.SetActive(false);
            _isShopOpen = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка закрытия - {e.Message}");
        }
    }

    /// <summary>
    /// Удаляет из магазина предметы, которые игрок продал ему (не defaultItemsForSale)
    /// </summary>
    private void RemovePlayerSoldItems()
    {
        var itemsToRemove = new List<Item>();

        foreach (var kvp in _shopInventory)
        {
            Item item = kvp.Key;
            // Не удаляем бесконечные предметы (из defaultItemsForSale)
            if (!IsInfiniteItem(item))
            {
                itemsToRemove.Add(item);
            }
        }

        foreach (Item item in itemsToRemove)
        {
            _shopInventory.Remove(item);
        }
    }

    private void UpdateShopUI()
    {
        if (shopSlotsContainer == null)
        {
            Debug.LogWarning("ShopInteraction: shopSlotsContainer не назначен!");
            return;
        }

        if (inventorySlotPrefab == null)
        {
            Debug.LogError("ShopInteraction: inventorySlotPrefab не назначен! Перетащите префаб InventorySlot.");
            return;
        }

        // Очищаем старые слоты
        for (int i = shopSlotsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(shopSlotsContainer.GetChild(i).gameObject);
        }

        if (_shopInventory.Count == 0) return;

        // Ограничиваем количество слотов
        int slotsToShow = Mathf.Min(_shopInventory.Count, maxShopSlots);

        // Создаём слоты из префаба InventorySlot
        int slotIndex = 0;
        foreach (var kvp in _shopInventory)
        {
            if (slotIndex >= slotsToShow) break;

            Item item = kvp.Key;
            int count = kvp.Value;

            if (item == null || count <= 0 || item.image == null) continue;

            // Instantiate префаб InventorySlot
            GameObject slotGo = Instantiate(inventorySlotPrefab, shopSlotsContainer);
            slotGo.name = $"ShopSlot_{slotIndex}";
            slotGo.SetActive(true);

            // Отключаем оригинальный InventorySlot чтобы нельзя было дропать предметы
            InventorySlot invSlot = slotGo.GetComponent<InventorySlot>();
            if (invSlot != null) invSlot.enabled = false;

            // Добавляем/получаем ShopSlot
            ShopSlot shopSlot = slotGo.GetComponent<ShopSlot>();
            if (shopSlot == null) shopSlot = slotGo.AddComponent<ShopSlot>();
            shopSlot.shopUI = this;
            shopSlot.Setup(item, GetItemPrice(item), count);

            // Добавляем кнопку покупки
            ShopSlotBuyButton buyButton = slotGo.GetComponent<ShopSlotBuyButton>();
            if (buyButton == null) buyButton = slotGo.AddComponent<ShopSlotBuyButton>();
            buyButton.shopSlot = shopSlot;

            // Создаём предмет в слоте
            if (inventoryItemPrefab != null)
            {
                GameObject itemGo = Instantiate(inventoryItemPrefab, slotGo.transform);
                itemGo.name = $"Item_{item.name}";
                InventoryItem invItem = itemGo.GetComponent<InventoryItem>();
                if (invItem != null)
                {
                    invItem.InitialiseItem(item);

                    // Бесконечное количество для предметов по умолчанию
                    bool isInfinite = IsInfiniteItem(item);
                    invItem.count = isInfinite ? 999 : count;
                    invItem.RefreshCount();

                    if (isInfinite)
                    {
                        // Показываем символ бесконечности вместо числа
                        if (invItem.countText != null)
                        {
                            invItem.countText.text = "∞";
                            invItem.countText.gameObject.SetActive(true);
                        }
                    }

                    // Полностью отключаем все обработчики перетаскивания
                    // Находим и отключаем IBeginDragHandler, IDragHandler, IEndDragHandler
                    MonoBehaviour[] behaviours = itemGo.GetComponents<MonoBehaviour>();
                    foreach (var behaviour in behaviours)
                    {
                        if (behaviour is IBeginDragHandler || behaviour is IDragHandler || behaviour is IEndDragHandler)
                        {
                            behaviour.enabled = false;
                        }
                    }

                    // Создаём текстовый элемент цены как дочерний элемент предмета
                    CreatePriceText(itemGo, GetItemPrice(item));
                }
            }

            slotIndex++;
        }
    }

    /// <summary>
    /// Создаёт текстовый элемент цены как дочерний элемент предмета с использованием TextMeshPro
    /// </summary>
    private void CreatePriceText(GameObject parentGo, int price)
    {
        GameObject priceObj = new GameObject("PriceText");
        priceObj.transform.SetParent(parentGo.transform);
        priceObj.transform.localScale = Vector3.one;

        var rectTransform = priceObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0, -15);
        rectTransform.sizeDelta = new Vector2(80, 22);

        var textMeshPro = priceObj.AddComponent<TMPro.TextMeshProUGUI>();
        textMeshPro.text = $"{price}$";
        textMeshPro.fontSize = 24;
        textMeshPro.fontStyle = TMPro.FontStyles.Bold;
        textMeshPro.alignment = TMPro.TextAlignmentOptions.Center;
        textMeshPro.color = Color.black; // Чёрный цвет
        textMeshPro.font = TMPro.TMP_Settings.defaultFontAsset;
        textMeshPro.enableWordWrapping = false;
        textMeshPro.raycastTarget = false;

        // Белая обводка для контраста на тёмном фоне
        textMeshPro.outlineColor = Color.white;
        textMeshPro.outlineWidth = 0.2f;
    }

    /// <summary>
    /// Проверяет, является ли предмет бесконечным (по умолчанию)
    /// </summary>
    private bool IsInfiniteItem(Item item)
    {
        // Предметы из defaultItemsForSale считаются бесконечными
        if (defaultItemsForSale != null && defaultItemsForSale.Contains(item))
        {
            return true;
        }
        return false;
    }

    private int GetItemPrice(Item item)
    {
        foreach (var itemPrice in itemPrices)
        {
            if (itemPrice.item == item)
            {
                return itemPrice.price;
            }
        }
        return 10; // Цена по умолчанию
    }

    public void BuyItem(Item item)
    {
        try
        {
            if (item == null) return;
            if (!_shopInventory.ContainsKey(item) || _shopInventory[item] <= 0)
            {
                Debug.LogWarning("ShopInteraction: предмета нет в наличии");
                return;
            }

            int price = GetItemPrice(item);

            // Проверяем деньги игрока
            if (Player.Instance != null)
            {
                if (Player.Instance.coin < price)
                {
                    Debug.LogWarning("ShopInteraction: недостаточно денег");
                    return;
                }

                Player.Instance.coin -= price;
            }

            // Добавляем предмет в инвентарь игрока
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                if (inventory.AddItem(item))
                {
                    // Уменьшаем количество только если не бесконечный
                    if (!IsInfiniteItem(item))
                    {
                        _shopInventory[item]--;
                        if (_shopInventory[item] <= 0)
                        {
                            _shopInventory.Remove(item);
                        }
                    }
                    UpdateShopUI();
                }
                else
                {
                    Debug.LogWarning("ShopInteraction: инвентарь заполнен");
                    // Возвращаем деньги если не удалось добавить предмет
                    if (Player.Instance != null)
                    {
                        Player.Instance.coin += price;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка покупки - {e.Message}");
        }
    }

    public void SellItemToShop(Item item, int count)
    {
        try
        {
            if (item == null || count <= 0) return;

            int sellPricePerUnit = Mathf.RoundToInt(GetItemPrice(item) * 0.5f); // Цена за 1 предмет
            int totalSellPrice = sellPricePerUnit * count; // Общая сумма за все предметы

            Debug.Log($"ShopInteraction: продано {count} x {item.name} по {sellPricePerUnit}$ = {totalSellPrice}$");

            // Начисляем деньги игроку
            if (Player.Instance != null)
            {
                Player.Instance.coin += totalSellPrice;
            }

            // Проверяем, есть ли место в магазине для добавления предмета
            bool shopIsFull = !_shopInventory.ContainsKey(item) && _shopInventory.Count >= maxShopSlots;

            if (shopIsFull)
            {
                Debug.LogWarning($"ShopInteraction: магазин заполнен ({maxShopSlots} слотов). Предмет продан, но магазин его не принимает.");
                return;
            }

            // Добавляем предмет в магазин (есть место)
            if (_shopInventory.ContainsKey(item))
            {
                _shopInventory[item] += count;
            }
            else
            {
                _shopInventory[item] = count;
                // Добавляем цену если её нет
                bool priceExists = false;
                for (int i = 0; i < itemPrices.Count; i++)
                {
                    if (itemPrices[i].item == item)
                    {
                        priceExists = true;
                        break;
                    }
                }
                if (!priceExists)
                {
                    int price = GetItemPrice(item);
                    itemPrices.Add(new ShopItemPrice { item = item, price = price });
                }
            }

            UpdateShopUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка продажи - {e.Message}");
        }
    }

    public void OpenShopPublic()
    {
        if (_isPlayerNearby)
            OpenShop();
    }

    public void CloseShopPublic() => CloseShop();

    public bool IsShopOpen() => _isShopOpen;
    public bool IsPlayerNearby() => _isPlayerNearby;

    /// <summary>
    /// Очищает все предметы из магазина
    /// </summary>
    public void ClearShopInventory()
    {
        _shopInventory.Clear();
        UpdateShopUI();
    }

    /// <summary>
    /// Добавляет предмет в магазин
    /// </summary>
    public void AddItemToShop(Item item, int count, int price)
    {
        if (item == null || count <= 0) return;

        // Проверяем лимит слотов
        if (!_shopInventory.ContainsKey(item) && _shopInventory.Count >= maxShopSlots)
        {
            Debug.LogWarning($"ShopInteraction: магазин заполнен ({maxShopSlots} слотов).");
            return;
        }

        if (_shopInventory.ContainsKey(item))
        {
            _shopInventory[item] += count;
        }
        else
        {
            _shopInventory[item] = count;
        }

        // Обновляем или добавляем цену
        bool priceExists = false;
        for (int i = 0; i < itemPrices.Count; i++)
        {
            if (itemPrices[i].item == item)
            {
                var updated = itemPrices[i];
                updated.price = price;
                itemPrices[i] = updated;
                priceExists = true;
                break;
            }
        }
        if (!priceExists)
        {
            itemPrices.Add(new ShopItemPrice { item = item, price = price });
        }

        UpdateShopUI();
    }

    void OnDestroy()
    {
        if (_isShopOpen && shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (_isInitialized && shopPanel != null && _isShopOpen)
        {
            CloseShop();
        }
    }
}
