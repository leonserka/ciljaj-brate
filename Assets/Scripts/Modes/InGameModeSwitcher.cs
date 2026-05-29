using UnityEngine;

public class InGameModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameModeSO[] availableModes;

    private ModeManager _manager;

    private void Start()
    {
        _manager = ModeManager.Current;
        BuildModeButtons();
    }

    private void BuildModeButtons()
    {
        if (availableModes == null || availableModes.Length == 0) return;

        float spacing = 2.2f;
        float startX = -(availableModes.Length - 1) * spacing / 2f;

        for (int i = 0; i < availableModes.Length; i++)
        {
            var mode = availableModes[i];
            if (mode == null) continue;

            var btnGO = new GameObject("ModeBtn_" + mode.ModeName);
            btnGO.transform.SetParent(transform, false);
            btnGO.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            btnGO.layer = gameObject.layer;

            // Background quad
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "BG";
            quad.transform.SetParent(btnGO.transform, false);
            quad.transform.localScale = new Vector3(2f, 0.8f, 1f);
            quad.layer = gameObject.layer;
            var bgMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bgMat.color = new Color(0.12f, 0.12f, 0.14f, 1f);
            quad.GetComponent<Renderer>().material = bgMat;

            // Label using 3D text
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            labelGO.transform.localPosition = new Vector3(0, 0, -0.01f);
            var tm = labelGO.AddComponent<TextMesh>();
            tm.text = mode.ModeName.ToUpper();
            tm.fontSize = 32;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;

            // Shootable component
            var shootable = btnGO.AddComponent<ShootableModeButton>();
            shootable.Init(mode);
        }
    }
}
