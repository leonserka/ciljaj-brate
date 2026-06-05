using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AimTrainingHUD : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (SceneManager.GetActiveScene().name != "AimTraining") return;
        new GameObject("AimTrainingHUD").AddComponent<AimTrainingHUD>();
    }

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f);
    private static readonly Color PanelBG = new Color(0.07f, 0.07f, 0.09f, 0.97f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color StatLabel = new Color(0.55f, 0.55f, 0.58f);
    private static readonly Color StatValue = new Color(0.95f, 0.95f, 0.97f);

    private GameObject _cardRoot;
    private TMP_Text _cardTime;
    private TMP_Text _cardAccuracy;
    private TMP_Text _cardScore;
    private PlayerLook _look;
    private PlayerController _playerController;

    private void Start()
    {
        BuildCard();
        var rc = RoundController.Instance;
        if (rc != null) rc.RoundEnded += OnRoundEnded;
    }

    private void OnDestroy()
    {
        var rc = RoundController.Instance;
        if (rc != null) rc.RoundEnded -= OnRoundEnded;
    }

    private void OnRoundEnded(SessionStats stats)
    {
        int totalSec = Mathf.FloorToInt(stats.ElapsedSeconds);
        int min = totalSec / 60;
        int sec = totalSec % 60;
        _cardTime.text = min.ToString("00") + ":" + sec.ToString("00");
        _cardAccuracy.text = Mathf.RoundToInt(stats.Accuracy * 100f) + "%";
        _cardScore.text = Mathf.RoundToInt(stats.Score).ToString();

        _cardRoot.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _look = FindAnyObjectByType<PlayerLook>();
        _playerController = FindAnyObjectByType<PlayerController>();
        if (_look != null) _look.enabled = false;
        if (_playerController != null) _playerController.enabled = false;
    }

    private void HideCard()
    {
        _cardRoot.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (_look != null) { _look.enabled = true; _look = null; }
        if (_playerController != null) { _playerController.enabled = true; _playerController = null; }
    }

    private void OnPlayAgainClicked()
    {
        HideCard();
        RoundController.Instance?.ShowClickToBegin();
    }

    private void BuildCard()
    {
        var canvasGO = new GameObject("AimResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        var titleTMP = MakeText("Title", panel.transform, "ROUND OVER", 28, FontStyles.Bold, TextAlignmentOptions.Left);
        var tRT = titleTMP.rectTransform;
        tRT.anchorMin = new Vector2(0, 1); tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot = new Vector2(0, 1); tRT.sizeDelta = new Vector2(-40, 50);
        tRT.anchoredPosition = new Vector2(24, -24);
        titleTMP.color = Color.white;

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

        var btnGO = new GameObject("PlayAgainBtn", typeof(RectTransform), typeof(Image));
        btnGO.transform.SetParent(panel.transform, false);
        var bRT = btnGO.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0, 0); bRT.anchorMax = new Vector2(1, 0);
        bRT.pivot = new Vector2(0.5f, 0); bRT.sizeDelta = new Vector2(-40, 56);
        bRT.anchoredPosition = new Vector2(0, 12);
        btnGO.GetComponent<Image>().color = Cherry;
        var btn = btnGO.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btnGO.AddComponent<MenuButtonEffect>().hoverVolume = 0.35f;
        btn.onClick.AddListener(OnPlayAgainClicked);

        var lbl = MakeText("Label", btnGO.transform, "PLAY AGAIN →", 17, FontStyles.Bold, TextAlignmentOptions.Center);
        lbl.color = Color.white;
        var clRT = lbl.rectTransform;
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
