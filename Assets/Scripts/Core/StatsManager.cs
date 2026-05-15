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

    public SessionStats CurrentStats => new SessionStats(_shotsFired, _hits, _misses, ElapsedSeconds);
    public float ElapsedSeconds => _sessionRunning ? Time.time - _sessionStartTime : 0f;

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
        Publish();
    }

    public void RecordShot(bool hit)
    {
        _shotsFired++;

        if (hit)
            _hits++;
        else
            _misses++;

        Publish();
    }

    private void Publish()
    {
        StatsChanged?.Invoke(CurrentStats);
    }
}
