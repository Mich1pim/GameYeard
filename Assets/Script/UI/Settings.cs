using UnityEngine;

public static class Settings
{
    private const string ResolutionXKey = "ResolutionX";
    private const string ResolutionYKey = "ResolutionY";
    private const string FullscreenKey = "Fullscreen";

    public static int GetResolutionX()
    {
        return PlayerPrefs.GetInt(ResolutionXKey, Screen.currentResolution.width);
    }

    public static int GetResolutionY()
    {
        return PlayerPrefs.GetInt(ResolutionYKey, Screen.currentResolution.height);
    }

    public static bool GetFullscreen()
    {
        return PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
    }

    public static void SetResolution(int width, int height, bool fullscreen)
    {
        PlayerPrefs.SetInt(ResolutionXKey, width);
        PlayerPrefs.SetInt(ResolutionYKey, height);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Screen.SetResolution(width, height, fullscreen);
    }

    public static Resolution[] GetAvailableResolutions()
    {
        return Screen.resolutions;
    }
}
