using UnityEngine;

public class MerchantDialog : MonoBehaviour, ISaveable
{
    [Header("Настройки")]
    [SerializeField] private string objectId = "Merchant_Trader";
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string merchantName = "Торговец";

    [Header("Магазин")]
    [SerializeField] private ShopInteraction shop;

    private const string QUEST_ITEM = "Docka";
    private const int QUEST_AMOUNT = 16;
    private const int QUEST_REWARD = 50;

    private enum QuestState { NotAccepted, Active, Completed }
    private QuestState _quest = QuestState.NotAccepted;

    private Transform _playerTransform;

    private void Start()
    {
        var player = GameObject.Find("Player");
        if (player != null) _playerTransform = player.transform;

        if (shop != null) shop.interactionEnabled = false;
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        if (DialogManager.Instance != null && DialogManager.Instance.IsActive) return;
        if (shop != null && shop.IsShopOpen()) return;

        float dist = Vector3.Distance(_playerTransform.position, transform.position);
        if (dist > interactionDistance) return;
        if (!Input.GetKeyDown(interactionKey)) return;

        StartGreetingDialog();
    }

    private void StartGreetingDialog()
    {
        string text;
        string[] choices;

        switch (_quest)
        {
            case QuestState.Active when CountItem(QUEST_ITEM) >= QUEST_AMOUNT:
                text = "О, ты вернулся! Принёс доски?";
                choices = new[] { "Вот твои доски!", "Ещё подожди" };
                break;
            case QuestState.Active:
                text = $"Как продвигается? Мне нужны доски... ({CountItem(QUEST_ITEM)}/{QUEST_AMOUNT})";
                choices = new[] { "Скоро принесу", "Хочу посмотреть товары" };
                break;
            case QuestState.Completed:
                text = "Снова ты! Рад видеть. Что будем делать?";
                choices = new[] { "Посмотреть товары", "Ничего, спасибо" };
                break;
            default:
                text = "Привет, путник! Давно не видел живой души в этих краях.\nЧем могу помочь?";
                choices = new[] { "Хочу посмотреть товары", "Есть ли у тебя задание?" };
                break;
        }

        DialogManager.Instance.StartDialog(merchantName, new[]
        {
            new DialogEntry { npcText = text, playerChoices = choices }
        }, OnGreetingComplete);
    }

    private void OnGreetingComplete()
    {
        int choice = DialogManager.Instance.LastChoiceIndex;

        switch (_quest)
        {
            case QuestState.Active when CountItem(QUEST_ITEM) >= QUEST_AMOUNT:
                if (choice == 0) CompleteQuest();
                break;
            case QuestState.Active:
                if (choice == 1) OpenShop();
                break;
            case QuestState.Completed:
                if (choice == 0) OpenShop();
                break;
            default:
                if (choice == 0) OpenShop();
                else StartQuestOfferDialog();
                break;
        }
    }

    private void StartQuestOfferDialog()
    {
        DialogManager.Instance.StartDialog(merchantName, new[]
        {
            new DialogEntry
            {
                npcText = "Задание? Пожалуй...\nМне нужны доски — 16 штук.\nПринесёшь — получишь 50 монет.",
                playerChoices = new[] { "Принять задание", "Откажусь" }
            }
        }, () =>
        {
            if (DialogManager.Instance.LastChoiceIndex == 0)
                _quest = QuestState.Active;
        });
    }

    private void CompleteQuest()
    {
        RemoveItems(QUEST_ITEM, QUEST_AMOUNT);
        if (Player.Instance != null) Player.Instance.coin += QUEST_REWARD;
        _quest = QuestState.Completed;

        DialogManager.Instance.StartDialog(merchantName, new[]
        {
            new DialogEntry
            {
                npcText = "Отлично! Вот твои 50 монет.\nПриятно иметь дело с надёжным человеком.",
                playerChoices = null
            }
        }, null);
    }

    private void OpenShop() => shop?.OpenShop();

    private int CountItem(string itemName)
    {
        int total = 0;
        if (InventoryManager.Instance == null) return 0;
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            var inv = slot.GetComponentInChildren<InventoryItem>();
            if (inv != null && inv.item != null && inv.item.name == itemName)
                total += inv.count;
        }
        return total;
    }

    private void RemoveItems(string itemName, int amount)
    {
        if (InventoryManager.Instance == null) return;
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            if (amount <= 0) break;
            var inv = slot.GetComponentInChildren<InventoryItem>();
            if (inv != null && inv.item != null && inv.item.name == itemName)
            {
                int take = Mathf.Min(amount, inv.count);
                inv.count -= take;
                amount -= take;
                if (inv.count <= 0)
                    Destroy(inv.gameObject);
                else
                    inv.RefreshCount();
            }
        }
    }

    // ─── ISaveable ───────────────────────────────────────────────

    public string GetObjectId() => objectId;

    public WorldObjectData GetSaveData() => new WorldObjectData
    {
        objectId = objectId,
        objectType = "MerchantDialog",
        questState = (int)_quest
    };

    public void LoadData(WorldObjectData data)
    {
        _quest = (QuestState)data.questState;
    }
}
