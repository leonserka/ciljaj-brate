using UnityEngine;

public readonly struct SessionStats
{
    public SessionStats(int shotsFired, int hits, int misses, float elapsedSeconds,
        bool lastShotHit, float score, int combo)
    {
        ShotsFired = shotsFired;
        Hits = hits;
        Misses = misses;
        ElapsedSeconds = elapsedSeconds;
        Accuracy = shotsFired > 0 ? (float)hits / shotsFired : 0f;
        Score = score;
        LastShotHit = lastShotHit;
        Combo = combo;
    }

    public int ShotsFired { get; }
    public int Hits { get; }
    public int Misses { get; }
    public float ElapsedSeconds { get; }
    public float Accuracy { get; }
    public float Score { get; }
    public bool LastShotHit { get; }
    public int Combo { get; }
}
