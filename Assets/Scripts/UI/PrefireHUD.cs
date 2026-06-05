using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrefireHUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthBarFill;

    [Header("Damage Flash")]
    [SerializeField] private CanvasGroup damageFlash;

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f);
    private static readonly Color DarkCherry = new Color(0.27f, 0f, 0.125f);
    private static readonly Color PanelBG = new Color(0.07f, 0.07f, 0.09f, 0.97f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color StatLabel = new Color(0.55f, 0.55f, 0.58f);
    private static readonly Color StatValue = new Color(0.95f, 0.95f, 0.97f);

    private PlayerHealth _playerHealth;
    private PrefireManager _prefireManager;
    private float _displayHealth;
    private float _flashAlpha;


    private GameObject _cardRoot;
    private TMP_Text _cardTitle;
    private TMP_Text _cardTime;
    private TMP_Text _cardAccuracy;
    private TMP_Text _cardScore;
    private TMP_Text _cardContinueLabel;
    private PlayerLook _look;
    private PlayerController _playerController;


    private TMP_Text _hudScore;
    private TMP_Text _hudTimer;
    private TMP_Text _hudAccuracy;
    private float _displayScore;
    private float _displayAccuracy;
    private Vector3 _scoreBaseScale;
    private float _scorePunch;


    private TMP_Text _botsText;

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
        }

        HideOldHudElements();
        BuildTopHUD();
        BuildResultCard();
    }

    private void HideOldHudElements()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        foreach (var rt in canvas.GetComponentsInChildren<RectTransform>(true))
        {
            string n = rt.gameObject.name;
            if (n == "TopPanel" || n == "TopBar")
                rt.gameObject.SetActive(false);
            if (n == "BotsNumber")
                _botsText = rt.GetComponent<TMP_Text>();
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
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = t;
                healthBarFill.color = GetHealthColor(t);
            }
        }

        if (_flashAlpha > 0f)
        {
            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, 2f * Time.unscaledDeltaTime);
            if (damageFlash != null) damageFlash.alpha = _flashAlpha;
        }

        if (_prefireManager != null)
        {
            UpdateTopHUD();
            if (_botsText != null)
                _botsText.text = _prefireManager.BotsRemaining.ToString();
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
        if (healthText != null)
        {
            healthText.text = current.ToString();
            healthText.color = GetHealthColor((float)current / max);
        }
    }

    private void OnRouteStarted(int index, int total)
    {
        HideResultCard();
        _displayScore = 0f;
        _displayAccuracy = 0f;
    }

    private void OnRouteCleared(PrefireRouteResult result)
    {
        _scorePunch = 1f;
        ShowResultCard(result);
    }

    private Color GetHealthColor(float t)
    {
        return t > 0.3f ? Color.white : new Color(0.9f, 0.2f, 0.15f);
    }



    private void BuildTopHUD()
    {
        var canvasGO = new GameObject("TopHudCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;


        var topHud = new GameObject("TopHUD", typeof(RectTransform));
        topHud.transform.SetParent(canvasGO.transform, false);
        var thRT = topHud.GetComponent<RectTransform>();
        thRT.anchorMin = new Vector2(0.5f, 1f); thRT.anchorMax = new Vector2(0.5f, 1f);
        thRT.pivot = new Vector2(0.5f, 1f);
        thRT.sizeDelta = new Vector2(640, 62);
        thRT.anchoredPosition = new Vector2(0, -25);


        var scorePanel = MakePanel("ScorePanel", topHud.transform,
            new Vector2(0, 0), new Vector2(0.3f, 1), new Vector3(1, 1.2f, 1));

        MakeAccentLine(scorePanel.transform);
        MakePanelLabel("ScoreLabel", scorePanel.transform, "SCORE");
        _hudScore = MakePanelValue("ScoreText", scorePanel.transform, "0");
        _scoreBaseScale = _hudScore.transform.localScale;


        var timerPanel = MakePanel("TimerPanel", topHud.transform,
            new Vector2(0.32f, 0), new Vector2(0.68f, 1), new Vector3(1.2f, 1.56f, 1.2f));
        timerPanel.GetComponent<Image>().color = new Color(1, 1, 1, 0.92f);
        timerPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -2.3f);

        MakeAccentLine(timerPanel.transform);
        _hudTimer = MakePanelValue("TimerText", timerPanel.transform, "00.00");
        _hudTimer.fontSize = 34;
        var tmRT = _hudTimer.rectTransform;
        tmRT.anchorMin = Vector2.zero; tmRT.anchorMax = Vector2.one;
        tmRT.anchoredPosition = new Vector2(1, -1.2f);
        tmRT.sizeDelta = new Vector2(0, -2);


        var accPanel = MakePanel("AccuracyPanel", topHud.transform,
            new Vector2(0.7f, 0), new Vector2(1, 1), new Vector3(1, 1.2f, 1));

        MakeAccentLine(accPanel.transform);
        MakePanelLabel("AccLabel", accPanel.transform, "ACCURACY");
        _hudAccuracy = MakePanelValue("AccText", accPanel.transform, "0%");
    }

    private GameObject MakePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector3 scale)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
        rt.localScale = scale;
        var img = go.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.85f);
        img.raycastTarget = false;
        return go;
    }

    private void MakeAccentLine(Transform parent)
    {
        var go = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 2.5f);
        rt.localScale = new Vector3(0.9f, 1, 1);
        rt.anchoredPosition = new Vector2(9.9f, 2.7f);
        go.GetComponent<Image>().color = DarkCherry;
        go.GetComponent<Image>().raycastTarget = false;
    }

    private TMP_Text MakePanelLabel(string name, Transform parent, string text)
    {
        var lbl = MakeText(name, parent, text, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        lbl.color = new Color(0.27f, 0f, 0.125f, 0.95f);
        var rt = lbl.rectTransform;
        rt.anchorMin = new Vector2(0, 0.52f); rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(0, -3); rt.sizeDelta = new Vector2(0, -6);
        return lbl;
    }

    private TMP_Text MakePanelValue(string name, Transform parent, string text)
    {
        var val = MakeText(name, parent, text, 30, FontStyles.Bold, TextAlignmentOptions.Center);
        val.color = DarkCherry;
        var rt = val.rectTransform;
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0.55f);
        rt.anchoredPosition = new Vector2(0, 1); rt.sizeDelta = new Vector2(0, -2);
        return val;
    }

    private void UpdateTopHUD()
    {
        if (_prefireManager == null) return;


        if (_hudTimer != null)
        {
            float t = _prefireManager.RouteTime;
            int sec = Mathf.FloorToInt(t);
            int ms = Mathf.FloorToInt((t - sec) * 100f);
            _hudTimer.text = sec.ToString("00") + "." + ms.ToString("00");
        }


        float accuracy = _prefireManager.CurrentAccuracy;
        float accPct = accuracy * 100f;
        _displayAccuracy = Mathf.Lerp(_displayAccuracy, accPct, 12f * Time.deltaTime);
        if (_hudAccuracy != null)
            _hudAccuracy.text = Mathf.RoundToInt(_displayAccuracy) + "%";


        float speedFactor = Mathf.Max(0f, 1f - _prefireManager.RouteTime / 120f);
        int score = Mathf.RoundToInt(accuracy * 700f + speedFactor * 300f);
        _displayScore = Mathf.MoveTowards(_displayScore, score,
            12f * Time.deltaTime * Mathf.Max(1f, Mathf.Abs(score - _displayScore)));
        if (_hudScore != null)
        {
            _hudScore.text = Mathf.RoundToInt(_displayScore).ToString();


            _scorePunch = Mathf.MoveTowards(_scorePunch, 0f, 8f * Time.deltaTime);
            float s = 1f + _scorePunch * 0.4f;
            _hudScore.transform.localScale = _scoreBaseScale * s;
        }
    }



    private void BuildResultCard()
    {
        var canvasGO = new GameObject("ResultCardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;


        var dimGO = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        dimGO.transform.SetParent(canvasGO.transform, false);
        var dimRT = dimGO.GetComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero; dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = dimRT.offsetMax = Vector2.zero;
        dimGO.GetComponent<Image>().color = DimColor;

        _cardRoot = dimGO;


        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(dimGO.transform, false);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(380, 520);
        panel.GetComponent<Image>().color = PanelBG;


        var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(panel.transform, false);
        var br = bar.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0, 1); br.anchorMax = new Vector2(1, 1);
        br.pivot = new Vector2(0.5f, 1); br.sizeDelta = new Vector2(0, 3);
        bar.GetComponent<Image>().color = Cherry;


        _cardTitle = MakeText("Title", panel.transform, "ROUTE NAME", 28, FontStyles.Bold, TextAlignmentOptions.Left);
        var tRT = _cardTitle.rectTransform;
        tRT.anchorMin = new Vector2(0, 1); tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot = new Vector2(0, 1); tRT.sizeDelta = new Vector2(-40, 50);
        tRT.anchoredPosition = new Vector2(24, -24);
        _cardTitle.color = Color.white;


        var div = new GameObject("Div", typeof(RectTransform), typeof(Image));
        div.transform.SetParent(panel.transform, false);
        var dRT = div.GetComponent<RectTransform>();
        dRT.anchorMin = new Vector2(0, 1); dRT.anchorMax = new Vector2(1, 1);
        dRT.pivot = new Vector2(0.5f, 1); dRT.sizeDelta = new Vector2(-40, 1);
        dRT.anchoredPosition = new Vector2(0, -82);
        div.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);


        float y = -100f;
        _cardTime = MakeStatRow(panel.transform, "TIME", ref y);
        _cardAccuracy = MakeStatRow(panel.transform, "ACCURACY", ref y);
        _cardScore = MakeStatRow(panel.transform, "SCORE", ref y);


        var div2 = new GameObject("Div2", typeof(RectTransform), typeof(Image));
        div2.transform.SetParent(panel.transform, false);
        var d2RT = div2.GetComponent<RectTransform>();
        d2RT.anchorMin = new Vector2(0, 0); d2RT.anchorMax = new Vector2(1, 0);
        d2RT.pivot = new Vector2(0.5f, 0); d2RT.sizeDelta = new Vector2(-40, 1);
        d2RT.anchoredPosition = new Vector2(0, 76);
        div2.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);


        var btnGO = new GameObject("ContinueBtn", typeof(RectTransform), typeof(Image));
        btnGO.transform.SetParent(panel.transform, false);
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0, 0); bRT.anchorMax = new Vector2(1, 0);
        bRT.pivot = new Vector2(0.5f, 0); bRT.sizeDelta = new Vector2(-40, 56);
        bRT.anchoredPosition = new Vector2(0, 12);
        btnGO.GetComponent<Image>().color = Cherry;
        var btn = btnGO.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btnGO.AddComponent<MenuButtonEffect>().hoverVolume = 0.35f;
        btn.onClick.AddListener(OnContinueClicked);

        _cardContinueLabel = MakeText("Label", btnGO.transform, "CONTINUE →", 17, FontStyles.Bold, TextAlignmentOptions.Center);
        _cardContinueLabel.color = Color.white;
        var clRT = _cardContinueLabel.rectTransform;
        clRT.anchorMin = Vector2.zero; clRT.anchorMax = Vector2.one;
        clRT.offsetMin = clRT.offsetMax = Vector2.zero;

        _cardRoot.SetActive(false);
    }

    private TMP_Text MakeStatRow(Transform parent, string label, ref float y)
    {
        var lblGO = new GameObject(label + "_Lbl", typeof(RectTransform));
        lblGO.transform.SetParent(parent, false);
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = 15;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.color = StatLabel;
        lbl.raycastTarget = false;
        var lRT = lblGO.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0, 1); lRT.anchorMax = new Vector2(0.5f, 1);
        lRT.pivot = new Vector2(0, 1); lRT.sizeDelta = new Vector2(-24, 44);
        lRT.anchoredPosition = new Vector2(24, y);

        var valGO = new GameObject(label + "_Val", typeof(RectTransform));
        valGO.transform.SetParent(parent, false);
        var val = valGO.AddComponent<TextMeshProUGUI>();
        val.text = "—";
        val.fontSize = 26;
        val.fontStyle = FontStyles.Bold;
        val.alignment = TextAlignmentOptions.Right;
        val.color = StatValue;
        val.raycastTarget = false;
        var vRT = valGO.GetComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0, 1); vRT.anchorMax = new Vector2(1, 1);
        vRT.pivot = new Vector2(1, 1); vRT.sizeDelta = new Vector2(-24, 44);
        vRT.anchoredPosition = new Vector2(0, y);

        y -= 58f;
        return val;
    }

    private void ShowResultCard(PrefireRouteResult result)
    {
        int sec = Mathf.FloorToInt(result.Time);
        int ms = Mathf.FloorToInt((result.Time - sec) * 100f);

        _cardTitle.text = result.RouteName.ToUpper();
        _cardTime.text = sec.ToString("00") + "." + ms.ToString("00") + "s";
        _cardAccuracy.text = Mathf.RoundToInt(result.Accuracy * 100f) + "%";
        _cardScore.text = result.Score.ToString();

        bool isLast = _prefireManager != null && _prefireManager.CurrentRouteIndex >= _prefireManager.TotalRoutes - 1;
        if (_cardContinueLabel != null)
            _cardContinueLabel.text = isLast ? "RESTART →" : "CONTINUE →";

        _cardRoot.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _look = FindAnyObjectByType<PlayerLook>();
        _playerController = FindAnyObjectByType<PlayerController>();
        if (_look != null) _look.enabled = false;
        if (_playerController != null) _playerController.enabled = false;
    }

    private void HideResultCard()
    {
        if (_cardRoot != null) _cardRoot.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_look != null) { _look.enabled = true; _look = null; }
        if (_playerController != null) { _playerController.enabled = true; _playerController = null; }
    }

    private void OnContinueClicked()
    {
        HideResultCard();
        PrefireManager.Instance?.ContinueAfterClear();
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
}
