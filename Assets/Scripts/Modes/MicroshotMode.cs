using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Microshot", fileName = "MicroshotMode")]
public class MicroshotMode : GameModeSO
{
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.6f, 7f);
    // Overall area the cluster is allowed to wander within (centered on spawner).
    [SerializeField] private Vector3 region = new Vector3(4f, 2.4f, 3f);
    // How far the next target can sit from the previous one (horizontal flick).
    [SerializeField] private float clusterRadius = 0.8f;
    // How much the depth (z) may change from the previous target.
    [SerializeField] private float depthStep = 1f;
    [SerializeField] private float targetScale = 0.3f;

    [System.NonSerialized] private Vector3 _lastPos;
    [System.NonSerialized] private bool _hasLast;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        if (manager.Spawner != null)
        {
            manager.Spawner.transform.position = spawnerPosition;
            manager.Spawner.SetTargetScale(targetScale);
        }

        _hasLast = false;
        SpawnOne(manager);
    }

    public override void OnExit(ModeManager manager)
    {
        manager.ClearTargets();
        if (manager.Spawner != null)
            manager.Spawner.SetTargetScale(1f);
    }

    public override void OnTargetKilled(ModeManager manager, Target target)
    {
        SpawnOne(manager);
    }

    public override void OnShot(ModeManager manager, bool hitTarget) { }

    private void SpawnOne(ModeManager manager)
    {
        if (manager.Spawner == null) return;

        Vector3 center = manager.Spawner.transform.position;
        Vector3 pos;

        if (!_hasLast)
        {
            // First target starts in the middle of the region.
            pos = center;
        }
        else
        {
            // Each new target sits a short flick away from the previous one,
            // with a small depth change, so the player makes tiny corrections.
            Vector2 off = Random.insideUnitCircle * clusterRadius;
            float dz = Random.Range(-depthStep, depthStep);
            pos = _lastPos + new Vector3(off.x, off.y, dz);
        }

        // Keep the wandering cluster inside the allowed region around the spawner.
        Vector3 half = region * 0.5f;
        pos.x = Mathf.Clamp(pos.x, center.x - half.x, center.x + half.x);
        pos.y = Mathf.Clamp(pos.y, center.y - half.y, center.y + half.y);
        pos.z = Mathf.Clamp(pos.z, center.z - half.z, center.z + half.z);

        var t = manager.SpawnTargetAt(pos);
        if (t != null) _lastPos = pos;
        _hasLast = true;
    }
}
