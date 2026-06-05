using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f);
    private static readonly Color PanelBG = new Color(0.10f, 0.10f, 0.12f, 0.98f);
    private static readonly Color ButtonBG = new Color(0.16f, 0.16f, 0.19f, 1f);
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.78f);
    private static readonly Color SidebarBG = new Color(0.08f, 0.08f, 0.10f, 1f);
    private static readonly Color SidebarItemBG = new Color(0.12f, 0.12f, 0.14f, 1f);
    private static readonly Color SidebarHover = new Color(0.22f, 0.08f, 0.14f, 1f);

    private GameObject _root;
    private GameObject _rootView;
    private GameObject _settingsView;
    private GameObject _modeView;
    private GameObject _routeView;
    private bool _open;

    private PlayerLook _look;
    private MonoBehaviour _weapon;
    private PlayerController _controller;
    private bool _weaponWas, _controllerWas;


    private Sprite[] _sprites;
    private int _selectedIndex;
    private Color _selectedColor;
    private Image _preview;
    private Transform _grid;
    private CrosshairApplier _applier;
    private Slider _redSlider, _greenSlider, _blueSlider, _sizeSlider;


    private GameObject _crosshairContent;
    private GameObject _gameplayContent;
    private Image _sidebarCrosshairImg;
    private Image _sidebarGameplayImg;
    private TMP_Text _sidebarCrosshairText;
    private TMP_Text _sidebarGameplayText;


    private Slider _sensSlider, _fovSlider;
    private Toggle _viewmodelToggle;
    private TMP_Text _sensValue, _fovValue;

    private void Awake()
    {
        _sprites = Resources.LoadAll<Sprite>("Crosshairs");
        System.Array.Sort(_sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        EnsureEventSystem();
        BuildUI();
        _root.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_open && (_settingsView.activeSelf || _modeView.activeSelf || _routeView.activeSelf))
                ShowRoot();
            else
                Toggle();
        }
    }



    private void Toggle()
    {
        if (_open) Resume();
        else Pause();
    }

    private void Pause()
    {
        _open = true;
        _root.SetActive(true);
        ShowRoot();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _look = FindAnyObjectByType<PlayerLook>();
        _weapon = FindAnyObjectByType<PrefireWeapon>() as MonoBehaviour
              ?? FindAnyObjectByType<PlayerWeapon>();
        _controller = FindAnyObjectByType<PlayerController>();

        if (_look != null) _look.enabled = false;
        if (_weapon != null) { _weaponWas = _weapon.enabled; _weapon.enabled = false; }
        if (_controller != null) { _controllerWas = _controller.enabled; _controller.enabled = false; }

    }

    private void Resume()
    {
        _open = false;
        _root.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_look != null) _look.enabled = true;
        if (_weapon != null) _weapon.enabled = _weaponWas;
        if (_controller != null) _controller.enabled = _controllerWas;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }



    private void ShowRoot()
    {
        _rootView.SetActive(true);
        _settingsView.SetActive(false);
        _modeView.SetActive(false);
        _routeView.SetActive(false);
    }

    private void ShowSettings()
    {
        _rootView.SetActive(false);
        _settingsView.SetActive(true);
        _modeView.SetActive(false);
        _routeView.SetActive(false);
        ShowSettingsTab(true);
    }

    private void ShowModes()
    {
        _rootView.SetActive(false);
        _settingsView.SetActive(false);
        _modeView.SetActive(true);
        _routeView.SetActive(false);
        UpdateModeButtonStates();
    }

    private void ShowRoutes()
    {
        _rootView.SetActive(false);
        _settingsView.SetActive(false);
        _modeView.SetActive(false);
        _routeView.SetActive(true);
        PopulateRouteGrid();
    }

    private void StartMode(string sceneName, string modeName = null)
    {
        if (!string.IsNullOrEmpty(modeName))
            AimModeSelection.SelectMode(modeName);

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }



    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _root = MakeStretch("Root", canvasGO.transform);
        var dim = _root.AddComponent<Image>();
        dim.color = Dim;

        BuildRootView(_root.transform);
        BuildSettingsView(_root.transform);
        BuildModeView(_root.transform);
        BuildRouteView(_root.transform);
    }



    private void BuildRootView(Transform parent)
    {
        _rootView = MakeStretch("RootView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_rootView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(440, 520);
        panel.GetComponent<Image>().color = PanelBG;

        var title = MakeText("Title", panel.transform, "PAUSED", 44, FontStyles.Bold, TextAlignmentOptions.Center);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(0, 60);
        tr.anchoredPosition = new Vector2(0, -36);
        title.color = Color.white;

        var bar = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panel.transform, false);
        var br = bar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 1); br.anchorMax = new Vector2(0.5f, 1);
        br.pivot = new Vector2(0.5f, 1); br.sizeDelta = new Vector2(60, 3);
        br.anchoredPosition = new Vector2(0, -96);
        bar.GetComponent<Image>().color = Cherry;

        var col = new GameObject("Buttons", typeof(RectTransform));
        col.transform.SetParent(panel.transform, false);
        var cr = col.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f); cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.pivot = new Vector2(0.5f, 0.5f); cr.sizeDelta = new Vector2(340, 300);
        cr.anchoredPosition = new Vector2(0, -30);
        var vlg = col.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        MakeButton("ResumeBtn", col.transform, "RESUME", Cherry).onClick.AddListener(Resume);
        MakeButton("SettingsBtn", col.transform, "SETTINGS", ButtonBG).onClick.AddListener(ShowSettings);

        if (SceneManager.GetActiveScene().name == "Prefire")
            MakeButton("SwitchRouteBtn", col.transform, "SWITCH ROUTE", ButtonBG).onClick.AddListener(ShowRoutes);
        else
            MakeButton("ChangeModeBtn", col.transform, "CHANGE MODE", ButtonBG).onClick.AddListener(ShowModes);

        MakeButton("MenuBtn", col.transform, "MAIN MENU", ButtonBG).onClick.AddListener(GoToMainMenu);

    }



    private void BuildSettingsView(Transform parent)
    {
        _settingsView = MakeStretch("SettingsView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_settingsView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1200, 700);
        panel.GetComponent<Image>().color = PanelBG;


        var back = MakeButton("BackBtn", panel.transform, "BACK", new Color(0.2f, 0.2f, 0.22f, 0.8f));
        var backR = back.GetComponent<RectTransform>();
        backR.anchorMin = backR.anchorMax = new Vector2(1, 1);
        backR.pivot = new Vector2(1, 1);
        backR.sizeDelta = new Vector2(110, 40);
        backR.anchoredPosition = new Vector2(-24, -22);
        back.onClick.AddListener(ShowRoot);


        var sidebar = new GameObject("Sidebar", typeof(RectTransform), typeof(Image));
        sidebar.transform.SetParent(panel.transform, false);
        var sbRT = sidebar.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0, 0);
        sbRT.anchorMax = new Vector2(0, 1);
        sbRT.pivot = new Vector2(0, 0.5f);
        sbRT.sizeDelta = new Vector2(200, 0);
        sbRT.anchoredPosition = Vector2.zero;
        sidebar.GetComponent<Image>().color = SidebarBG;


        var sbTitle = MakeText("SBTitle", sidebar.transform, "SETTINGS", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        var stRT = sbTitle.rectTransform;
        stRT.anchorMin = new Vector2(0, 1); stRT.anchorMax = new Vector2(1, 1);
        stRT.pivot = new Vector2(0, 1); stRT.sizeDelta = new Vector2(0, 50);
        stRT.anchoredPosition = new Vector2(0, 0);
        stRT.offsetMin = new Vector2(20, stRT.offsetMin.y);
        sbTitle.color = Color.white;


        float itemY = -60f;
        MakeSidebarItem(sidebar.transform, "CROSSHAIR", itemY, true, out _sidebarCrosshairImg, out _sidebarCrosshairText,
            () => ShowSettingsTab(true));
        itemY -= 46f;
        MakeSidebarItem(sidebar.transform, "GAMEPLAY", itemY, false, out _sidebarGameplayImg, out _sidebarGameplayText,
            () => ShowSettingsTab(false));


        var contentArea = new GameObject("ContentArea", typeof(RectTransform));
        contentArea.transform.SetParent(panel.transform, false);
        var caRT = contentArea.GetComponent<RectTransform>();
        caRT.anchorMin = new Vector2(0, 0);
        caRT.anchorMax = new Vector2(1, 1);
        caRT.offsetMin = new Vector2(200, 0);
        caRT.offsetMax = Vector2.zero;

        BuildCrosshairContent(contentArea.transform);
        BuildGameplayContent(contentArea.transform);

        _settingsView.SetActive(false);
    }

    private void MakeSidebarItem(Transform parent, string label, float yPos, bool active,
        out Image bgImg, out TMP_Text text, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "_Item", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        bgImg = go.GetComponent<Image>();
        bgImg.color = active ? Cherry : SidebarItemBG;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 42);
        rt.anchoredPosition = new Vector2(0, yPos);

        var txtGO = new GameObject("Label", typeof(RectTransform));
        txtGO.transform.SetParent(go.transform, false);
        text = txtGO.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 15;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Left;
        text.color = active ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        text.raycastTarget = false;
        var tRT = txtGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(20, 0); tRT.offsetMax = Vector2.zero;


        if (active)
        {
            var ind = new GameObject("ActiveBar", typeof(RectTransform), typeof(Image));
            ind.transform.SetParent(go.transform, false);
            var iRT = ind.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0, 0.1f); iRT.anchorMax = new Vector2(0, 0.9f);
            iRT.pivot = new Vector2(0, 0.5f);
            iRT.sizeDelta = new Vector2(3, 0);
            ind.GetComponent<Image>().color = Color.white;
        }

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(onClick);

        go.AddComponent<SidebarItemHover>().Init(bgImg, active, SidebarItemBG, SidebarHover, Cherry);
    }

    private void ShowSettingsTab(bool crosshair)
    {
        if (_crosshairContent != null) _crosshairContent.SetActive(crosshair);
        if (_gameplayContent != null) _gameplayContent.SetActive(!crosshair);

        if (_sidebarCrosshairImg != null)
        {
            _sidebarCrosshairImg.color = crosshair ? Cherry : SidebarItemBG;
            var hover = _sidebarCrosshairImg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.SetActive(crosshair);
        }
        if (_sidebarGameplayImg != null)
        {
            _sidebarGameplayImg.color = !crosshair ? Cherry : SidebarItemBG;
            var hover = _sidebarGameplayImg.GetComponent<SidebarItemHover>();
            if (hover != null) hover.SetActive(!crosshair);
        }
        if (_sidebarCrosshairText != null) _sidebarCrosshairText.color = crosshair ? Color.white : new Color(0.7f, 0.7f, 0.72f);
        if (_sidebarGameplayText != null) _sidebarGameplayText.color = !crosshair ? Color.white : new Color(0.7f, 0.7f, 0.72f);

        if (crosshair)
            SyncCrosshairWidgets();
        else
            SyncGameplayWidgets();
    }



    private void BuildCrosshairContent(Transform parent)
    {
        _crosshairContent = new GameObject("CrosshairContent", typeof(RectTransform));
        _crosshairContent.transform.SetParent(parent, false);
        Stretch(_crosshairContent.GetComponent<RectTransform>());


        var title = MakeText("Title", _crosshairContent.transform, "CROSSHAIR", 32, FontStyles.Bold, TextAlignmentOptions.Left);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(-72, 50);
        tr.anchoredPosition = new Vector2(0, -16);
        title.color = Color.white;

        BuildCrosshairGrid(_crosshairContent.transform);
        BuildCrosshairControls(_crosshairContent.transform);
    }

    private void BuildCrosshairGrid(Transform parent)
    {
        var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(parent, false);
        var sr = scrollGO.GetComponent<RectTransform>();
        sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0.5f, 0.5f);
        sr.sizeDelta = new Vector2(580, 520);
        sr.anchoredPosition = new Vector2(-160, -30);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);

        var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportGO.transform.SetParent(scrollGO.transform, false);
        Stretch(viewportGO.GetComponent<RectTransform>());

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewportGO.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(0, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0, contentRect.offsetMax.y);
        contentRect.anchoredPosition = Vector2.zero;

        var glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(92, 92);
        glg.spacing = new Vector2(12, 12);
        glg.padding = new RectOffset(18, 18, 18, 18);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5;
        glg.childAlignment = TextAnchor.UpperLeft;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = scrollGO.GetComponent<ScrollRect>();
        scroll.viewport = viewportGO.GetComponent<RectTransform>();
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32f;

        _grid = content.transform;
    }

    private void BuildCrosshairControls(Transform parent)
    {
        var side = new GameObject("Controls", typeof(RectTransform));
        side.transform.SetParent(parent, false);
        var sd = side.GetComponent<RectTransform>();
        sd.anchorMin = sd.anchorMax = new Vector2(0.5f, 0.5f);
        sd.pivot = new Vector2(0.5f, 0.5f);
        sd.sizeDelta = new Vector2(340, 520);
        sd.anchoredPosition = new Vector2(310, -30);

        var previewBox = new GameObject("PreviewBox", typeof(RectTransform), typeof(Image));
        previewBox.transform.SetParent(side.transform, false);
        var pb = previewBox.GetComponent<RectTransform>();
        pb.anchorMin = new Vector2(0.5f, 1); pb.anchorMax = new Vector2(0.5f, 1);
        pb.pivot = new Vector2(0.5f, 1); pb.sizeDelta = new Vector2(160, 160);
        pb.anchoredPosition = new Vector2(0, -10);
        previewBox.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        var previewGO = new GameObject("Preview", typeof(RectTransform), typeof(Image));
        previewGO.transform.SetParent(previewBox.transform, false);
        _preview = previewGO.GetComponent<Image>();
        _preview.preserveAspect = true;
        _preview.raycastTarget = false;
        var prv = previewGO.GetComponent<RectTransform>();
        prv.anchorMin = prv.anchorMax = new Vector2(0.5f, 0.5f);

        float y = -200f;
        var rS = MakeSlider("Red", side.transform, y, new Color(0.9f, 0.25f, 0.25f)); y -= 70;
        var gS = MakeSlider("Green", side.transform, y, new Color(0.3f, 0.85f, 0.35f)); y -= 70;
        var bS = MakeSlider("Blue", side.transform, y, new Color(0.3f, 0.5f, 0.95f)); y -= 70;
        var sizeS = MakeSlider("Size", side.transform, y, Cherry);
        sizeS.minValue = 12f; sizeS.maxValue = 96f;

        rS.onValueChanged.AddListener(_ => OnColor(rS, gS, bS));
        gS.onValueChanged.AddListener(_ => OnColor(rS, gS, bS));
        bS.onValueChanged.AddListener(_ => OnColor(rS, gS, bS));
        sizeS.onValueChanged.AddListener(v => { CrosshairSettings.CrosshairSize = v; UpdatePreview(); ApplyLive(); });

        _redSlider = rS; _greenSlider = gS; _blueSlider = bS; _sizeSlider = sizeS;
    }

    private void SyncCrosshairWidgets()
    {
        if (_applier == null) _applier = FindAnyObjectByType<CrosshairApplier>(FindObjectsInactive.Include);

        _selectedIndex = CrosshairSettings.CrosshairIndex;
        _selectedColor = CrosshairSettings.CrosshairColor;

        if (_redSlider != null) _redSlider.SetValueWithoutNotify(_selectedColor.r);
        if (_greenSlider != null) _greenSlider.SetValueWithoutNotify(_selectedColor.g);
        if (_blueSlider != null) _blueSlider.SetValueWithoutNotify(_selectedColor.b);
        if (_sizeSlider != null) _sizeSlider.SetValueWithoutNotify(CrosshairSettings.CrosshairSize);

        PopulateGrid();
        UpdatePreview();
    }

    private void PopulateGrid()
    {
        if (_grid == null || _sprites == null) return;

        foreach (Transform child in _grid)
            Destroy(child.gameObject);

        for (int i = 0; i < _sprites.Length; i++)
        {
            var cell = new GameObject("CH_" + i, typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(_grid, false);
            var cellImg = cell.GetComponent<Image>();
            cellImg.color = i == _selectedIndex ? new Color(1f, 1f, 1f, 0.14f) : new Color(1f, 1f, 1f, 0.04f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(cell.transform, false);
            var ir = icon.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.15f, 0.15f); ir.anchorMax = new Vector2(0.85f, 0.85f);
            ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;
            var iconImg = icon.GetComponent<Image>();
            iconImg.sprite = _sprites[i];
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            int idx = i;
            var btn = cell.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => SelectCrosshair(idx));

            cell.AddComponent<CrosshairCellHover>().Init(cellImg, i == _selectedIndex);
        }
    }

    private void SelectCrosshair(int index)
    {
        _selectedIndex = index;
        CrosshairSettings.CrosshairIndex = index;

        for (int i = 0; i < _grid.childCount; i++)
        {
            var child = _grid.GetChild(i);
            var img = child.GetComponent<Image>();
            var hover = child.GetComponent<CrosshairCellHover>();
            bool selected = i == _selectedIndex;
            if (hover != null) hover.SetSelected(selected);
            if (img != null && (hover == null || !hover.IsHovered))
                img.color = selected ? new Color(1f, 1f, 1f, 0.14f) : new Color(1f, 1f, 1f, 0.04f);
        }

        UpdatePreview();
        ApplyLive();
    }

    private void OnColor(Slider r, Slider g, Slider b)
    {
        _selectedColor = new Color(r.value, g.value, b.value, 1f);
        CrosshairSettings.CrosshairColor = _selectedColor;
        UpdatePreview();
        ApplyLive();
    }

    private void UpdatePreview()
    {
        if (_preview == null || _sprites == null || _sprites.Length == 0) return;
        int idx = Mathf.Clamp(_selectedIndex, 0, _sprites.Length - 1);
        _preview.sprite = _sprites[idx];
        _preview.color = _selectedColor;
        float size = Mathf.Clamp(CrosshairSettings.CrosshairSize, 24f, 120f);
        _preview.rectTransform.sizeDelta = new Vector2(size, size);
    }

    private void ApplyLive()
    {
        if (_applier == null) _applier = FindAnyObjectByType<CrosshairApplier>(FindObjectsInactive.Include);
        if (_applier != null) _applier.Apply();
    }



    private void BuildGameplayContent(Transform parent)
    {
        _gameplayContent = new GameObject("GameplayContent", typeof(RectTransform));
        _gameplayContent.transform.SetParent(parent, false);
        Stretch(_gameplayContent.GetComponent<RectTransform>());

        var title = MakeText("Title", _gameplayContent.transform, "GAMEPLAY", 32, FontStyles.Bold, TextAlignmentOptions.Left);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(-72, 50);
        tr.anchoredPosition = new Vector2(0, -16);
        title.color = Color.white;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(_gameplayContent.transform, false);
        var cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 0);
        cRT.anchorMax = new Vector2(1, 1);
        cRT.offsetMin = new Vector2(40, 40);
        cRT.offsetMax = new Vector2(-40, -76);

        float y = 0f;

        y = MakeSettingsSlider(content.transform, "SENSITIVITY", y, 0.1f, 10f,
            GameplaySettings.Sensitivity, out _sensSlider, out _sensValue);
        _sensSlider.onValueChanged.AddListener(v =>
        {
            float rounded = Mathf.Round(v * 100f) / 100f;
            GameplaySettings.Sensitivity = rounded;
            _sensValue.text = rounded.ToString("F2");
            if (_look != null) _look.SetSensitivity(rounded);
        });

        y -= 24f;

        y = MakeSettingsSlider(content.transform, "FIELD OF VIEW", y, 60f, 120f,
            GameplaySettings.Fov, out _fovSlider, out _fovValue);
        _fovSlider.wholeNumbers = true;
        _fovSlider.onValueChanged.AddListener(v =>
        {
            GameplaySettings.Fov = v;
            _fovValue.text = Mathf.RoundToInt(v).ToString() + "°";
            if (_look != null) _look.SetFov(v);
        });

        bool isAimTraining = SceneManager.GetActiveScene().name != "Prefire";
        if (isAimTraining)
        {
            y -= 24f;
            MakeSettingsToggle(content.transform, "SHOW WEAPON VIEWMODEL", y,
                GameplaySettings.ShowViewmodel, out _viewmodelToggle);
            _viewmodelToggle.onValueChanged.AddListener(v =>
            {
                GameplaySettings.ShowViewmodel = v;


                if (_weapon is PlayerWeapon pw)
                    pw.SetViewmodelVisible(v);
            });
        }

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
        float min, float max, float initial,
        out Slider slider, out TMP_Text valueText)
    {
        var labelGO = new GameObject(label + "_Label", typeof(RectTransform));
        labelGO.transform.SetParent(parent, false);
        var lbl = labelGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = 15;
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
        valueText.fontSize = 15;
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
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fRT = fill.GetComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 0); fRT.anchorMax = new Vector2(0, 1);
        fRT.sizeDelta = new Vector2(10, 0);
        fill.GetComponent<Image>().color = Cherry;

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
        lbl.fontSize = 15;
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



    private struct ModeEntry { public string label; public string modeName; }

    private static readonly ModeEntry[] _aimModes = new[]
    {
        new ModeEntry { label = "GRIDSHOT", modeName = "Gridshot" },
        new ModeEntry { label = "CLASSIC", modeName = "Classic" },
        new ModeEntry { label = "SPIDERSHOT", modeName = "Spidershot" },
        new ModeEntry { label = "MICROSHOT", modeName = "Microshot" },
        new ModeEntry { label = "SIXSHOT", modeName = "Sixshot" },
        new ModeEntry { label = "STRAFETRACK", modeName = "Strafetrack" },
    };

    private readonly List<(string modeName, Image image)> _modeButtons = new();

    private void BuildModeView(Transform parent)
    {
        _modeView = MakeStretch("ModeView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_modeView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(520, 560);
        panel.GetComponent<Image>().color = PanelBG;

        var title = MakeText("Title", panel.transform, "SELECT MODE", 36, FontStyles.Bold, TextAlignmentOptions.Left);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(-60, 50);
        tr.anchoredPosition = new Vector2(0, -26);
        title.color = Color.white;

        var back = MakeButton("BackBtn", panel.transform, "BACK", new Color(0.2f, 0.2f, 0.22f, 0.8f));
        var backR = back.GetComponent<RectTransform>();
        backR.anchorMin = backR.anchorMax = new Vector2(1, 1);
        backR.pivot = new Vector2(1, 1);
        backR.sizeDelta = new Vector2(110, 40);
        backR.anchoredPosition = new Vector2(-24, -22);
        back.onClick.AddListener(ShowRoot);

        var col = new GameObject("Modes", typeof(RectTransform));
        col.transform.SetParent(panel.transform, false);
        var cr = col.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f); cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.pivot = new Vector2(0.5f, 0.5f); cr.sizeDelta = new Vector2(440, 440);
        cr.anchoredPosition = new Vector2(0, -20);
        var vlg = col.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.UpperCenter;

        _modeButtons.Clear();
        string scene = SceneManager.GetActiveScene().name;

        foreach (var mode in _aimModes)
        {
            var bg = IsCurrentMode(mode.modeName) ? Cherry : ButtonBG;
            var btn = MakeButton(mode.modeName, col.transform, mode.label, bg);
            var img = btn.GetComponent<Image>();
            _modeButtons.Add((mode.modeName, img));

            string mn = mode.modeName;
            btn.onClick.AddListener(() => StartMode(scene, mn));
        }
    }

    private void UpdateModeButtonStates()
    {
        foreach (var (modeName, image) in _modeButtons)
        {
            if (image != null)
                image.color = IsCurrentMode(modeName) ? Cherry : ButtonBG;
        }
    }

    private static bool IsCurrentMode(string modeName)
    {
        ModeManager manager = ModeManager.Current;
        if (manager != null && manager.ActiveMode != null)
            return string.Equals(manager.ActiveMode.ModeName, modeName, System.StringComparison.OrdinalIgnoreCase);

        return AimModeSelection.IsSelected(modeName);
    }



    private static readonly Color RouteCherryAccent = new Color(0.45f, 0.04f, 0.16f);
    private static readonly Color RouteCardBG = new Color(0.12f, 0.12f, 0.14f, 1f);
    private static readonly Color RouteCardActive = new Color(0.50f, 0.06f, 0.20f, 1f);
    private Transform _routeGrid;

    private void BuildRouteView(Transform parent)
    {
        _routeView = MakeStretch("RouteView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_routeView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.15f, 0.15f);
        pr.anchorMax = new Vector2(0.85f, 0.85f);
        pr.offsetMin = Vector2.zero;
        pr.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 0.98f);

        var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(panel.transform, false);
        var ar = accent.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0, 1); ar.anchorMax = new Vector2(1, 1);
        ar.pivot = new Vector2(0.5f, 1); ar.sizeDelta = new Vector2(0, 3);
        accent.GetComponent<Image>().color = RouteCherryAccent;

        var title = MakeText("Title", panel.transform, "SELECT ROUTE", 32, FontStyles.Bold, TextAlignmentOptions.Left);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(-60, 50);
        tr.anchoredPosition = new Vector2(0, -18);
        title.color = Color.white;

        var back = MakeButton("BackBtn", panel.transform, "BACK", new Color(0.18f, 0.18f, 0.20f, 0.9f));
        var backR = back.GetComponent<RectTransform>();
        backR.anchorMin = backR.anchorMax = new Vector2(1, 1);
        backR.pivot = new Vector2(1, 1);
        backR.sizeDelta = new Vector2(100, 36);
        backR.anchoredPosition = new Vector2(-20, -18);
        back.onClick.AddListener(ShowRoot);

        var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGO.transform.SetParent(panel.transform, false);
        var sr = scrollGO.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0, 0); sr.anchorMax = new Vector2(1, 1);
        sr.offsetMin = new Vector2(24, 20); sr.offsetMax = new Vector2(-24, -76);

        var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportGO.transform.SetParent(scrollGO.transform, false);
        Stretch(viewportGO.GetComponent<RectTransform>());

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewportGO.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(0, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0, contentRect.offsetMax.y);
        contentRect.anchoredPosition = Vector2.zero;

        var glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(560, 340);
        glg.spacing = new Vector2(24, 24);
        glg.padding = new RectOffset(16, 16, 12, 12);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;
        glg.childAlignment = TextAnchor.UpperLeft;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = scrollGO.GetComponent<ScrollRect>();
        scroll.viewport = viewportGO.GetComponent<RectTransform>();
        scroll.content = contentRect;
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32f;

        _routeGrid = content.transform;
    }

    private void PopulateRouteGrid()
    {
        if (_routeGrid == null) return;

        foreach (Transform child in _routeGrid)
            Destroy(child.gameObject);

        var pm = PrefireManager.Instance;
        if (pm == null || pm.Routes == null) return;

        for (int i = 0; i < pm.Routes.Length; i++)
        {
            var route = pm.Routes[i];
            bool isCurrent = i == pm.CurrentRouteIndex;
            MakeRouteCard(_routeGrid, route, i, isCurrent);
        }

        MakeComingSoonCard(_routeGrid);
    }

    private void MakeComingSoonCard(Transform parent)
    {
        var card = new GameObject("Route_ComingSoon", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.09f, 0.09f, 0.11f, 0.7f);

        var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(card.transform, false);
        var iRT = inner.GetComponent<RectTransform>();
        iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
        iRT.offsetMin = new Vector2(3, 3); iRT.offsetMax = new Vector2(-3, -3);
        inner.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.12f, 0.5f);

        var plus = MakeText("Plus", inner.transform, "+", 48, FontStyles.Normal, TextAlignmentOptions.Center);
        plus.color = new Color(0.45f, 0.04f, 0.16f, 0.5f);
        var pRT = plus.rectTransform;
        pRT.anchorMin = new Vector2(0, 0.35f); pRT.anchorMax = new Vector2(1, 0.75f);
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

        var label = MakeText("Label", inner.transform, "COMING SOON", 14, FontStyles.Bold, TextAlignmentOptions.Center);
        label.color = new Color(1f, 1f, 1f, 0.2f);
        var lRT = label.rectTransform;
        lRT.anchorMin = new Vector2(0, 0.15f); lRT.anchorMax = new Vector2(1, 0.35f);
        lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;
    }

    private void MakeRouteCard(Transform parent, PrefireRoute route, int index, bool active)
    {
        var card = new GameObject("Route_" + index, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        cardImg.color = active ? RouteCardActive : RouteCardBG;

        var thumbArea = new GameObject("ThumbArea", typeof(RectTransform), typeof(Image));
        thumbArea.transform.SetParent(card.transform, false);
        var taRT = thumbArea.GetComponent<RectTransform>();
        taRT.anchorMin = new Vector2(0, 0.30f); taRT.anchorMax = new Vector2(1, 1);
        taRT.offsetMin = new Vector2(4, 0); taRT.offsetMax = new Vector2(-4, -4);
        thumbArea.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.08f, 1f);

        if (route.Thumbnail != null)
        {
            var thumbGO = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
            thumbGO.transform.SetParent(thumbArea.transform, false);
            Stretch(thumbGO.GetComponent<RectTransform>());
            var thumbImg = thumbGO.GetComponent<Image>();
            thumbImg.sprite = route.Thumbnail;
            thumbImg.preserveAspect = true;
            thumbImg.raycastTarget = false;
        }
        else
        {
            var ph = MakeText("Placeholder", thumbArea.transform, "NO PREVIEW", 12, FontStyles.Normal, TextAlignmentOptions.Center);
            ph.color = new Color(1f, 1f, 1f, 0.2f);
            var pRT = ph.rectTransform;
            pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
        }

        var labelArea = new GameObject("LabelArea", typeof(RectTransform));
        labelArea.transform.SetParent(card.transform, false);
        var laRT = labelArea.GetComponent<RectTransform>();
        laRT.anchorMin = new Vector2(0, 0); laRT.anchorMax = new Vector2(1, 0.30f);
        laRT.offsetMin = Vector2.zero; laRT.offsetMax = Vector2.zero;

        var nameText = MakeText("Name", labelArea.transform, route.RouteName.ToUpper(), 13, FontStyles.Bold, TextAlignmentOptions.Left);
        nameText.color = active ? Color.white : new Color(0.85f, 0.85f, 0.85f);
        var nRT = nameText.rectTransform;
        nRT.anchorMin = new Vector2(0, 0.35f); nRT.anchorMax = new Vector2(1, 1);
        nRT.offsetMin = new Vector2(6, 0); nRT.offsetMax = new Vector2(-6, 0);

        var countText = MakeText("Count", labelArea.transform, route.BotCount + " BOTS", 11, FontStyles.Normal, TextAlignmentOptions.Left);
        countText.color = new Color(0.6f, 0.6f, 0.65f);
        var cRT = countText.rectTransform;
        cRT.anchorMin = new Vector2(0, 0); cRT.anchorMax = new Vector2(1, 0.45f);
        cRT.offsetMin = new Vector2(6, 0); cRT.offsetMax = new Vector2(-6, 0);

        if (active)
        {
            var indicator = new GameObject("ActiveBar", typeof(RectTransform), typeof(Image));
            indicator.transform.SetParent(card.transform, false);
            var indRT = indicator.GetComponent<RectTransform>();
            indRT.anchorMin = new Vector2(0, 0); indRT.anchorMax = new Vector2(0, 1);
            indRT.pivot = new Vector2(0, 0.5f); indRT.sizeDelta = new Vector2(3, 0);
            indicator.GetComponent<Image>().color = Cherry;
        }

        var btn = card.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.targetGraphic = cardImg;
        int routeIndex = index;
        btn.onClick.AddListener(() => SelectRoute(routeIndex));

        card.AddComponent<RouteCardHover>().Init(cardImg, active);
    }

    private void SelectRoute(int index)
    {
        var pm = PrefireManager.Instance;
        if (pm == null) return;
        pm.SwitchToRoute(index);
        Resume();
    }



    private static GameObject MakeStretch(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    private static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, FontStyles style, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    private static Button MakeButton(string name, Transform parent, string label, Color bg)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = bg;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 52;
        le.preferredWidth = 340;

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        go.AddComponent<MenuButtonEffect>().hoverVolume = 0.35f;

        var lbl = MakeText("Label", go.transform, label, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        var lr = lbl.rectTransform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        lbl.color = Color.white;

        return btn;
    }

    private Slider MakeSlider(string label, Transform parent, float y, Color fillColor)
    {
        var row = new GameObject(label + "Row", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var rr = row.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0, 1); rr.anchorMax = new Vector2(1, 1);
        rr.pivot = new Vector2(0.5f, 1);
        rr.sizeDelta = new Vector2(0, 50);
        rr.anchoredPosition = new Vector2(0, y);

        var lbl = MakeText("Label", row.transform, label.ToUpper(), 15, FontStyles.Bold, TextAlignmentOptions.Left);
        lbl.color = new Color(1f, 1f, 1f, 0.75f);
        var lr = lbl.rectTransform;
        lr.anchorMin = new Vector2(0, 1); lr.anchorMax = new Vector2(1, 1);
        lr.pivot = new Vector2(0, 1); lr.sizeDelta = new Vector2(0, 20);
        lr.anchoredPosition = new Vector2(4, 0);

        var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(row.transform, false);
        var slr = sliderGO.GetComponent<RectTransform>();
        slr.anchorMin = new Vector2(0, 0); slr.anchorMax = new Vector2(1, 0);
        slr.pivot = new Vector2(0.5f, 0); slr.sizeDelta = new Vector2(0, 16);
        slr.anchoredPosition = new Vector2(0, 2);

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        Stretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0, 0); fr.anchorMax = new Vector2(0, 1);
        fr.sizeDelta = new Vector2(10, 0);
        fill.GetComponent<Image>().color = fillColor;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGO.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var hr = handle.GetComponent<RectTransform>();
        hr.sizeDelta = new Vector2(14, 22);
        handle.GetComponent<Image>().color = Color.white;

        var slider = sliderGO.GetComponent<Slider>();
        slider.fillRect = fr;
        slider.handleRect = hr;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        return slider;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
