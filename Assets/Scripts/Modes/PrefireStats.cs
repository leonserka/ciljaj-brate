using UnityEngine;

public class PrefireRouteResult
{
    public string RouteName;
    public float Time;
    public int ShotsFired;
    public int ShotsHit;

    public float Accuracy => ShotsFired > 0 ? (float)ShotsHit / ShotsFired : 0f;

    // Score 0-1000: 70% from accuracy, 30% from speed (full bonus under 120s, zero at 120s+)
    public int Score
    {
        get
        {
            float speedFactor = Mathf.Max(0f, 1f - Time / 120f);
            return Mathf.RoundToInt(Accuracy * 700f + speedFactor * 300f);
        }
    }
}
