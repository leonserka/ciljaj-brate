using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefireHUD : MonoBehaviour
{
    private Canvas _canvas;
    private TMP_Text _healthText;
    private Image _healthBarFill;
    private Image _healthBarBg;
    private TMP_Text _routeText;
    private TMP_Text _timerText;
    private TMP_Text _botsText;
    private CanvasGroup _damageFlash;

    private PlayerHealth _playerHealth;
    private PrefireManager _prefireManager;
    private float _displayHealth;
    private float _flashAlpha;

    private static readonly Color HealthGreen = new Color(0.35f, 0.85f, 0.35f);
    private static readonly Color HealthYellow = new Color(0.95f, 0.85f, 0.2f);
    private static readonly Color HealthRed = new Color(0.9f, 0.2f, 0.15f);
    private static readonly Color PanelBg = new Color(0.05f, 0.05f, 0.08f, 0.75f);
    private static readonly Color BarBg = new Color(0.15f, 0.15f, 0.18f, 0.9f);

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        _playerHealth = PlayerHealth.Instance;
        if (_playerHealth == null)
            _playerHealth = FindAnyObjectByType<PlayerHealth>();

        _prefireManager = PrefireManager.Instance;
        if (_prefireManager == null)
            _prefireManager = FindAnyObjectByType<PrefireManager>();

        if (_playerHealth != null)
        {
            _playerHealth.HealthChanged += OnHealthChanged;
            _displayHealth = _playerHealth.CurrentHealth;
            UpdateHealthDisplay(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }

        if (_prefireManager != null)
        {
            _prefireManager.RouteStarted += OnRouteStarted;
            _prefireManager.RouteCleared += OnRouteCleared;
            UpdateRouteDisplay();
        }
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.HealthChanged -= OnHealthChanged;
        if (_prefireManager != null)
        {
            _prefireManager.RouteStarted -= OnRouteStarted;
            _prefireManager.RouteCleared -= OnRouteCleared;
        }
    }

    private void Update()
    {
        if (_playerHealth != null)
        {
            _displayHealth = Mathf.MoveTowards(_displayHealth, _playerHealth.CurrentHealth, 120f * Time.deltaTime);
            float t = _displayHealth / _playerHealth.MaxHealth;
            _healthBarFill.fillAmount = t;
            _healthBarFill.color = GetHealthColor(t);
        }

        if (_flashAlpha > 0f)
        {
            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, 2f * Time.deltaTime);
            _damageFlash.alpha = _flashAlpha;
        }

        if (_prefireManager != null)
        {
            UpdateTimerDisplay();
            UpdateBotsDisplay();
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current < _displayHealth)
            _flashAlpha = 0.35f;

        UpdateHealthDisplay(current, max);
    }

    private void UpdateHealthDisplay(int current, int max)
    {
        _healthText.text = current.ToString();
        float t = (float)current / max;
        _healthText.color = GetHealthColor(t);
    }

    private void OnRouteStarted(int index, int total)
    {
        UpdateRouteDisplay();
    }

    private void OnRouteCleared(int index, float time)
    {
        UpdateRouteDisplay();
    }

    private void UpdateRouteDisplay()
    {
        if (_prefireManager == null) return;
        string name = _prefireManager.CurrentRouteName;
        int idx = _prefireManager.CurrentRouteIndex + 1;
        int total = _prefireManager.TotalRoutes;
        _routeText.text = name + "  " + idx + "/" + total;
    }

    private void UpdateTimerDisplay()
    {
        float t = _prefireManager.RouteTime;
        int sec = Mathf.FloorToInt(t);
        int ms = Mathf.FloorToInt((t - sec) * 100f);
        _timerText.text = sec.ToString("00") + "." + ms.ToString("00");
    }

    private void UpdateBotsDisplay()
    {
        int remaining = _prefireManager.BotsRemaining;
        _botsText.text = remaining.ToString();
    }

    private Color GetHealthColor(float t)
    {
        if (t > 0.5f) return Color.Lerp(HealthYellow, HealthGreen, (t - 0.5f) * 2f);
        return Color.Lerp(HealthRed, HealthYellow, t * 2f);
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("PrefireHUD_Canvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Damage flash overlay ---
        var flashGO = CreatePanel(canvasGO.transform, "DamageFlash", Color.red);
        var flashRT = flashGO.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.sizeDelta = Vector2.zero;
        _damageFlash = flashGO.AddComponent<CanvasGroup>();
        _damageFlash.alpha = 0f;
        _damageFlash.blocksRaycasts = false;
        _damageFlash.interactable = false;
        flashGO.GetComponent<Image>().color = new Color(0.8f, 0f, 0f, 0.25f);

        // --- Bottom bar container ---
        var bottomBar = new GameObject("BottomBar", typeof(RectTransform));
        bottomBar.transform.SetParent(canvasGO.transform, false);
        var bbRT = bottomBar.GetComponent<RectTransform>();
        bbRT.anchorMin = new Vector2(0f, 0f);
        bbRT.anchorMax = new Vector2(1f, 0f);
        bbRT.pivot = new Vector2(0.5f, 0f);
        bbRT.anchoredPosition = new Vector2(0f, 12f);
        bbRT.sizeDelta = new Vector2(0f, 60f);

        // --- Health panel (left-center) ---
        var healthPanel = CreatePanel(bbRT, "HealthPanel", PanelBg);
        var hpRT = healthPanel.GetComponent<RectTransform>();
        hpRT.anchorMin = new Vector2(0.5f, 0.5f);
        hpRT.anchorMax = new Vector2(0.5f, 0.5f);
        hpRT.pivot = new Vector2(1f, 0.5f);
        hpRT.anchoredPosition = new Vector2(-60f, 0f);
        hpRT.sizeDelta = new Vector2(200f, 50f);

        // Health bar background
        var barBgGO = CreatePanel(healthPanel.transform, "HealthBarBg", BarBg);
        var barBgRT = barBgGO.GetComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0f, 0f);
        barBgRT.anchorMax = new Vector2(0.35f, 1f);
        barBgRT.offsetMin = new Vector2(8f, 10f);
        barBgRT.offsetMax = new Vector2(0f, -10f);
        _healthBarBg = barBgGO.GetComponent<Image>();

        // Health bar fill
        var barFillGO = CreatePanel(barBgGO.transform, "HealthBarFill", HealthGreen);
        var barFillRT = barFillGO.GetComponent<RectTransform>();
        barFillRT.anchorMin = Vector2.zero;
        barFillRT.anchorMax = Vector2.one;
        barFillRT.offsetMin = new Vector2(2f, 2f);
        barFillRT.offsetMax = new Vector2(-2f, -2f);
        _healthBarFill = barFillGO.GetComponent<Image>();
        _healthBarFill.type = Image.Type.Filled;
        _healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        _healthBarFill.fillOrigin = 0;
        _healthBarFill.fillAmount = 1f;

        // Health + icon
        var plusGO = CreateText(healthPanel.transform, "HealthIcon", "+", 22, FontStyles.Bold, HealthGreen);
        var plusRT = plusGO.GetComponent<RectTransform>();
        plusRT.anchorMin = new Vector2(0.35f, 0f);
        plusRT.anchorMax = new Vector2(0.5f, 1f);
        plusRT.offsetMin = Vector2.zero;
        plusRT.offsetMax = Vector2.zero;

        // Health number
        var healthNumGO = CreateText(healthPanel.transform, "HealthNumber", "100", 32, FontStyles.Bold, Color.white);
        var healthNumRT = healthNumGO.GetComponent<RectTransform>();
        healthNumRT.anchorMin = new Vector2(0.5f, 0f);
        healthNumRT.anchorMax = new Vector2(1f, 1f);
        healthNumRT.offsetMin = Vector2.zero;
        healthNumRT.offsetMax = new Vector2(-8f, 0f);
        _healthText = healthNumGO.GetComponent<TMP_Text>();
        _healthText.alignment = TextAlignmentOptions.Left;

        // --- Bots panel (right-center) ---
        var botsPanel = CreatePanel(bbRT, "BotsPanel", PanelBg);
        var bpRT = botsPanel.GetComponent<RectTransform>();
        bpRT.anchorMin = new Vector2(0.5f, 0.5f);
        bpRT.anchorMax = new Vector2(0.5f, 0.5f);
        bpRT.pivot = new Vector2(0f, 0.5f);
        bpRT.anchoredPosition = new Vector2(60f, 0f);
        bpRT.sizeDelta = new Vector2(160f, 50f);

        // Bots icon
        var targetIconGO = CreateText(botsPanel.transform, "BotsIcon", "◎", 20, FontStyles.Normal, new Color(0.9f, 0.6f, 0.2f));
        var tiRT = targetIconGO.GetComponent<RectTransform>();
        tiRT.anchorMin = new Vector2(0f, 0f);
        tiRT.anchorMax = new Vector2(0.3f, 1f);
        tiRT.offsetMin = new Vector2(8f, 0f);
        tiRT.offsetMax = Vector2.zero;

        // Bots remaining number
        var botsNumGO = CreateText(botsPanel.transform, "BotsNumber", "4", 32, FontStyles.Bold, Color.white);
        var bnRT = botsNumGO.GetComponent<RectTransform>();
        bnRT.anchorMin = new Vector2(0.3f, 0f);
        bnRT.anchorMax = new Vector2(0.65f, 1f);
        bnRT.offsetMin = Vector2.zero;
        bnRT.offsetMax = Vector2.zero;
        _botsText = botsNumGO.GetComponent<TMP_Text>();
        _botsText.alignment = TextAlignmentOptions.Left;

        // "ALIVE" label
        var aliveGO = CreateText(botsPanel.transform, "AliveLabel", "ALIVE", 11, FontStyles.Normal, new Color(0.6f, 0.6f, 0.65f));
        var alRT = aliveGO.GetComponent<RectTransform>();
        alRT.anchorMin = new Vector2(0.6f, 0f);
        alRT.anchorMax = new Vector2(1f, 1f);
        alRT.offsetMin = Vector2.zero;
        alRT.offsetMax = new Vector2(-8f, 0f);

        // --- Top info bar ---
        var topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(canvasGO.transform, false);
        var tbRT = topBar.GetComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0.5f, 1f);
        tbRT.anchorMax = new Vector2(0.5f, 1f);
        tbRT.pivot = new Vector2(0.5f, 1f);
        tbRT.anchoredPosition = new Vector2(0f, -10f);
        tbRT.sizeDelta = new Vector2(300f, 40f);

        var topPanel = CreatePanel(tbRT, "TopPanel", PanelBg);
        var tpRT = topPanel.GetComponent<RectTransform>();
        tpRT.anchorMin = Vector2.zero;
        tpRT.anchorMax = Vector2.one;
        tpRT.offsetMin = Vector2.zero;
        tpRT.offsetMax = Vector2.zero;

        // Route name
        var routeGO = CreateText(topPanel.transform, "RouteText", "Route 1/1", 16, FontStyles.Bold, Color.white);
        var rtRT = routeGO.GetComponent<RectTransform>();
        rtRT.anchorMin = new Vector2(0f, 0f);
        rtRT.anchorMax = new Vector2(0.6f, 1f);
        rtRT.offsetMin = new Vector2(12f, 0f);
        rtRT.offsetMax = Vector2.zero;
        _routeText = routeGO.GetComponent<TMP_Text>();
        _routeText.alignment = TextAlignmentOptions.Left;

        // Timer
        var timerGO = CreateText(topPanel.transform, "TimerText", "00.00", 18, FontStyles.Normal, new Color(0.9f, 0.9f, 0.5f));
        var tmRT = timerGO.GetComponent<RectTransform>();
        tmRT.anchorMin = new Vector2(0.6f, 0f);
        tmRT.anchorMax = new Vector2(1f, 1f);
        tmRT.offsetMin = Vector2.zero;
        tmRT.offsetMax = new Vector2(-12f, 0f);
        _timerText = timerGO.GetComponent<TMP_Text>();
        _timerText.alignment = TextAlignmentOptions.Right;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return go;
    }
}
