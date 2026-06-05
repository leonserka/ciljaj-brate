using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Spidershot", fileName = "SpidershotMode")]
public class SpidershotMode : GameModeSO
{
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.6f, 7f);
    [SerializeField] private Vector3 outerSpawnBox = new Vector3(5f, 3f, 0f);
    [SerializeField] private float centerScale = 0.8f;
    [SerializeField] private float outerScale = 0.55f;

    [System.NonSerialized] private bool _nextIsCenter;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        if (manager.Spawner != null)
        {
            manager.Spawner.transform.position = spawnerPosition;
            manager.Spawner.SetSpawnBox(outerSpawnBox, 0f);
        }

        _nextIsCenter = true;
        SpawnNext(manager);
    }

    public override void OnExit(ModeManager manager)
    {
        manager.ClearTargets();
        if (manager.Spawner != null)
            manager.Spawner.SetTargetScale(1f);
    }

    public override void OnTargetKilled(ModeManager manager, Target target)
    {
        _nextIsCenter = !_nextIsCenter;
        SpawnNext(manager);
    }

    public override void OnShot(ModeManager manager, bool hitTarget) { }

    private void SpawnNext(ModeManager manager)
    {
        if (manager.Spawner == null) return;

        if (_nextIsCenter)
        {
            manager.Spawner.SetTargetScale(centerScale);


            Vector3 p = manager.Spawner.transform.position;
            p.z += Random.Range(-outerSpawnBox.z * 0.5f, outerSpawnBox.z * 0.5f);
            manager.SpawnTargetAt(p);
        }
        else
        {
            manager.Spawner.SetTargetScale(outerScale);
            manager.SpawnTarget();
        }
    }
}
