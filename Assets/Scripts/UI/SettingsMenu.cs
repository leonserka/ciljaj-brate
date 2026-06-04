using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private Transform crosshairGrid;
    [SerializeField] private Image crosshairPreview;
    [SerializeField] private Image colorPreview;
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider sizeSlider;

    [Header("Crosshair Scene Elements")]
    [SerializeField] private GameObject gridPanel;
    [SerializeField] private GameObject bottomBar;

    [Header("Sidebar")]
    [SerializeField] private Image crosshairTabBg;
    [SerializeField] private Image gameplayTabBg;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Gameplay")]
    [SerializeField] private GameObject gameplayContent;
    [SerializeField] private Slider sensSlider;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text sensValueText;
    [SerializeField] private TMP_Text fovValueText;
    [SerializeField] private Toggle viewmodelToggle;

    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(0.271f, 0f, 0.125f);
    [SerializeField] private Color inactiveColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color hoverColor = new Color(0.22f, 0.08f, 0.14f, 0.6f);

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

        WireEvents();
    }

    private void WireEvents()
    {
        if (crosshairTabBg != null)
        {
            var btn = crosshairTabBg.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ShowCrosshairTab);
            var hover = crosshairTabBg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.Init(crosshairTabBg, true, inactiveColor, hoverColor, activeColor);
        }

        if (gameplayTabBg != null)
        {
            var btn = gameplayTabBg.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ShowGameplayTab);
            var hover = gameplayTabBg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.Init(gameplayTabBg, false, inactiveColor, hoverColor, activeColor);
        }

        if (sensSlider != null) sensSlider.onValueChanged.AddListener(v =>
        {
            float rounded = Mathf.Round(v * 100f) / 100f;
            GameplaySettings.Sensitivity = rounded;
            if (sensValueText != null) sensValueText.text = rounded.ToString("F2");
        });

        if (fovSlider != null) fovSlider.onValueChanged.AddListener(v =>
        {
            GameplaySettings.Fov = v;
            if (fovValueText != null) fovValueText.text = Mathf.RoundToInt(v).ToString() + "°";
        });

        if (viewmodelToggle != null)
            viewmodelToggle.onValueChanged.AddListener(v => GameplaySettings.ShowViewmodel = v);
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
        SyncGameplayWidgets();
        ShowCrosshairTab();
    }

    private void OnDisable()
    {
        if (redSlider != null) redSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (greenSlider != null) greenSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (blueSlider != null) blueSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (sizeSlider != null) sizeSlider.onValueChanged.RemoveListener(OnSizeChanged);
    }

    // ---------------------------------------------------------------- tab switching

    private void ShowCrosshairTab()
    {
        if (gridPanel != null) gridPanel.SetActive(true);
        if (bottomBar != null) bottomBar.SetActive(true);
        if (gameplayContent != null) gameplayContent.SetActive(false);
        if (subtitleText != null) subtitleText.text = "CROSSHAIR";
        UpdateSidebarState(true);
    }

    private void ShowGameplayTab()
    {
        if (gridPanel != null) gridPanel.SetActive(false);
        if (bottomBar != null) bottomBar.SetActive(false);
        if (gameplayContent != null) gameplayContent.SetActive(true);
        if (subtitleText != null) subtitleText.text = "GAMEPLAY";
        UpdateSidebarState(false);
        SyncGameplayWidgets();
    }

    private void UpdateSidebarState(bool crosshairActive)
    {
        if (crosshairTabBg != null)
        {
            crosshairTabBg.color = crosshairActive ? activeColor : inactiveColor;
            var h = crosshairTabBg.GetComponent<SidebarItemHover>();
            if (h != null) h.SetActive(crosshairActive);
            var t = crosshairTabBg.GetComponentInChildren<TMP_Text>();
            if (t != null) t.color = crosshairActive ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        }
        if (gameplayTabBg != null)
        {
            gameplayTabBg.color = !crosshairActive ? activeColor : inactiveColor;
            var h = gameplayTabBg.GetComponent<SidebarItemHover>();
            if (h != null) h.SetActive(!crosshairActive);
            var t = gameplayTabBg.GetComponentInChildren<TMP_Text>();
            if (t != null) t.color = !crosshairActive ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        }
    }

    // ---------------------------------------------------------------- gameplay

    private void SyncGameplayWidgets()
    {
        if (sensSlider != null)
        {
            sensSlider.SetValueWithoutNotify(GameplaySettings.Sensitivity);
            if (sensValueText != null) sensValueText.text = GameplaySettings.Sensitivity.ToString("F2");
        }
        if (fovSlider != null)
        {
            fovSlider.SetValueWithoutNotify(GameplaySettings.Fov);
            if (fovValueText != null) fovValueText.text = Mathf.RoundToInt(GameplaySettings.Fov).ToString() + "°";
        }
        if (viewmodelToggle != null)
            viewmodelToggle.SetIsOnWithoutNotify(GameplaySettings.ShowViewmodel);
    }

    // ---------------------------------------------------------------- crosshair

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
            cellImg.color = i == _selectedIndex ? new Color(1f, 1f, 1f, 0.12f) : new Color(1f, 1f, 1f, 0.03f);
            cellImg.raycastTarget = true;

            var icon = new GameObject("Icon", typeof(RectTransform));
            icon.transform.SetParent(cell.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            var iconImg = icon.AddComponent<Image>();
            iconImg.sprite = _sprites[i];
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            int idx = i;
            var btn = cell.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { PlayClick(); SelectCrosshair(idx); });
            cell.AddComponent<CrosshairCellHover>().Init(cellImg, i == _selectedIndex);
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
