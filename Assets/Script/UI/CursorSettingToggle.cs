using UnityEngine;
using UnityEngine.UI;

public class CursorSettingToggle : MonoBehaviour
{
    public Toggle _toggle;

    void Start()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle == null) return;

        _toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(CursorManager.PrefKey, 1) == 1);
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool value)
    {
        PlayerPrefs.SetInt(CursorManager.PrefKey, value ? 1 : 0);
        PlayerPrefs.Save();
        ApplyCursorSetting(value);
    }

    void ApplyCursorSetting(bool useCustom)
    {
        var cm = FindObjectOfType<CursorManager>(true);
        if (cm != null)
            cm.enabled = useCustom;

        if (!useCustom)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
