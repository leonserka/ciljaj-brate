using UnityEngine;
using UnityEngine.UI;

public class CrosshairApplier : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;
    [SerializeField] private GameObject defaultCrosshair;
    private Sprite[] _sprites;

    private void Awake()
    {
        _sprites = Resources.LoadAll<Sprite>("Crosshairs");
        System.Array.Sort(_sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
    }

    private void Start()
    {
        Apply();
    }

    public void Apply()
    {
        if (_sprites == null || _sprites.Length == 0) return;

        int idx = Mathf.Clamp(CrosshairSettings.CrosshairIndex, 0, _sprites.Length - 1);
        Color col = CrosshairSettings.CrosshairColor;

        if (crosshairImage != null)
        {
            crosshairImage.sprite = _sprites[idx];
            crosshairImage.color = col;
            float size = CrosshairSettings.CrosshairSize;
            crosshairImage.rectTransform.sizeDelta = new Vector2(size, size);
        }

        if (defaultCrosshair != null)
            defaultCrosshair.SetActive(false);
    }
}
