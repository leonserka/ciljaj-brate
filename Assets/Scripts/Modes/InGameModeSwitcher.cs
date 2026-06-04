using UnityEngine;

public class InGameModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameModeSO[] availableModes;
    [SerializeField] private GameObject modeButtonPrefab;
    [SerializeField] private float spacing = 2.2f;

    private void Start()
    {
        BuildModeButtons();
    }

    private void BuildModeButtons()
    {
        if (availableModes == null || availableModes.Length == 0 || modeButtonPrefab == null) return;

        float startX = -(availableModes.Length - 1) * spacing / 2f;

        for (int i = 0; i < availableModes.Length; i++)
        {
            var mode = availableModes[i];
            if (mode == null) continue;

            var btnGO = Instantiate(modeButtonPrefab, transform);
            btnGO.name = "ModeBtn_" + mode.ModeName;
            btnGO.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            btnGO.layer = gameObject.layer;

            foreach (Transform child in btnGO.transform)
                child.gameObject.layer = gameObject.layer;

            var label = btnGO.GetComponentInChildren<TextMesh>();
            if (label != null) label.text = mode.ModeName.ToUpper();

            var shootable = btnGO.GetComponent<ShootableModeButton>();
            if (shootable != null) shootable.Init(mode);
        }
    }
}
