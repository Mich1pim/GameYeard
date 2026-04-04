using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;
    private Resolution currentResolution;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        resolutions = Settings.GetAvailableResolutions();
        currentResolution.width = Settings.GetResolutionX();
        currentResolution.height = Settings.GetResolutionY();

        SetupResolutionDropdown();
        SetupFullscreenToggle();
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));

            if (resolutions[i].width == currentResolution.width &&
                resolutions[i].height == currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupFullscreenToggle()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Settings.GetFullscreen();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ApplyResolution()
    {
        if (resolutionDropdown == null) return;

        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        bool fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : true;

        Settings.SetResolution(selectedResolution.width, selectedResolution.height, fullscreen);
    }

    public void ApplySettings()
    {
        ApplyResolution();
        CloseSettings();
    }
}
