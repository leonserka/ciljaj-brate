using UnityEngine;

public readonly struct SessionStats
{
    public SessionStats(int shotsFired, int hits, int misses, float elapsedSeconds)
    {
        ShotsFired = shotsFired;
        Hits = hits;
        Misses = misses;
        ElapsedSeconds = elapsedSeconds;
        Accuracy = shotsFired > 0 ? (float)hits / shotsFired : 0f;
        Score = CalculateSingleStaticScore(hits, shotsFired);
    }

    public int ShotsFired { get; }
    public int Hits { get; }
    public int Misses { get; }
    public float ElapsedSeconds { get; }
    public float Accuracy { get; }
    public float Score { get; }

    public static float CalculateSingleStaticScore(int hits, int shotsFired)
    {
        if (shotsFired <= 0 || hits <= 0)
            return 0f;

        float accuracy = Mathf.Clamp01((float)hits / shotsFired);
        return hits * 100f * accuracy;
    }
}
