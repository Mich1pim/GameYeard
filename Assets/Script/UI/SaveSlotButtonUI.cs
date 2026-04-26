using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButtonUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI dayLabel;
    public Button deleteButton;

    private int _slotIndex;

    void Awake()
    {
        GetComponent<Button>()?.onClick.AddListener(OnClick);

        if (deleteButton == null)
            deleteButton = transform.Find("DeleteButton")?.GetComponent<Button>();

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteClick);
            deleteButton.gameObject.SetActive(false);
        }
    }

    public void Refresh(int slotIndex, bool isSaveMode)
    {
        _slotIndex = slotIndex;

        if (slotLabel != null)
            slotLabel.text = $"Слот {slotIndex}";

        var meta = SaveManager.Instance?.GetSaveMetadata(slotIndex);
        bool hasSave = meta != null && meta.hasSave;

        if (timeLabel != null)
            timeLabel.text = hasSave ? meta.saveTime : (isSaveMode ? "Пусто" : "Нет сохранения");

        if (dayLabel != null)
            dayLabel.text = hasSave ? $"День {meta.day}" : "";

        var btn = GetComponent<Button>();
        if (btn != null && !isSaveMode)
            btn.interactable = hasSave;

        if (deleteButton != null)
            deleteButton.gameObject.SetActive(hasSave);
    }

    public void OnClick()
    {
        SaveSlotsPanelUI.Instance?.OnSlotClicked(_slotIndex);
    }

    void OnDeleteClick()
    {
        SaveSlotsPanelUI.Instance?.RequestDeleteSlot(_slotIndex);
    }
}
