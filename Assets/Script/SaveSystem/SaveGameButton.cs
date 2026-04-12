using UnityEngine;

/// <summary>
/// Скрипт для кнопки сохранения в меню паузы.
/// Вешается на кнопку "Сохранить" в панели паузы.
/// </summary>
public class SaveGameButton : MonoBehaviour
{
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log("[SaveGameButton] Game saved!");
        }
        else
        {
            Debug.LogError("[SaveGameButton] SaveManager not found!");
        }
    }
}
