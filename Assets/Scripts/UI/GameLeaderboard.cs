using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLeaderboard : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene != "Prefire" && scene != "AimTraining") return;
        new GameObject("GameLeaderboard").AddComponent<GameLeaderboard>();
    }

    private static readonly Color Cherry = new Color(0.55f, 0.05f, 0.20f);
    private static readonly Color PanelBG = new Color(0.05f, 0.05f, 0.07f, 0.65f);
    private static readonly Color EntryText = new Color(0.92f, 0.92f, 0.94f);
    private static readonly Color DimText = new Color(0.65f, 0.65f, 0.68f);
    private static readonly Color SectionHeader = new Color(0.80f, 0.80f, 0.82f);

    private const int MaxEntriesPerMode = 5;
    private const float HeaderH = 38f;
    private const float SectionH = 26f;
    private const float RowH = 28f;
    private const float BottomPad = 8f;

    private bool _isPrefire;
    private GameObject _panel;
    private Transform _listContent;

    private struct RoundEntry
    {
        public int Round;
        public float Accuracy;
        public int Score;
    }

    private readonly Dictionary<string, List<RoundEntry>> _modeEntries = new();
    private readonly List<string> _modeOrder = new();

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        _isPrefire = scene == "Prefire";
        BuildUI();

        if (_isPrefire)
        {
            var pm = PrefireManager.Instance;
            if (pm != null)
            {
                pm.RouteCleared += OnRouteCleared;
                RefreshFromPrefire();
            }
        }
        else
        {
            var rc = RoundController.Instance;
            if (rc != null) rc.RoundEnded += OnRoundEnded;
        }
    }

    private void OnDestroy()
    {
        if (_isPrefire && PrefireManager.Instance != null)
            PrefireManager.Instance.RouteCleared -= OnRouteCleared;
        if (!_isPrefire && RoundController.Instance != null)
            RoundController.Instance.RoundEnded -= OnRoundEnded;
    }

    private void OnRouteCleared(PrefireRouteResult result) => RefreshFromPrefire();

    private void OnRoundEnded(SessionStats stats)
    {
        string modeName = ModeManager.Current != null ? ModeManager.Current.CurrentModeName : "Unknown";

        if (!_modeEntries.ContainsKey(modeName))
        {
            _modeEntries[modeName] = new List<RoundEntry>();
            _modeOrder.Add(modeName);
        }

        var list = _modeEntries[modeName];
        list.Add(new RoundEntry
        {
            Round = list.Count + 1,
            Accuracy = stats.Accuracy,
            Score = Mathf.RoundToInt(stats.Score)
        });
        if (list.Count > MaxEntriesPerMode) list.RemoveAt(0);

        RebuildList();
    }

    private void RefreshFromPrefire()
    {
        _modeEntries.Clear();
        _modeOrder.Clear();
        var pm = PrefireManager.Instance;
        if (pm?.Results == null) return;

        const string key = "ROUTES";
        _modeEntries[key] = new List<RoundEntry>();
        _modeOrder.Add(key);
        for (int i = 0; i < pm.Results.Count; i++)
        {
            var r = pm.Results[i];
            if (r == null) continue;
            _modeEntries[key].Add(new RoundEntry
            {
                Round = i + 1,
                Accuracy = r.Accuracy,
                Score = r.Score
            });
        }
        RebuildList();
    }

    private void RebuildList()
    {
        if (_listContent == null || _panel == null) return;

        foreach (Transform child in _listContent)
            Destroy(child.gameObject);

        float y = 0f;
        int totalRows = 0;
        int sectionCount = 0;

        foreach (string mode in _modeOrder)
        {
            if (!_modeEntries.ContainsKey(mode)) continue;
            var list = _modeEntries[mode];
            if (list.Count == 0) continue;

            string sectionLabel = _isPrefire ? "" : mode.ToUpper();
            if (!_isPrefire && sectionLabel.Length > 0)
            {
                var hdr = new GameObject("Hdr_" + mode, typeof(RectTransform));
                hdr.transform.SetParent(_listContent, false);
                var hRT = hdr.GetComponent<RectTransform>();
                hRT.anchorMin = new Vector2(0, 1); hRT.anchorMax = new Vector2(1, 1);
                hRT.pivot = new Vector2(0.5f, 1);
                hRT.sizeDelta = new Vector2(0, SectionH);
                hRT.anchoredPosition = new Vector2(0, y);

                var hTmp = hdr.AddComponent<TextMeshProUGUI>();
                hTmp.text = sectionLabel;
                hTmp.fontSize = 11;
                hTmp.fontStyle = FontStyles.Bold;
                hTmp.alignment = TextAlignmentOptions.Left;
                hTmp.color = SectionHeader;
                hTmp.raycastTarget = false;
                hTmp.margin = new Vector4(10, 0, 0, 0);

                y -= SectionH;
                sectionCount++;
            }

            int bestIdx = -1;
            int bestScore = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Score > bestScore) { bestScore = list[i].Score; bestIdx = i; }
            }

            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                bool isBest = i == bestIdx;

                var row = new GameObject("Row", typeof(RectTransform));
                row.transform.SetParent(_listContent, false);
                var rRT = row.GetComponent<RectTransform>();
                rRT.anchorMin = new Vector2(0, 1); rRT.anchorMax = new Vector2(1, 1);
                rRT.pivot = new Vector2(0.5f, 1);
                rRT.sizeDelta = new Vector2(0, RowH);
                rRT.anchoredPosition = new Vector2(0, y);

                string label = _isPrefire
                    ? (PrefireManager.Instance != null && PrefireManager.Instance.Results != null && i < PrefireManager.Instance.Results.Count && PrefireManager.Instance.Results[i] != null
                        ? PrefireManager.Instance.Results[i].RouteName : "#" + e.Round)
                    : "#" + e.Round;
                if (label.Length > 14) label = label.Substring(0, 14);

                Color nameColor = isBest ? Color.white : EntryText;
                Color accColor = DimText;
                Color scoreColor = isBest ? Cherry : DimText;

                MakeCell(row.transform, label.ToUpper(),
                    new Vector2(0, 0), new Vector2(0.50f, 1), new Vector2(10, 0), new Vector2(-2, 0),
                    11, FontStyles.Bold, TextAlignmentOptions.Left, nameColor);

                MakeCell(row.transform, Mathf.RoundToInt(e.Accuracy * 100f) + "%",
                    new Vector2(0.50f, 0), new Vector2(0.74f, 1), Vector2.zero, Vector2.zero,
                    11, FontStyles.Normal, TextAlignmentOptions.Right, accColor);

                MakeCell(row.transform, e.Score.ToString(),
                    new Vector2(0.74f, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-8, 0),
                    12, FontStyles.Bold, TextAlignmentOptions.Right, scoreColor);

                y -= RowH;
                totalRows++;
            }
        }

        var lcRT = _listContent.GetComponent<RectTransform>();
        lcRT.sizeDelta = new Vector2(0, -y);

        var pr = _panel.GetComponent<RectTransform>();
        float totalH = HeaderH + (-y) + BottomPad;
        pr.sizeDelta = new Vector2(260, Mathf.Max(HeaderH + BottomPad, totalH));
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("LeaderboardCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(canvasGO.transform, false);
        var pr = _panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(1f, 1f);
        pr.pivot = new Vector2(1f, 1f);
        pr.sizeDelta = new Vector2(260, HeaderH + BottomPad);
        pr.anchoredPosition = new Vector2(-12, -12);
        _panel.GetComponent<Image>().color = PanelBG;

        var titleGO = new GameObject("Title", typeof(RectTransform));
        titleGO.transform.SetParent(_panel.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "LEADERBOARD";
        titleTMP.fontSize = 12;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Left;
        titleTMP.color = Color.white;
        titleTMP.raycastTarget = false;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1); titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0, 1); titleRT.sizeDelta = new Vector2(-20, 26);
        titleRT.anchoredPosition = new Vector2(10, -6);

        MakeHLine(_panel.transform, -34f);

        var entryGO = new GameObject("Entries", typeof(RectTransform));
        entryGO.transform.SetParent(_panel.transform, false);
        var ecRT = entryGO.GetComponent<RectTransform>();
        ecRT.anchorMin = new Vector2(0, 1); ecRT.anchorMax = new Vector2(1, 1);
        ecRT.pivot = new Vector2(0.5f, 1);
        ecRT.sizeDelta = new Vector2(0, 0);
        ecRT.anchoredPosition = new Vector2(0, -HeaderH);
        _listContent = entryGO.transform;
    }

    private static void MakeHLine(Transform parent, float y)
    {
        var go = new GameObject("Div", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1); rt.sizeDelta = new Vector2(-16, 1);
        rt.anchoredPosition = new Vector2(0, y);
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
    }

    private static void MakeCell(Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, FontStyles style, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject("Cell", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
    }
}
