using UnityEngine;

public class NPCDialog : MonoBehaviour, ISaveable
{
    [Header("NPC")]
    [SerializeField] private string objectId = "NPC_Stranger";
    [SerializeField] private string npcName = "Незнакомец";

    [Header("Эффект исчезновения")]
    [SerializeField] private GameObject disappearEffectPrefab;

    [Header("Диалог")]
    [SerializeField] private DialogEntry[] dialogEntries = new DialogEntry[]
    {
        new DialogEntry {
            npcText = "...Ты очнулся. Хорошо.\nЯ уже думал, что ты не выживешь.",
            playerChoices = new[] { "Где я?", "Что произошло?", "Кто ты?" }
        },
        new DialogEntry {
            npcText = "Это место называют по-разному. Долиной. Забытым краем.\nЗдесь нет ни городов, ни дорог, ни людей.\nТолько земля, небо... и то, что приходит с темнотой.",
            playerChoices = new[] { "То, что приходит с темнотой?", "Мне нужно выбраться.", "Как долго я здесь?" }
        },
        new DialogEntry {
            npcText = "О ночи пока не думай. Сначала — живи.\nЗемля здесь помнит руки. Старые грядки ещё можно поднять.\nСемена найдёшь, если поищешь.",
            playerChoices = null
        },
        new DialogEntry {
            npcText = "Я сам здесь давно. Давно перестал считать дни.\nНо знаю одно — этот край проверяет. Всех проверяет.",
            playerChoices = new[] { "Как проверяет?", "Я справлюсь.", "Есть ли выход отсюда?" }
        },
        new DialogEntry {
            npcText = "Следи за небом — оно расскажет больше, чем я.\nДождь кормит землю. Солнце её греет.\nБуря... буря это уже другой разговор.",
            playerChoices = null
        },
        new DialogEntry {
            npcText = "Что привело тебя сюда? Я не знаю.\nТы, наверное, тоже не помнишь.",
            playerChoices = new[] { "Ты прав. Я не помню.", "Я что-то искал.", "Это была ошибка." }
        },
        new DialogEntry {
            npcText = "Начни с малого. Посади что-то. Построй что-то своё.\nВыживи одну ночь — потом другую. Остальное придёт само. Или не придёт. Это уж как пойдёт.",
            playerChoices = null
        }
    };

    private bool _vanished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_vanished) return;
        if (!other.CompareTag("Player")) return;
        if (DialogManager.Instance == null) return;
        if (DialogManager.Instance.IsActive) return;

        DialogManager.Instance.StartDialog(npcName, dialogEntries, OnDialogComplete);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // Не прерываем диалог при отходе — он должен завершиться сам
    }

    private void OnDialogComplete()
    {
        Vanish(playEffect: true);
    }

    private void Vanish(bool playEffect)
    {
        if (_vanished) return;
        _vanished = true;

        if (playEffect && disappearEffectPrefab != null)
            Instantiate(disappearEffectPrefab, transform.position, Quaternion.identity);

        // Скрываем визуал и отключаем коллайдеры/скрипты, но не сам GO
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;

        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        enabled = false; // отключаем сам NPCDialog
    }

    // ─── ISaveable ───────────────────────────────────────────────

    public string GetObjectId() => objectId;

    public WorldObjectData GetSaveData() => new WorldObjectData
    {
        objectId = objectId,
        objectType = "NPCDialog",
        isDead = _vanished
    };

    public void LoadData(WorldObjectData data)
    {
        if (data.isDead)
            Vanish(playEffect: false);
    }
}
