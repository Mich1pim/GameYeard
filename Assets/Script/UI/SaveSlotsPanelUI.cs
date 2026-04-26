using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotsPanelUI : MonoBehaviour
{
    public static SaveSlotsPanelUI Instance { get; private set; }

    [Header("Кнопки слотов")]
    public SaveSlotButtonUI slot1;
    public SaveSlotButtonUI slot2;
    public SaveSlotButtonUI slot3;

    [Header("Подтверждение удаления")]
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;

    private bool _isSaveMode;
    private int _pendingDeleteSlot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var closeBtn = transform.Find("Container/CloseButton")?.GetComponent<UnityEngine.UI.Button>();
        if (closeBtn != null) closeBtn.onClick.AddListener(Close);

        if (confirmPanel == null)
            confirmPanel = transform.Find("Container/ConfirmPanel")?.gameObject;

        if (confirmText == null)
            confirmText = transform.Find("Container/ConfirmPanel/ConfirmText")?.GetComponent<TMPro.TextMeshProUGUI>();

        var yesBtn = transform.Find("Container/ConfirmPanel/YesButton")?.GetComponent<UnityEngine.UI.Button>();
        if (yesBtn != null) yesBtn.onClick.AddListener(ConfirmDelete);

        var noBtn = transform.Find("Container/ConfirmPanel/NoButton")?.GetComponent<UnityEngine.UI.Button>();
        if (noBtn != null) noBtn.onClick.AddListener(CancelDelete);

        confirmPanel?.SetActive(false);
    }

    public void OpenForSave()
    {
        _isSaveMode = true;
        RefreshSlots();
        gameObject.SetActive(true);
    }

    public void OpenForLoad()
    {
        _isSaveMode = false;
        RefreshSlots();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        confirmPanel?.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (_isSaveMode)
        {
            SaveManager.Instance?.SaveGame(slotIndex);
            Close();
            SaveFeedback.Instance?.Show();
        }
        else
        {
            PlayerPrefs.SetInt("new_game", 0);
            PlayerPrefs.SetInt("load_slot", slotIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MainMap");
        }
    }

    public void RequestDeleteSlot(int slotIndex)
    {
        _pendingDeleteSlot = slotIndex;
        if (confirmText != null)
            confirmText.text = $"Удалить сохранение\nв Слоте {slotIndex}?";
        confirmPanel?.SetActive(true);
    }

    public void ConfirmDelete()
    {
        SaveManager.Instance?.DeleteSave(_pendingDeleteSlot);
        confirmPanel?.SetActive(false);
        RefreshSlots();
    }

    public void CancelDelete()
    {
        confirmPanel?.SetActive(false);
    }

    private void RefreshSlots()
    {
        slot1?.Refresh(1, _isSaveMode);
        slot2?.Refresh(2, _isSaveMode);
        slot3?.Refresh(3, _isSaveMode);
    }
}
