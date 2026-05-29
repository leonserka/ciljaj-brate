using UnityEngine;

public static class CrosshairSettings
{
    private const string KeyIndex = "Crosshair_Index";
    private const string KeySize = "Crosshair_Size";
    private const string KeyR = "Crosshair_R";
    private const string KeyG = "Crosshair_G";
    private const string KeyB = "Crosshair_B";

    public static float CrosshairSize
    {
        get => PlayerPrefs.GetFloat(KeySize, 32f);
        set { PlayerPrefs.SetFloat(KeySize, value); PlayerPrefs.Save(); }
    }

    public static int CrosshairIndex
    {
        get => PlayerPrefs.GetInt(KeyIndex, 0);
        set { PlayerPrefs.SetInt(KeyIndex, value); PlayerPrefs.Save(); }
    }

    public static Color CrosshairColor
    {
        get => new Color(
            PlayerPrefs.GetFloat(KeyR, 1f),
            PlayerPrefs.GetFloat(KeyG, 1f),
            PlayerPrefs.GetFloat(KeyB, 1f),
            1f);
        set
        {
            PlayerPrefs.SetFloat(KeyR, value.r);
            PlayerPrefs.SetFloat(KeyG, value.g);
            PlayerPrefs.SetFloat(KeyB, value.b);
            PlayerPrefs.Save();
        }
    }
}
