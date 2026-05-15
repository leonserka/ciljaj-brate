using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject dotPreset;
    [SerializeField] private GameObject crossPreset;
    [SerializeField] private Color color = Color.white;
    [SerializeField, Range(0, 1)] private int preset;

    private void Awake()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public void SetPreset(int newPreset)
    {
        preset = Mathf.Clamp(newPreset, 0, 1);
        Apply();
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
        Apply();
    }

    private void Apply()
    {
        if (dotPreset != null)
            dotPreset.SetActive(preset == 0);

        if (crossPreset != null)
            crossPreset.SetActive(preset == 1);

        ApplyColor(dotPreset);
        ApplyColor(crossPreset);
    }

    private void ApplyColor(GameObject root)
    {
        if (root == null)
            return;

        foreach (Image image in root.GetComponentsInChildren<Image>(true))
            image.color = color;
    }
}
