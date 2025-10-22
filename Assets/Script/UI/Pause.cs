using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame)
            {
                Resume();
            }   
            else
            {
                PauseM();
            }
        }
    }

    public void Resume()
    {
        pauseGameMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;
    }

    public void PauseM()
    {
        pauseGameMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        PauseGame = true;
    }

    public void LoadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
