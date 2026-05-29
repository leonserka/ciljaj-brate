using UnityEngine;

public static class AimModeSelection
{
    private const string SelectedModeKey = "AimTraining.SelectedMode";

    public static void SelectMode(string modeName)
    {
        if (string.IsNullOrWhiteSpace(modeName))
            return;

        PlayerPrefs.SetString(SelectedModeKey, Normalize(modeName));
        PlayerPrefs.Save();
    }

    public static GameModeSO ResolveSelectedMode(GameModeSO fallback, GameModeSO[] availableModes)
    {
        string selected = PlayerPrefs.GetString(SelectedModeKey, string.Empty);
        if (string.IsNullOrWhiteSpace(selected))
            return fallback;

        GameModeSO match = FindByName(selected, availableModes);
        return match != null ? match : fallback;
    }

    public static bool IsSelected(GameModeSO mode)
    {
        if (mode == null)
            return false;

        return IsSelected(mode.ModeName);
    }

    public static bool IsSelected(string modeName)
    {
        if (string.IsNullOrWhiteSpace(modeName))
            return false;

        string selected = PlayerPrefs.GetString(SelectedModeKey, string.Empty);
        return Normalize(modeName) == selected;
    }

    private static GameModeSO FindByName(string selected, GameModeSO[] availableModes)
    {
        if (availableModes == null)
            return null;

        for (int i = 0; i < availableModes.Length; i++)
        {
            GameModeSO mode = availableModes[i];
            if (mode != null && Normalize(mode.ModeName) == selected)
                return mode;
        }

        return null;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
