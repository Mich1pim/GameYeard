using UnityEngine;
using UnityEngine.UI;

public class CursorSettingToggle : MonoBehaviour
{
    private Toggle _toggle;

    void Start()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle == null) return;

        _toggle.isOn = PlayerPrefs.GetInt(CursorManager.PrefKey, 1) == 1;
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool value)
    {
        PlayerPrefs.SetInt(CursorManager.PrefKey, value ? 1 : 0);
        PlayerPrefs.Save();

        var cm = FindObjectOfType<CursorManager>(true);
        if (cm != null)
        {
            cm.enabled = value;
            if (!value)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
