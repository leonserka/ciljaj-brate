using UnityEngine;
using UnityEditor;

public class PrefireHitboxEditor : EditorWindow
{
    private const string PrefabPath = "Assets/Prefabs/PrefireBot.prefab";

    private GameObject _prefab;
    private Transform _headTransform;
    private SphereCollider _headSphere;
    private SerializedObject _serializedSphere;

    [MenuItem("Game/Prefire Hitbox Editor")]
    public static void Open()
    {
        var win = GetWindow<PrefireHitboxEditor>("Hitbox Editor");
        win.minSize = new Vector2(320, 220);
        win.Show();
    }

    private void OnEnable()
    {
        LoadPrefab();
        SceneView.duringSceneGui += DrawSceneGizmos;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGizmos;
    }

    private void LoadPrefab()
    {
        _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (_prefab == null) return;
        _headTransform = _prefab.transform.Find("Head");
        if (_headTransform == null) return;
        _headSphere = _headTransform.GetComponent<SphereCollider>();
        if (_headSphere != null)
            _serializedSphere = new SerializedObject(_headSphere);
    }

    private void OnGUI()
    {
        if (_headSphere == null)
        {
            LoadPrefab();
            if (_headSphere == null)
            {
                EditorGUILayout.HelpBox("PrefireBot prefab or Head child not found at " + PrefabPath, MessageType.Error);
                return;
            }
        }

        EditorGUILayout.LabelField("Bot Head Sphere (local coords of Head child)", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        _serializedSphere.Update();
        EditorGUI.BeginChangeCheck();

        var centerProp = _serializedSphere.FindProperty("m_Center");
        var radiusProp = _serializedSphere.FindProperty("m_Radius");
        EditorGUILayout.PropertyField(centerProp, new GUIContent("Center"));
        radiusProp.floatValue = EditorGUILayout.Slider("Radius", radiusProp.floatValue, 0.05f, 0.5f);

        bool changed = EditorGUI.EndChangeCheck();
        if (changed)
        {
            _serializedSphere.ApplyModifiedProperties();
            EditorUtility.SetDirty(_headSphere);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("World-space result (bot at origin):", EditorStyles.miniLabel);
        var wc = _headTransform.TransformPoint(_headSphere.center);
        float wr = _headSphere.radius * _headTransform.lossyScale.x;
        EditorGUILayout.LabelField($"  center  y={wc.y:F3}   z={wc.z:F3}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"  radius  {wr:F3}  (covers y {wc.y - wr:F3} – {wc.y + wr:F3})", EditorStyles.miniLabel);

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Save Prefab"))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (changed) SceneView.RepaintAll();
    }

    private void DrawSceneGizmos(SceneView sv)
    {

        if (Application.isPlaying)
        {
            var bots = FindObjectsByType<PrefireBot>(FindObjectsSortMode.None);
            foreach (var bot in bots)
                DrawBotGizmos(bot.transform);
        }
        else if (_headSphere != null)
        {
            DrawSphere(_headTransform.TransformPoint(_headSphere.center),
                       _headSphere.radius * _headTransform.lossyScale.x,
                       new Color(1f, 0.15f, 0.15f, 0.9f));
        }
    }

    private static void DrawBotGizmos(Transform botRoot)
    {
        var head = botRoot.Find("Head");
        if (head == null) return;
        var sc = head.GetComponent<SphereCollider>();
        if (sc == null) return;
        DrawSphere(head.TransformPoint(sc.center), sc.radius * head.lossyScale.x, new Color(1f, 0.15f, 0.15f, 0.9f));
    }

    private static void DrawSphere(Vector3 center, float radius, Color col)
    {
        Handles.color = col;
        Handles.DrawWireDisc(center, Vector3.up, radius);
        Handles.DrawWireDisc(center, Vector3.forward, radius);
        Handles.DrawWireDisc(center, Vector3.right, radius);
    }
}
