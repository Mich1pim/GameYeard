using UnityEngine;

/// <summary>
/// Скрипт для кнопки сохранения в меню паузы.
/// Вешается на кнопку "Сохранить" в панели паузы.
/// </summary>
public class SaveGameButton : MonoBehaviour
{
    public void OpenSaveSlotsPanel()
    {
        var panel = FindObjectOfType<SaveSlotsPanelUI>(true);
        if (panel != null)
            panel.OpenForSave();
        else
            Debug.LogError("[SaveGameButton] SaveSlotsPanelUI not found!");
    }
}
