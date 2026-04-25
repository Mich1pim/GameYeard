using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SaveFeedback : MonoBehaviour
{
    public static SaveFeedback Instance { get; private set; }

    private GameObject _feedbackPanel;
    private RectTransform _spinner;
    private bool _isShowing;

    void Awake() => Instance = this;

    void Start()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var panel = canvas.transform.Find("SaveFeedback");
            if (panel != null)
            {
                _feedbackPanel = panel.gameObject;
                var spinnerGO = panel.Find("Spinner");
                if (spinnerGO != null) _spinner = spinnerGO.GetComponent<RectTransform>();
            }
        }
        if (_feedbackPanel != null)
            _feedbackPanel.SetActive(false);
    }

    void Update()
    {
        if (_isShowing && _spinner != null)
            _spinner.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime);
    }

    public void Show()
    {
        if (_isShowing) return;
        StartCoroutine(ShowCoroutine());
    }

    IEnumerator ShowCoroutine()
    {
        _isShowing = true;
        _feedbackPanel?.SetActive(true);

        // Отключаем кнопки паузы на время показа
        var pauseMenu = GameObject.Find("PauseMenu");
        if (pauseMenu != null)
            foreach (var btn in pauseMenu.GetComponentsInChildren<Button>())
                btn.interactable = false;

        // Блокируем весь игровой ввод на 1 секунду
        Player.Instance?.DisableInput();
        FindObjectOfType<Atack>()?.DisableInput();
        FindObjectOfType<InventoryOpen>()?.DisableInput();

        yield return new WaitForSecondsRealtime(1f);

        // Возвращаем кнопки паузы
        if (pauseMenu != null)
            foreach (var btn in pauseMenu.GetComponentsInChildren<Button>())
                btn.interactable = true;

        // Оставляем надпись ещё полсекунды, потом скрываем
        yield return new WaitForSecondsRealtime(0.5f);

        _feedbackPanel?.SetActive(false);
        _isShowing = false;

        // Возвращаем ввод (игрок всё ещё в паузе, так что Pause.Resume() потом вернёт его сам)
        Player.Instance?.EnableInput();
        FindObjectOfType<Atack>()?.EnableInput();
        FindObjectOfType<InventoryOpen>()?.EnableInput();
    }
}
