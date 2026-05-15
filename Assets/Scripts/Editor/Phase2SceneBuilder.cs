using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Phase2SceneBuilder
{
    private const string AimTrainingScenePath = "Assets/Scenes/AimTraining.unity";
    private const string TargetPrefabPath = "Assets/Prefabs/Targets/Target.prefab";
    private const string SingleStaticModePath = "Assets/ScriptableObjects/Modes/SingleStaticMode.asset";
    private const string TargetMaterialPath = "Assets/Materials/Targets/M_Target_Pink.mat";

    [MenuItem("Ciljaj Brate/Phase 2/Build Vertical Slice")]
    public static void Build()
    {
        EnsureFolders();

        GameObject targetPrefab = CreateTargetPrefab();
        SingleStaticMode singleStaticMode = CreateSingleStaticMode();

        Scene scene = EditorSceneManager.OpenScene(AimTrainingScenePath, OpenSceneMode.Single);
        RemovePhaseOneTargets();

        GameObject root = GameObject.Find("Phase2_SingleStatic");
        if (root != null)
            Object.DestroyImmediate(root);

        root = new GameObject("Phase2_SingleStatic");

        StatsManager statsManager = new GameObject("StatsManager").AddComponent<StatsManager>();
        statsManager.transform.SetParent(root.transform);

        TargetSpawner spawner = new GameObject("TargetSpawner").AddComponent<TargetSpawner>();
        spawner.transform.SetParent(root.transform);
        spawner.transform.position = new Vector3(0f, 2.6f, 9.2f);
        SetSerialized(spawner, "targetPrefab", targetPrefab);
        SetSerialized(spawner, "spawnBoxSize", new Vector3(5.5f, 2.6f, 0f));
        SetSerialized(spawner, "minimumDistanceFromLast", 1.35f);

        ModeManager modeManager = new GameObject("ModeManager").AddComponent<ModeManager>();
        modeManager.transform.SetParent(root.transform);
        SetSerialized(modeManager, "activeMode", singleStaticMode);
        SetSerialized(modeManager, "targetSpawner", spawner);
        SetSerialized(modeManager, "statsManager", statsManager);

        CreateHud(root.transform, modeManager, statsManager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Targets");
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects/Modes");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static GameObject CreateTargetPrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);
        if (existing != null)
            return existing;

        GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetObject.name = "Target";
        targetObject.transform.localScale = Vector3.one * 0.55f;

        Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(TargetMaterialPath);
        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer != null && targetMaterial != null)
            renderer.sharedMaterial = targetMaterial;

        targetObject.AddComponent<Target>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(targetObject, TargetPrefabPath);
        Object.DestroyImmediate(targetObject);
        return prefab;
    }

    private static SingleStaticMode CreateSingleStaticMode()
    {
        SingleStaticMode mode = AssetDatabase.LoadAssetAtPath<SingleStaticMode>(SingleStaticModePath);
        if (mode == null)
        {
            mode = ScriptableObject.CreateInstance<SingleStaticMode>();
            AssetDatabase.CreateAsset(mode, SingleStaticModePath);
        }

        SetSerialized(mode, "modeName", "Single Static");
        EditorUtility.SetDirty(mode);
        return mode;
    }

    private static void RemovePhaseOneTargets()
    {
        foreach (TestShootableTarget testTarget in Object.FindObjectsByType<TestShootableTarget>(FindObjectsInactive.Exclude))
            Object.DestroyImmediate(testTarget.gameObject);

        for (int i = 1; i <= 5; i++)
        {
            GameObject target = GameObject.Find($"Target{i}");
            if (target != null)
                Object.DestroyImmediate(target);
        }
    }

    private static void CreateHud(Transform root, ModeManager modeManager, StatsManager statsManager)
    {
        GameObject canvasObject = new GameObject("HUD Canvas");
        canvasObject.transform.SetParent(root);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        Text modeText = CreateText(canvasObject.transform, "ModeText", "Single Static", font, new Vector2(32f, -28f), 32);
        Text scoreText = CreateText(canvasObject.transform, "ScoreText", "Score 0", font, new Vector2(32f, -72f), 28);
        Text accuracyText = CreateText(canvasObject.transform, "AccuracyText", "Acc 0%  H 0 / S 0", font, new Vector2(32f, -110f), 24);
        Text timerText = CreateText(canvasObject.transform, "TimerText", "00:00", font, new Vector2(32f, -146f), 24);

        HUD hud = canvasObject.AddComponent<HUD>();
        SetSerialized(hud, "modeManager", modeManager);
        SetSerialized(hud, "statsManager", statsManager);
        SetSerialized(hud, "modeText", modeText);
        SetSerialized(hud, "scoreText", scoreText);
        SetSerialized(hud, "accuracyText", accuracyText);
        SetSerialized(hud, "timerText", timerText);
    }

    private static Text CreateText(Transform parent, string name, string text, Font font, Vector2 anchoredPosition, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 36f);

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(0.05f, 0.05f, 0.06f, 1f);
        label.raycastTarget = false;
        return label;
    }

    private static void SetSerialized(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetSerialized(Object target, string propertyName, Vector3 value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).vector3Value = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetSerialized(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetSerialized(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }
}

public static class Phase2RuntimeSmoke
{
    private const int KillCount = 100;
    private const double KillIntervalSeconds = 0.12d;

    private static int _killsDone;
    private static double _nextKillTime;
    private static bool _waitingForFinalRespawn;

    [MenuItem("Ciljaj Brate/Phase 2/Run Runtime Smoke")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("PHASE2_SMOKE_FAIL: enter Play Mode before running the Phase 2 runtime smoke.");
            return;
        }

        _killsDone = 0;
        _waitingForFinalRespawn = false;
        _nextKillTime = EditorApplication.timeSinceStartup + KillIntervalSeconds;

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Debug.Log("PHASE2_SMOKE_START");
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying)
        {
            Finish("PHASE2_SMOKE_FAIL: Play Mode stopped before smoke completed.");
            return;
        }

        if (EditorApplication.timeSinceStartup < _nextKillTime)
            return;

        if (_waitingForFinalRespawn)
        {
            Target[] liveTargets = Object.FindObjectsByType<Target>(FindObjectsInactive.Exclude);
            StatsManager statsManager = Object.FindAnyObjectByType<StatsManager>();
            SessionStats stats = statsManager != null ? statsManager.CurrentStats : default;
            string message = liveTargets.Length == 1 && stats.ShotsFired == KillCount && stats.Hits == KillCount
                ? $"PHASE2_SMOKE_PASS: kills={_killsDone};liveTargets={liveTargets.Length};score={stats.Score}"
                : $"PHASE2_SMOKE_FAIL: kills={_killsDone};liveTargets={liveTargets.Length};shots={stats.ShotsFired};hits={stats.Hits}";

            Finish(message);
            return;
        }

        Target target = Object.FindAnyObjectByType<Target>();
        ModeManager modeManager = Object.FindAnyObjectByType<ModeManager>();
        if (target == null || modeManager == null)
        {
            Finish("PHASE2_SMOKE_FAIL: missing target or ModeManager.");
            return;
        }

        modeManager.HandleShot(true);
        target.OnShot(default, Vector3.forward);
        _killsDone++;

        if (_killsDone >= KillCount)
        {
            _waitingForFinalRespawn = true;
            _nextKillTime = EditorApplication.timeSinceStartup + 0.25d;
            return;
        }

        _nextKillTime = EditorApplication.timeSinceStartup + KillIntervalSeconds;
    }

    private static void Finish(string message)
    {
        EditorApplication.update -= Tick;
        Debug.Log(message);
    }
}
