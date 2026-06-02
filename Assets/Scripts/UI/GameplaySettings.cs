using System;
using System.IO;
using UnityEngine;

public static class GameplaySettings
{
    private const string FileName = "gameplay-settings.json";

    private static GameplaySaveData _data;
    private static bool _loaded;

    public static float Sensitivity
    {
        get { EnsureLoaded(); return _data.sensitivity; }
        set { EnsureLoaded(); _data.sensitivity = Mathf.Clamp(value, 0.1f, 10f); Save(); }
    }

    public static float Fov
    {
        get { EnsureLoaded(); return _data.fov; }
        set { EnsureLoaded(); _data.fov = Mathf.Clamp(value, 60f, 120f); Save(); }
    }

    public static bool ShowViewmodel
    {
        get { EnsureLoaded(); return _data.showViewmodel; }
        set { EnsureLoaded(); _data.showViewmodel = value; Save(); }
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;

        string path = Path.Combine(Application.persistentDataPath, FileName);
        if (File.Exists(path))
        {
            try { _data = JsonUtility.FromJson<GameplaySaveData>(File.ReadAllText(path)); }
            catch { _data = null; }
        }

        if (_data == null) _data = new GameplaySaveData();
        _data.Sanitize();
        _loaded = true;
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(
                Path.Combine(Application.persistentDataPath, FileName),
                JsonUtility.ToJson(_data, true));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to save gameplay settings. {ex.Message}");
        }
    }

    [Serializable]
    private class GameplaySaveData
    {
        public float sensitivity = 1f;
        public float fov = 68f;
        public bool showViewmodel = true;

        public void Sanitize()
        {
            sensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
            fov = Mathf.Clamp(fov, 60f, 120f);
        }
    }
}
