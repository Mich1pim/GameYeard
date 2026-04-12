using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Settings Menu")]
    public GameObject settingsMenu;

    [Header("Save System")]
    public GameObject loadButton;           // Кнопка "Загрузить" (показывается если есть сохранение)

    void Start()
    {
        UpdateLoadButtonVisibility();
    }

    private void UpdateLoadButtonVisibility()
    {
        if (loadButton != null)
        {
            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave();
            loadButton.SetActive(hasSave);
        }
    }

    public void StartGame()
    {
        // Помечаем что это новая игра (не загружаем сохранение)
        PlayerPrefs.SetInt("new_game", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMap");
    }

    /// <summary>
    /// Загрузить последнее сохранение и начать игру
    /// </summary>
    public void LoadGame()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasSave())
        {
            Debug.LogWarning("[MainMenu] No save found to load");
            return;
        }

        PlayerPrefs.SetInt("new_game", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMap");
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
