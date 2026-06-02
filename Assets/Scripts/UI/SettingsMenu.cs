using TMPro;
using UnityEngine;
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

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f);
    private static readonly Color SidebarBG = Color.clear;
    private static readonly Color SidebarItemBG = new Color(0f, 0f, 0f, 0f);
    private static readonly Color SidebarHover = new Color(0.22f, 0.08f, 0.14f, 0.6f);
    private static readonly Color SliderFill = new Color(0.45f, 0.04f, 0.16f);

    private GameObject _sidebarPanel;
    private GameObject _gameplayContent;
    private Image _sidebarCrosshairImg;
    private Image _sidebarGameplayImg;
    private TMP_Text _subtitleText;

    private Slider _sensSlider;
    private Slider _fovSlider;
    private Toggle _viewmodelToggle;
    private TMP_Text _sensValue;
    private TMP_Text _fovValue;

    // scene elements to show/hide
    private GameObject _gridPanel;
    private GameObject _bottomBar;

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

        CacheSceneElements();
        BuildSidebar();
        BuildGameplayContent();
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

        if (_sidebarPanel != null) _sidebarPanel.SetActive(true);
    }

    private void OnDisable()
    {
        if (redSlider != null) redSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (greenSlider != null) greenSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (blueSlider != null) blueSlider.onValueChanged.RemoveListener(OnColorChanged);
        if (sizeSlider != null) sizeSlider.onValueChanged.RemoveListener(OnSizeChanged);

        if (_sidebarPanel != null) _sidebarPanel.SetActive(false);
    }

    // ---------------------------------------------------------------- scene element caching

    private void CacheSceneElements()
    {
        var content = transform.Find("Content");
        if (content == null) return;

        var label = content.Find("CrosshairLabel");
        if (label != null) _subtitleText = label.GetComponent<TMP_Text>();

        _gridPanel = content.Find("GridPanel")?.gameObject;
        _bottomBar = content.Find("BottomBar")?.gameObject;
    }

    // ---------------------------------------------------------------- sidebar

    private void BuildSidebar()
    {
        var content = transform.Find("Content");
        if (content == null) return;
        var contentRT = content.GetComponent<RectTransform>();

        _sidebarPanel = new GameObject("SettingsSidebar", typeof(RectTransform), typeof(Image));
        _sidebarPanel.transform.SetParent(transform, false);

        float contentLeft = contentRT.anchorMin.x;

        var sbRT = _sidebarPanel.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(contentLeft - 0.07f, contentRT.anchorMax.y - 0.30f);
        sbRT.anchorMax = new Vector2(contentLeft - 0.005f, contentRT.anchorMax.y - 0.13f);
        sbRT.offsetMin = Vector2.zero;
        sbRT.offsetMax = Vector2.zero;

        _sidebarPanel.GetComponent<Image>().color = Color.clear;

        float itemY = -2f;
        _sidebarCrosshairImg = MakeSidebarItem(_sidebarPanel.transform, "CROSSHAIR", itemY, true, ShowCrosshairTab);
        itemY -= 44f;

        // Separator line
        var sep = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(_sidebarPanel.transform, false);
        var sepRT = sep.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0.1f, 1); sepRT.anchorMax = new Vector2(0.9f, 1);
        sepRT.pivot = new Vector2(0.5f, 1);
        sepRT.sizeDelta = new Vector2(0, 1);
        sepRT.anchoredPosition = new Vector2(0, itemY + 1);
        sep.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        _sidebarGameplayImg = MakeSidebarItem(_sidebarPanel.transform, "GAMEPLAY", itemY, false, ShowGameplayTab);

        _sidebarPanel.SetActive(false);
    }

    private Image MakeSidebarItem(Transform parent, string label, float yPos, bool active,
        UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "_Item", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = active ? Cherry : SidebarItemBG;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 42);
        rt.anchoredPosition = new Vector2(0, yPos);

        var txtGO = new GameObject("Label", typeof(RectTransform));
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 15;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = active ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        tmp.raycastTarget = false;
        var tRT = txtGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(16, 0); tRT.offsetMax = Vector2.zero;

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(onClick);

        go.AddComponent<SidebarItemHover>().Init(img, active, SidebarItemBG, SidebarHover, Cherry);

        return img;
    }

    // ---------------------------------------------------------------- tab switching

    private void ShowCrosshairTab()
    {
        if (_gridPanel != null) _gridPanel.SetActive(true);
        if (_bottomBar != null) _bottomBar.SetActive(true);
        if (_gameplayContent != null) _gameplayContent.SetActive(false);

        if (_subtitleText != null) _subtitleText.text = "CROSSHAIR";

        UpdateSidebarState(true);
    }

    private void ShowGameplayTab()
    {
        if (_gridPanel != null) _gridPanel.SetActive(false);
        if (_bottomBar != null) _bottomBar.SetActive(false);
        if (_gameplayContent != null) _gameplayContent.SetActive(true);

        if (_subtitleText != null) _subtitleText.text = "GAMEPLAY";

        UpdateSidebarState(false);
        SyncGameplayWidgets();
    }

    private void UpdateSidebarState(bool crosshairActive)
    {
        if (_sidebarCrosshairImg != null)
        {
            var hover = _sidebarCrosshairImg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.SetActive(crosshairActive);
            var txt = _sidebarCrosshairImg.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.color = crosshairActive ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        }
        if (_sidebarGameplayImg != null)
        {
            var hover = _sidebarGameplayImg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.SetActive(!crosshairActive);
            var txt = _sidebarGameplayImg.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.color = !crosshairActive ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        }
    }

    // ---------------------------------------------------------------- gameplay content

    private void BuildGameplayContent()
    {
        var content = transform.Find("Content");
        if (content == null) return;

        _gameplayContent = new GameObject("GameplayContent", typeof(RectTransform));
        _gameplayContent.transform.SetParent(content, false);
        var rt = _gameplayContent.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.22f);
        rt.anchorMax = new Vector2(0.95f, 0.88f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        float y = 0f;

        y = MakeSettingsSlider(_gameplayContent.transform, "SENSITIVITY", y,
            0.1f, 10f, GameplaySettings.Sensitivity, false,
            out _sensSlider, out _sensValue);
        _sensSlider.onValueChanged.AddListener(v =>
        {
            float rounded = Mathf.Round(v * 100f) / 100f;
            GameplaySettings.Sensitivity = rounded;
            _sensValue.text = rounded.ToString("F2");
        });

        y -= 24f;

        y = MakeSettingsSlider(_gameplayContent.transform, "FIELD OF VIEW", y,
            60f, 120f, GameplaySettings.Fov, true,
            out _fovSlider, out _fovValue);
        _fovSlider.onValueChanged.AddListener(v =>
        {
            GameplaySettings.Fov = v;
            _fovValue.text = Mathf.RoundToInt(v).ToString() + "°";
        });

        y -= 24f;

        MakeSettingsToggle(_gameplayContent.transform, "SHOW WEAPON VIEWMODEL", y,
            GameplaySettings.ShowViewmodel, out _viewmodelToggle);
        _viewmodelToggle.onValueChanged.AddListener(v => GameplaySettings.ShowViewmodel = v);

        _gameplayContent.SetActive(false);
    }

    private void SyncGameplayWidgets()
    {
        if (_sensSlider != null)
        {
            _sensSlider.SetValueWithoutNotify(GameplaySettings.Sensitivity);
            _sensValue.text = GameplaySettings.Sensitivity.ToString("F2");
        }
        if (_fovSlider != null)
        {
            _fovSlider.SetValueWithoutNotify(GameplaySettings.Fov);
            _fovValue.text = Mathf.RoundToInt(GameplaySettings.Fov).ToString() + "°";
        }
        if (_viewmodelToggle != null)
            _viewmodelToggle.SetIsOnWithoutNotify(GameplaySettings.ShowViewmodel);
    }

    private float MakeSettingsSlider(Transform parent, string label, float yPos,
        float min, float max, float initial, bool wholeNumbers,
        out Slider slider, out TMP_Text valueText)
    {
        var labelGO = new GameObject(label + "_Label", typeof(RectTransform));
        labelGO.transform.SetParent(parent, false);
        var lbl = labelGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = 14;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.color = new Color(0.8f, 0.8f, 0.82f);
        lbl.raycastTarget = false;
        var lRT = labelGO.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0, 1); lRT.anchorMax = new Vector2(0.7f, 1);
        lRT.pivot = new Vector2(0, 1); lRT.sizeDelta = new Vector2(0, 24);
        lRT.anchoredPosition = new Vector2(0, yPos);

        var valGO = new GameObject(label + "_Value", typeof(RectTransform));
        valGO.transform.SetParent(parent, false);
        valueText = valGO.AddComponent<TextMeshProUGUI>();
        valueText.fontSize = 14;
        valueText.alignment = TextAlignmentOptions.Right;
        valueText.color = new Color(0.9f, 0.9f, 0.9f);
        valueText.raycastTarget = false;
        var vRT = valGO.GetComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0.7f, 1); vRT.anchorMax = new Vector2(1, 1);
        vRT.pivot = new Vector2(1, 1); vRT.sizeDelta = new Vector2(0, 24);
        vRT.anchoredPosition = new Vector2(0, yPos);

        yPos -= 28f;

        var sliderGO = new GameObject(label + "_Slider", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(parent, false);
        var sRT = sliderGO.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0, 1); sRT.anchorMax = new Vector2(1, 1);
        sRT.pivot = new Vector2(0.5f, 1); sRT.sizeDelta = new Vector2(0, 16);
        sRT.anchoredPosition = new Vector2(0, yPos);

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        Stretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 0); fRT.anchorMax = new Vector2(0, 1);
        fRT.sizeDelta = new Vector2(10, 0);
        fill.GetComponent<Image>().color = SliderFill;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGO.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var hRT = handle.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(14, 22);
        handle.GetComponent<Image>().color = Color.white;

        slider = sliderGO.GetComponent<Slider>();
        slider.fillRect = fRT;
        slider.handleRect = hRT;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.SetValueWithoutNotify(initial);

        yPos -= 24f;
        return yPos;
    }

    private void MakeSettingsToggle(Transform parent, string label, float yPos,
        bool initial, out Toggle toggle)
    {
        var row = new GameObject(label + "_Row", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var rRT = row.GetComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0, 1); rRT.anchorMax = new Vector2(1, 1);
        rRT.pivot = new Vector2(0, 1); rRT.sizeDelta = new Vector2(0, 32);
        rRT.anchoredPosition = new Vector2(0, yPos);

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(row.transform, false);
        var lbl = labelGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = 14;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.color = new Color(0.8f, 0.8f, 0.82f);
        lbl.raycastTarget = false;
        var lRT = labelGO.GetComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = new Vector2(0.8f, 1);
        lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;

        var toggleGO = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
        toggleGO.transform.SetParent(row.transform, false);
        var tRT = toggleGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(1, 0.5f); tRT.anchorMax = new Vector2(1, 0.5f);
        tRT.pivot = new Vector2(1, 0.5f); tRT.sizeDelta = new Vector2(40, 24);

        var boxBG = new GameObject("Background", typeof(RectTransform), typeof(Image));
        boxBG.transform.SetParent(toggleGO.transform, false);
        Stretch(boxBG.GetComponent<RectTransform>());
        boxBG.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(boxBG.transform, false);
        var cRT = checkGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.1f, 0.1f); cRT.anchorMax = new Vector2(0.9f, 0.9f);
        cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;
        checkGO.GetComponent<Image>().color = Cherry;

        toggle = toggleGO.GetComponent<Toggle>();
        toggle.targetGraphic = boxBG.GetComponent<Image>();
        toggle.graphic = checkGO.GetComponent<Image>();
        toggle.isOn = initial;
    }

    // ---------------------------------------------------------------- crosshair grid

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

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
