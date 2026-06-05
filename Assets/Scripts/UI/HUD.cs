using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private StatsManager statsManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text timerText;

    [Header("Score Pop")]
    [SerializeField] private float popScale = 1.4f;
    [SerializeField] private float popSpeed = 8f;

    [Header("Number Rolling")]
    [SerializeField] private float rollSpeed = 12f;

    private float _displayScore;
    private float _displayAccuracy;
    private int _targetScore;
    private float _targetAccuracy;
    private int _lastScore;
    private Vector3 _scoreBaseScale;
    private float _scorePunch;

    private void Awake()
    {
        if (statsManager == null)
            statsManager = FindAnyObjectByType<StatsManager>();
    }

    private void OnEnable()
    {
        if (scoreText != null)
            _scoreBaseScale = scoreText.transform.localScale;

        if (statsManager != null)
            statsManager.StatsChanged += HandleStatsChanged;
        SnapToCurrentStats();
    }

    private void OnDisable()
    {
        if (statsManager != null)
            statsManager.StatsChanged -= HandleStatsChanged;
    }

    private void Update()
    {
        AnimateScore();
        AnimateAccuracy();
        RefreshTimer();
    }

    private void HandleStatsChanged(SessionStats stats)
    {
        int newScore = Mathf.RoundToInt(stats.Score);
        if (newScore > _lastScore)
            _scorePunch = 1f;
        _lastScore = newScore;

        _targetScore = newScore;
        _targetAccuracy = stats.Accuracy * 100f;
    }

    private void SnapToCurrentStats()
    {
        if (statsManager == null) return;
        var s = statsManager.CurrentStats;
        _targetScore = Mathf.RoundToInt(s.Score);
        _targetAccuracy = s.Accuracy * 100f;
        _displayScore = _targetScore;
        _displayAccuracy = _targetAccuracy;
        _lastScore = _targetScore;
        UpdateScoreText();
        UpdateAccuracyText();
    }

    private void AnimateScore()
    {
        if (scoreText == null) return;

        _displayScore = Mathf.MoveTowards(_displayScore, _targetScore, rollSpeed * Time.deltaTime * Mathf.Max(1f, Mathf.Abs(_targetScore - _displayScore)));
        UpdateScoreText();


        _scorePunch = Mathf.MoveTowards(_scorePunch, 0f, popSpeed * Time.deltaTime);
        float scale = 1f + _scorePunch * (popScale - 1f);
        scoreText.transform.localScale = _scoreBaseScale * scale;
    }

    private void AnimateAccuracy()
    {
        if (accuracyText == null) return;
        _displayAccuracy = Mathf.Lerp(_displayAccuracy, _targetAccuracy, rollSpeed * Time.deltaTime);
        UpdateAccuracyText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = Mathf.RoundToInt(_displayScore).ToString();
    }

    private void UpdateAccuracyText()
    {
        if (accuracyText != null)
            accuracyText.text = Mathf.RoundToInt(_displayAccuracy) + "%";
    }

    private void RefreshTimer()
    {
        if (timerText == null) return;

        float display;
        if (RoundController.Instance != null)
            display = RoundController.Instance.TimeRemaining;
        else
            display = statsManager != null ? statsManager.ElapsedSeconds : 0f;

        int minutes = Mathf.FloorToInt(display / 60f);
        int seconds = Mathf.FloorToInt(display % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
