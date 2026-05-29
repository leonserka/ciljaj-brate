using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Classic", fileName = "ClassicMode")]
public class ClassicMode : GameModeSO
{
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.6f, 7f);
    [SerializeField] private Vector3 spawnBoxSize = new Vector3(5.2f, 2.9f, 0f);
    [SerializeField] private float minimumDistanceFromLast = 1.1f;
    [SerializeField, Range(0.03f, 0.12f)] private float respawnDelay = 0.05f;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        if (manager.Spawner != null)
        {
            manager.Spawner.transform.position = spawnerPosition;
            manager.Spawner.SetSpawnBox(spawnBoxSize, minimumDistanceFromLast);
        }

        manager.SpawnTarget();
    }

    public override void OnExit(ModeManager manager)
    {
        manager.ClearTargets();
    }

    public override void OnTargetKilled(ModeManager manager, Target target)
    {
        manager.SpawnTargetAfter(respawnDelay);
    }

    public override void OnShot(ModeManager manager, bool hitTarget)
    {
    }
}
