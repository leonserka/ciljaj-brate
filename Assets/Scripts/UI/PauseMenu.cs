using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Self-contained in-game pause menu. Press ESC to open. Builds its own overlay
/// canvas in code so it can be dropped into any gameplay scene (AimTraining, Prefire)
/// with no manual wiring. Offers Resume, Change Mode, Crosshair settings, and Main Menu.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "MainMenu";

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f); // dark cherry, matches main menu
    private static readonly Color PanelBG = new Color(0.10f, 0.10f, 0.12f, 0.98f);
    private static readonly Color ButtonBG = new Color(0.16f, 0.16f, 0.19f, 1f);
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.78f);

    private GameObject _root;        // dim background + everything
    private GameObject _rootView;    // main pause buttons
    private GameObject _crosshairView;
    private GameObject _modeView;
    private bool _open;

    // captured component states so we can restore them on resume
    private PlayerLook _look;
    private PlayerWeapon _weapon;
    private PlayerController _controller;
    private bool _weaponWas, _controllerWas;

    // crosshair settings widgets
    private Sprite[] _sprites;
    private int _selectedIndex;
    private Color _selectedColor;
    private Image _preview;
    private Transform _grid;
    private CrosshairApplier _applier;

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
            if (_open && (_crosshairView.activeSelf || _modeView.activeSelf))
                ShowRoot();          // ESC backs out of a sub-panel first
            else
                Toggle();
        }
    }

    // ---------------------------------------------------------------- open/close

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
        _weapon = FindAnyObjectByType<PlayerWeapon>();
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

    // ---------------------------------------------------------------- views

    private void ShowRoot()
    {
        _rootView.SetActive(true);
        _crosshairView.SetActive(false);
        _modeView.SetActive(false);
    }

    private void ShowCrosshair()
    {
        _rootView.SetActive(false);
        _crosshairView.SetActive(true);
        _modeView.SetActive(false);
        SyncCrosshairWidgets();
    }

    private void ShowModes()
    {
        _rootView.SetActive(false);
        _crosshairView.SetActive(false);
        _modeView.SetActive(true);
    }

    private void StartMode(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ---------------------------------------------------------------- UI build

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
        // Overlay canvas, drawn on top of HUD/crosshair
        var canvasGO = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Dim background (full-screen, blocks clicks to game)
        _root = MakeStretch("Root", canvasGO.transform);
        var dim = _root.AddComponent<Image>();
        dim.color = Dim;

        BuildRootView(_root.transform);
        BuildCrosshairView(_root.transform);
        BuildModeView(_root.transform);
    }

    private void BuildModeView(Transform parent)
    {
        _modeView = MakeStretch("ModeView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_modeView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(520, 560);
        panel.GetComponent<Image>().color = PanelBG;
        AddOutline(panel);

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

        // Mode list
        var col = new GameObject("Modes", typeof(RectTransform));
        col.transform.SetParent(panel.transform, false);
        var cr = col.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f); cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.pivot = new Vector2(0.5f, 0.5f); cr.sizeDelta = new Vector2(440, 380);
        cr.anchoredPosition = new Vector2(0, -30);
        var vlg = col.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // Only Combo Shredder is implemented; it restarts the current arena scene.
        string scene = SceneManager.GetActiveScene().name;
        MakeButton("ComboBtn", col.transform, "COMBO SHREDDER", Cherry).onClick.AddListener(() => StartMode(scene));
        MakeModeSoon("Gridshot", col.transform, "GRIDSHOT");
        MakeModeSoon("Spidershot", col.transform, "SPIDERSHOT");
        MakeModeSoon("Flicking", col.transform, "FLICKING");
    }

    private static void MakeModeSoon(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.13f, 0.13f, 0.15f, 0.7f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 52;
        le.preferredWidth = 440;

        var lbl = MakeText("Label", go.transform, label, 18, FontStyles.Bold, TextAlignmentOptions.Center);
        var lr = lbl.rectTransform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        lbl.color = new Color(1f, 1f, 1f, 0.35f);

        var soon = MakeText("Soon", go.transform, "COMING SOON", 11, FontStyles.Bold, TextAlignmentOptions.Right);
        var sr = soon.rectTransform;
        sr.anchorMin = new Vector2(0, 0); sr.anchorMax = new Vector2(1, 1);
        sr.offsetMin = new Vector2(0, 0); sr.offsetMax = new Vector2(-16, 0);
        soon.color = new Color(0.55f, 0.05f, 0.20f, 0.8f);
    }

    private void BuildRootView(Transform parent)
    {
        _rootView = MakeStretch("RootView", parent);

        // Center panel
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_rootView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(440, 520);
        panel.GetComponent<Image>().color = PanelBG;
        AddOutline(panel);

        // Title
        var title = MakeText("Title", panel.transform, "PAUSED", 44, FontStyles.Bold, TextAlignmentOptions.Center);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(0, 60);
        tr.anchoredPosition = new Vector2(0, -36);
        title.color = Color.white;

        // Cherry accent bar under title
        var bar = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panel.transform, false);
        var br = bar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 1); br.anchorMax = new Vector2(0.5f, 1);
        br.pivot = new Vector2(0.5f, 1); br.sizeDelta = new Vector2(60, 3);
        br.anchoredPosition = new Vector2(0, -96);
        bar.GetComponent<Image>().color = Cherry;

        // Button column
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
        MakeButton("CrosshairBtn", col.transform, "CROSSHAIR", ButtonBG).onClick.AddListener(ShowCrosshair);

        // Prefire uses routes instead of modes; switching isn't wired up yet, so grey it out.
        if (SceneManager.GetActiveScene().name == "Prefire")
            MakeDisabledButton("SwitchRouteBtn", col.transform, "SWITCH ROUTE");
        else
            MakeButton("ChangeModeBtn", col.transform, "CHANGE MODE", ButtonBG).onClick.AddListener(ShowModes);

        MakeButton("MenuBtn", col.transform, "MAIN MENU", ButtonBG).onClick.AddListener(GoToMainMenu);
    }

    private void BuildCrosshairView(Transform parent)
    {
        _crosshairView = MakeStretch("CrosshairView", parent);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_crosshairView.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1120, 700);
        panel.GetComponent<Image>().color = PanelBG;
        AddOutline(panel);

        var title = MakeText("Title", panel.transform, "CROSSHAIR", 36, FontStyles.Bold, TextAlignmentOptions.Left);
        var tr = title.rectTransform;
        tr.anchorMin = new Vector2(0, 1); tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1); tr.sizeDelta = new Vector2(-72, 50);
        tr.anchoredPosition = new Vector2(0, -26);
        title.color = Color.white;

        // Back button (top-right, subtle)
        var back = MakeButton("BackBtn", panel.transform, "BACK", new Color(0.2f, 0.2f, 0.22f, 0.8f));
        var backR = back.GetComponent<RectTransform>();
        backR.anchorMin = backR.anchorMax = new Vector2(1, 1);
        backR.pivot = new Vector2(1, 1);
        backR.sizeDelta = new Vector2(110, 40);
        backR.anchoredPosition = new Vector2(-24, -22);
        back.onClick.AddListener(ShowRoot);

        // Scrollable crosshair grid (left side)
        BuildCrosshairGrid(panel.transform);

        // Sliders + preview (right side)
        BuildCrosshairControls(panel.transform);
    }

    private void BuildCrosshairGrid(Transform panel)
    {
        // Scroll root, center-anchored on the left half of the panel
        var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGO.transform.SetParent(panel, false);
        var sr = scrollGO.GetComponent<RectTransform>();
        sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0.5f, 0.5f);
        sr.sizeDelta = new Vector2(680, 520);
        sr.anchoredPosition = new Vector2(-200, -30);
        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);

        // Viewport (this is what clips), fills the scroll root
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
        glg.constraintCount = 6;
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

    private void BuildCrosshairControls(Transform panel)
    {
        var side = new GameObject("Controls", typeof(RectTransform));
        side.transform.SetParent(panel, false);
        var sd = side.GetComponent<RectTransform>();
        sd.anchorMin = sd.anchorMax = new Vector2(0.5f, 0.5f);
        sd.pivot = new Vector2(0.5f, 0.5f);
        sd.sizeDelta = new Vector2(360, 520);
        sd.anchoredPosition = new Vector2(340, -30);

        // Preview box
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

        // Sliders
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

    private Slider _redSlider, _greenSlider, _blueSlider, _sizeSlider;

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

    // ---------------------------------------------------------------- helpers

    private static GameObject MakeStretch(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    private static void AddOutline(GameObject go)
    {
        var o = go.AddComponent<Outline>();
        o.effectColor = new Color(1f, 1f, 1f, 0.06f);
        o.effectDistance = new Vector2(1, 1);
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

    private static void MakeDisabledButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.13f, 0.13f, 0.15f, 0.7f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 52;
        le.preferredWidth = 340;

        var lbl = MakeText("Label", go.transform, label, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        var lr = lbl.rectTransform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        lbl.color = new Color(1f, 1f, 1f, 0.30f);
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
