using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 1.2f;
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private float fadeDelay = 0.2f;
    [SerializeField] private Color hitColor = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private Color missColor = new Color(1f, 0.3f, 0.2f, 0.8f);

    private RectTransform _rect;
    private Canvas _canvas;
    private Camera _cam;
    private StatsManager _stats;
    private float _lastScore;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        var tmp = GetComponent<TMP_Text>();
        if (tmp != null) tmp.enabled = false;
        _canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        _cam = Camera.main;
        _stats = StatsManager.Instance != null ? StatsManager.Instance : FindAnyObjectByType<StatsManager>();
        if (_stats != null)
        {
            _lastScore = _stats.CurrentStats.Score;
            _stats.StatsChanged += OnStats;
        }
    }

    private void Start()
    {
        if (_stats == null)
        {
            _stats = StatsManager.Instance != null ? StatsManager.Instance : FindAnyObjectByType<StatsManager>();
            if (_stats != null)
            {
                _lastScore = _stats.CurrentStats.Score;
                _stats.StatsChanged += OnStats;
            }
        }
    }

    private void OnDisable()
    {
        if (_stats != null)
            _stats.StatsChanged -= OnStats;
    }

    private void OnStats(SessionStats stats)
    {
        float diff = stats.Score - _lastScore;
        _lastScore = stats.Score;

        if (Mathf.Abs(diff) < 0.5f) return;

        SpawnPopup(diff);
    }

    private void SpawnPopup(float diff)
    {
        if (_cam == null) _cam = Camera.main;

        Vector3 worldPos = PlayerWeapon.LastShotPoint;

        var go = new GameObject("ScorePop");
        go.transform.SetParent(transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(120, 30);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        int points = Mathf.RoundToInt(diff);
        tmp.text = points > 0 ? "+" + points : points.ToString();
        tmp.fontSize = points > 0 ? 22 : 16;
        tmp.color = points > 0 ? hitColor : missColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        go.AddComponent<ScorePopupFloat>().Init(worldPos, _cam, _rect, riseSpeed, lifetime, fadeDelay);
    }
}
