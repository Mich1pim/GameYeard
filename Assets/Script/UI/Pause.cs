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
            if (saveSlotsPanel != null && saveSlotsPanel.gameObject.activeSelf)
            {
                saveSlotsPanel.Close();
                return;
            }

            if (PauseGame)
                Resume();
            else
                PauseM();
        }
    }

    public void Resume()
    {
        pauseGameMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;

        if (Player.Instance != null)
            Player.Instance.EnableInput();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.EnableInput();
        if (FindObjectOfType<Atack>() != null)
            FindObjectOfType<Atack>().EnableInput();
        if (FindObjectOfType<InventoryOpen>() != null)
            FindObjectOfType<InventoryOpen>().EnableInput();
    }

    public void PauseM()
    {
        pauseGameMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
        PauseGame = true;

        if (Player.Instance != null)
            Player.Instance.DisableInput();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.DisableInput();
        if (FindObjectOfType<Atack>() != null)
            FindObjectOfType<Atack>().DisableInput();
        if (FindObjectOfType<InventoryOpen>() != null)
            FindObjectOfType<InventoryOpen>().DisableInput();
    }

    public void LoadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Сохранить игру (вызывается из кнопки в меню паузы)
    /// </summary>
    [Header("Save Slots Panel")]
    public SaveSlotsPanelUI saveSlotsPanel;

    public void OpenSaveSlotsPanel()
    {
        if (saveSlotsPanel != null)
            saveSlotsPanel.OpenForSave();
        else
            Debug.LogError("[Pause] saveSlotsPanel не назначен!");
    }
}
