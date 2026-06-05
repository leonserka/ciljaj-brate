using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    public event Action<SessionStats> StatsChanged;

    private int _shotsFired;
    private int _hits;
    private int _misses;
    private float _sessionStartTime;
    private bool _sessionRunning;
    private bool _lastShotHit;
    private float _score;
    private int _combo;
    private int _bestCombo;

    private const float BaseHitPoints = 100f;
    private const float MissDeduction = 50f;
    private const float MaxMultiplier = 5f;

    public SessionStats CurrentStats => new SessionStats(
        _shotsFired, _hits, _misses, ElapsedSeconds, _lastShotHit, _score, _combo);
    public float ElapsedSeconds => _sessionRunning ? Time.time - _sessionStartTime : 0f;
    public int Combo => _combo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ResetSession()
    {
        _shotsFired = 0;
        _hits = 0;
        _misses = 0;
        _sessionStartTime = Time.time;
        _sessionRunning = true;
        _score = 0;
        _combo = 0;
        _bestCombo = 0;
        Publish();
    }

    public void RecordShot(bool hit)
    {
        _shotsFired++;
        _lastShotHit = hit;

        if (hit)
        {
            _hits++;
            _combo++;
            if (_combo > _bestCombo) _bestCombo = _combo;

            float multiplier = Mathf.Min(1f + (_combo - 1) * 0.15f, MaxMultiplier);
            _score += BaseHitPoints * multiplier;
        }
        else
        {
            _misses++;
            _combo = 0;
            _score = Mathf.Max(0, _score - MissDeduction);
        }

        Publish();
    }

    public void RecordTrackingFrame(bool onTarget, float deltaTime)
    {
        _shotsFired++;
        _lastShotHit = onTarget;
        if (onTarget)
        {
            _hits++;
            _combo++;
            if (_combo > _bestCombo) _bestCombo = _combo;
            _score += 150f * deltaTime;
        }
        else
        {
            _misses++;
            _combo = 0;
        }
        Publish();
    }

    private void Publish()
    {
        StatsChanged?.Invoke(CurrentStats);
    }
}
