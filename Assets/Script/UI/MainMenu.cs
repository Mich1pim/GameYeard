using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MainMap");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
