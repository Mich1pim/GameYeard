using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Settings Menu")]
    public GameObject settingsMenu;

    public void StartGame()
    {
        SceneManager.LoadScene("MainMap");
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
