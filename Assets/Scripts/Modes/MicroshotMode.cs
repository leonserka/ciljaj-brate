using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Microshot", fileName = "MicroshotMode")]
public class MicroshotMode : GameModeSO
{
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.6f, 7f);
    [SerializeField] private Vector3 spawnBox = new Vector3(1.2f, 0.8f, 0f);
    [SerializeField] private float targetScale = 0.3f;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        if (manager.Spawner != null)
        {
            manager.Spawner.transform.position = spawnerPosition;
            manager.Spawner.SetSpawnBox(spawnBox, 0f);
            manager.Spawner.SetTargetScale(targetScale);
        }

        manager.SpawnTarget();
    }

    public override void OnExit(ModeManager manager)
    {
        manager.ClearTargets();
        if (manager.Spawner != null)
            manager.Spawner.SetTargetScale(1f);
    }

    public override void OnTargetKilled(ModeManager manager, Target target)
    {
        manager.SpawnTarget();
    }

    public override void OnShot(ModeManager manager, bool hitTarget) { }
}
