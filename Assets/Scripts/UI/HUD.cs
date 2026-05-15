using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private ModeManager modeManager;
    [SerializeField] private StatsManager statsManager;
    [SerializeField] private Text modeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text accuracyText;
    [SerializeField] private Text timerText;

    private SessionStats _lastStats;

    private void Awake()
    {
        if (modeManager == null)
            modeManager = FindAnyObjectByType<ModeManager>();

        if (statsManager == null)
            statsManager = FindAnyObjectByType<StatsManager>();
    }

    private void OnEnable()
    {
        if (statsManager != null)
            statsManager.StatsChanged += HandleStatsChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (statsManager != null)
            statsManager.StatsChanged -= HandleStatsChanged;
    }

    private void Update()
    {
        RefreshTimer();
    }

    private void HandleStatsChanged(SessionStats stats)
    {
        _lastStats = stats;
        Refresh();
    }

    private void Refresh()
    {
        if (statsManager != null)
            _lastStats = statsManager.CurrentStats;

        if (modeText != null)
            modeText.text = modeManager != null ? modeManager.CurrentModeName : "Single Static";

        if (scoreText != null)
            scoreText.text = $"Score {Mathf.RoundToInt(_lastStats.Score)}";

        if (accuracyText != null)
            accuracyText.text = $"Acc {(_lastStats.Accuracy * 100f):0}%  H {_lastStats.Hits} / S {_lastStats.ShotsFired}";

        RefreshTimer();
    }

    private void RefreshTimer()
    {
        if (timerText == null)
            return;

        float elapsed = statsManager != null ? statsManager.ElapsedSeconds : _lastStats.ElapsedSeconds;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
