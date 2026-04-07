using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] private List<Item> defaultItemsForSale = new List<Item>();
    [SerializeField] private Dictionary<Item, int> itemPrices = new Dictionary<Item, int>();

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
                itemPrices = new Dictionary<Item, int>();
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

        foreach (Item item in defaultItemsForSale)
        {
            if (item == null) continue;

            int price = 10; // Цена по умолчанию
            if (itemPrices.ContainsKey(item))
            {
                price = itemPrices[item];
            }
            else if (itemPrices.Count > 0)
            {
                // Берём среднюю цену
                int totalPrice = 0;
                foreach (var kvp in itemPrices) totalPrice += kvp.Value;
                price = totalPrice / itemPrices.Count;
            }

            _shopInventory[item] = 5; // По умолчанию 5 штук
            itemPrices[item] = price;
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

            shopPanel.SetActive(false);
            _isShopOpen = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShopInteraction: ошибка закрытия - {e.Message}");
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

        // Создаём слоты из префаба InventorySlot
        int slotIndex = 0;
        foreach (var kvp in _shopInventory)
        {
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
                }
            }

            slotIndex++;
        }
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
        if (itemPrices.ContainsKey(item))
        {
            return itemPrices[item];
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

            // Добавляем предмет в магазин
            if (_shopInventory.ContainsKey(item))
            {
                _shopInventory[item] += count;
            }
            else
            {
                _shopInventory[item] = count;
                itemPrices[item] = GetItemPrice(item);
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

        if (_shopInventory.ContainsKey(item))
        {
            _shopInventory[item] += count;
        }
        else
        {
            _shopInventory[item] = count;
        }

        itemPrices[item] = price;
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
