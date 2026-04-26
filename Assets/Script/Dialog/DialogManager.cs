using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogEntry
{
    [TextArea(1, 4)] public string npcText;
    public string[] playerChoices;
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Typewriter")]
    [SerializeField] private float typewriterSpeed = 0.03f;

    private DialogEntry[] _entries;
    private string _speakerName;
    private int _currentIndex;
    private bool _isActive;
    private bool _isTyping;
    private bool _awaitingChoice;
    private Coroutine _typewriterCoroutine;
    private readonly List<GameObject> _choiceButtons = new List<GameObject>();
    private readonly List<RectTransform> _choiceRects = new List<RectTransform>();

    public int LastChoiceIndex { get; private set; } = -1;

    private void Awake()
    {
        Instance = this;
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private System.Action _onComplete;

    public void StartDialog(string speakerName, DialogEntry[] entries, System.Action onComplete = null)
    {
        if (_isActive) return;
        if (entries == null || entries.Length == 0) return;

        _speakerName = speakerName;
        _entries = entries;
        _currentIndex = 0;
        _isActive = true;
        _onComplete = onComplete;

        dialogPanel.SetActive(true);
        DisableAllInput();
        ShowEntry(_currentIndex);
    }

    private void Update()
    {
        if (!_isActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (_awaitingChoice)
        {
            // Проверяем, попал ли клик на одну из кнопок
            for (int i = 0; i < _choiceRects.Count; i++)
            {
                if (_choiceRects[i] == null) continue;
                Camera cam = dialogPanel.GetComponentInParent<Canvas>()?.worldCamera;
                if (RectTransformUtility.RectangleContainsScreenPoint(_choiceRects[i], Input.mousePosition, cam))
                {
                    OnChoiceSelected(i);
                    return;
                }
            }
            return; // клик мимо кнопок — ничего не делаем
        }

        if (_isTyping)
        {
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            dialogText.text = _entries[_currentIndex].npcText;
            _isTyping = false;
            TryShowChoices(_currentIndex);
        }
        else
        {
            Advance();
        }
    }

    private void ShowEntry(int index)
    {
        ClearChoices();
        _awaitingChoice = false;

        if (speakerNameText != null)
            speakerNameText.text = _speakerName;

        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_entries[index].npcText, () =>
        {
            TryShowChoices(index);
        }));
    }

    private void TryShowChoices(int index)
    {
        var choices = _entries[index].playerChoices;
        if (choices == null || choices.Length == 0) return;

        _awaitingChoice = true;
        foreach (string choice in choices)
        {
            var btnGo = Instantiate(choiceButtonPrefab, choicesContainer);
            _choiceButtons.Add(btnGo);
            var tmp = btnGo.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = choice;
            var btn = btnGo.GetComponent<Button>();
            int capturedIndex = _choiceButtons.Count - 1;
            if (btn != null) btn.onClick.AddListener(() => OnChoiceSelected(capturedIndex));
            var rect = btnGo.GetComponent<RectTransform>();
            if (rect != null) _choiceRects.Add(rect);
        }
    }

    private void OnChoiceSelected(int index = 0)
    {
        if (!_awaitingChoice) return;
        LastChoiceIndex = index;
        ClearChoices();
        _awaitingChoice = false;
        Advance();
    }

    private void Advance()
    {
        _currentIndex++;
        if (_currentIndex >= _entries.Length)
            EndDialog();
        else
            ShowEntry(_currentIndex);
    }

    private void ClearChoices()
    {
        foreach (var btn in _choiceButtons)
            if (btn != null) Destroy(btn);
        _choiceButtons.Clear();
        _choiceRects.Clear();
    }

    private IEnumerator TypewriterEffect(string line, System.Action onComplete)
    {
        _isTyping = true;
        dialogText.text = "";
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        _isTyping = false;
        onComplete?.Invoke();
    }

    public void EndDialog()
    {
        if (!_isActive) return;
        _isActive = false;
        _awaitingChoice = false;

        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        ClearChoices();
        dialogPanel.SetActive(false);
        EnableAllInput();

        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }

    private void DisableAllInput()
    {
        Player.Instance?.DisableInput();
        FindObjectOfType<Atack>()?.DisableInput();
        FindObjectOfType<InventoryOpen>()?.DisableInput();
    }

    private void EnableAllInput()
    {
        Player.Instance?.EnableInput();
        FindObjectOfType<Atack>()?.EnableInput();
        FindObjectOfType<InventoryOpen>()?.EnableInput();
    }

    public bool IsActive => _isActive;
}
