using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Transform crosshairGrid;

    [Header("Preview")]
    [SerializeField] private Image crosshairPreview;
    [SerializeField] private Image colorPreview;

    [Header("Sliders")]
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider sizeSlider;

    private Sprite[] _sprites;
    private int _selectedIndex;
    private Color _selectedColor;

    private static AudioClip _hoverClip;
    private static AudioClip _clickClip;
    private static AudioSource _uiAudio;

    private void Awake()
    {
        _sprites = Resources.LoadAll<Sprite>("Crosshairs");
        System.Array.Sort(_sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        if (_hoverClip == null) _hoverClip = Resources.Load<AudioClip>("UI/hover");
        if (_clickClip == null) _clickClip = Resources.Load<AudioClip>("UI/click");
        if (_uiAudio == null)
        {
            var existing = GameObject.Find("UIAudio");
            if (existing != null) _uiAudio = existing.GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        _selectedIndex = CrosshairSettings.CrosshairIndex;
        _selectedColor = CrosshairSettings.CrosshairColor;

        if (redSlider != null) { redSlider.value = _selectedColor.r; redSlider.onValueChanged.AddListener(OnColorChanged); }
        if (greenSlider != null) { greenSlider.value = _selectedColor.g; greenSlider.onValueChanged.AddListener(OnColorChanged); }
        if (blueSlider != null) { blueSlider.value = _selectedColor.b; blueSlider.onValueChanged.AddListener(OnColorChanged); }
        if (sizeSlider != null) { sizeSlider.value = CrosshairSettings.CrosshairSize; sizeSlider.onValueChanged.AddListener(OnSizeChanged); }

        PopulateGrid();
        UpdatePreview();
    }

    private void OnDisable()
    {
        if (redSlider != null) redSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (greenSlider != null) greenSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (blueSlider != null) blueSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (sizeSlider != null) sizeSlider.onValueChanged.RemoveListener(OnSizeChanged);
    }

    private void PopulateGrid()
    {
        if (crosshairGrid == null || _sprites == null) return;

        foreach (Transform child in crosshairGrid)
            Destroy(child.gameObject);

        for (int i = 0; i < _sprites.Length; i++)
        {
            var cell = new GameObject("CH_" + i, typeof(RectTransform));
            cell.transform.SetParent(crosshairGrid, false);

            var cellImg = cell.AddComponent<Image>();
            cellImg.color = i == _selectedIndex
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(1f, 1f, 1f, 0.03f);
            cellImg.raycastTarget = true;

            var icon = new GameObject("Icon", typeof(RectTransform));
            icon.transform.SetParent(cell.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = icon.AddComponent<Image>();
            iconImg.sprite = _sprites[i];
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            int idx = i;
            var btn = cell.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { PlayClick(); SelectCrosshair(idx); });

            var hover = cell.AddComponent<CrosshairCellHover>();
            hover.Init(cellImg, i == _selectedIndex);
        }
    }

    private void SelectCrosshair(int index)
    {
        _selectedIndex = index;
        CrosshairSettings.CrosshairIndex = index;
        UpdatePreview();
        HighlightSelected();
    }

    private void HighlightSelected()
    {
        for (int i = 0; i < crosshairGrid.childCount; i++)
        {
            var cell = crosshairGrid.GetChild(i);
            var cellImg = cell.GetComponent<Image>();
            var hover = cell.GetComponent<CrosshairCellHover>();
            bool selected = i == _selectedIndex;
            if (hover != null) hover.SetSelected(selected);
            if (cellImg != null && hover != null && !hover.IsHovered)
                cellImg.color = selected ? new Color(1f, 1f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.03f);
        }
    }

    private void OnColorChanged(float _)
    {
        _selectedColor = new Color(
            redSlider != null ? redSlider.value : 1f,
            greenSlider != null ? greenSlider.value : 1f,
            blueSlider != null ? blueSlider.value : 1f, 1f);
        CrosshairSettings.CrosshairColor = _selectedColor;
        UpdatePreview();
    }

    private void OnSizeChanged(float val)
    {
        CrosshairSettings.CrosshairSize = val;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (crosshairPreview != null && _sprites != null && _selectedIndex < _sprites.Length)
        {
            crosshairPreview.sprite = _sprites[_selectedIndex];
            crosshairPreview.color = _selectedColor;
            float size = CrosshairSettings.CrosshairSize;
            crosshairPreview.rectTransform.sizeDelta = new Vector2(size, size);
        }
        if (colorPreview != null)
            colorPreview.color = _selectedColor;
    }

    private static void PlayClick()
    {
        if (_clickClip != null && _uiAudio != null)
            _uiAudio.PlayOneShot(_clickClip);
    }
}
