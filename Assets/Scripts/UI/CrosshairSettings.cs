using System;
using System.IO;
using UnityEngine;

public static class CrosshairSettings
{
    private const string FileName = "crosshair-settings.json";
    private const string KeyIndex = "Crosshair_Index";
    private const string KeySize = "Crosshair_Size";
    private const string KeyR = "Crosshair_R";
    private const string KeyG = "Crosshair_G";
    private const string KeyB = "Crosshair_B";

    private static CrosshairSaveData _data;
    private static bool _loaded;

    public static string SettingsPath => Path.Combine(Application.persistentDataPath, FileName);

    public static float CrosshairSize
    {
        get
        {
            EnsureLoaded();
            return _data.size;
        }
        set
        {
            EnsureLoaded();
            _data.size = Mathf.Clamp(value, 12f, 120f);
            Save();
        }
    }

    public static int CrosshairIndex
    {
        get
        {
            EnsureLoaded();
            return _data.index;
        }
        set
        {
            EnsureLoaded();
            _data.index = Mathf.Max(0, value);
            Save();
        }
    }

    public static Color CrosshairColor
    {
        get
        {
            EnsureLoaded();
            return new Color(_data.r, _data.g, _data.b, 1f);
        }
        set
        {
            EnsureLoaded();
            _data.r = Mathf.Clamp01(value.r);
            _data.g = Mathf.Clamp01(value.g);
            _data.b = Mathf.Clamp01(value.b);
            Save();
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
            return;

        _data = LoadFromJson();
        if (_data == null)
            _data = LoadFromPlayerPrefs();

        _data.Sanitize();
        _loaded = true;
        Save();
    }

    private static CrosshairSaveData LoadFromJson()
    {
        string path = SettingsPath;
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonUtility.FromJson<CrosshairSaveData>(File.ReadAllText(path));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to load crosshair settings JSON. Defaults will be used. {ex.Message}");
            return null;
        }
    }

    private static CrosshairSaveData LoadFromPlayerPrefs()
    {
        return new CrosshairSaveData
        {
            index = PlayerPrefs.GetInt(KeyIndex, 0),
            size = PlayerPrefs.GetFloat(KeySize, 32f),
            r = PlayerPrefs.GetFloat(KeyR, 1f),
            g = PlayerPrefs.GetFloat(KeyG, 1f),
            b = PlayerPrefs.GetFloat(KeyB, 1f)
        };
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to save crosshair settings JSON. {ex.Message}");
        }
    }

    [Serializable]
    private class CrosshairSaveData
    {
        public int index = 0;
        public float size = 32f;
        public float r = 1f;
        public float g = 1f;
        public float b = 1f;

        public void Sanitize()
        {
            index = Mathf.Max(0, index);
            size = Mathf.Clamp(size, 12f, 120f);
            r = Mathf.Clamp01(r);
            g = Mathf.Clamp01(g);
            b = Mathf.Clamp01(b);
        }
    }
}
