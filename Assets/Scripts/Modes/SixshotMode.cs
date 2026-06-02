using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Sixshot", fileName = "SixshotMode")]
public class SixshotMode : GameModeSO
{
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.6f, 7f);
    [SerializeField] private Vector3 spawnBox = new Vector3(5.2f, 2.9f, 0f);
    [SerializeField] private float minimumDistance = 0.6f;
    [SerializeField] private float targetScale = 0.25f;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        if (manager.Spawner != null)
        {
            manager.Spawner.transform.position = spawnerPosition;
            manager.Spawner.SetSpawnBox(spawnBox, minimumDistance);
            manager.Spawner.SetTargetScale(targetScale);
        }

        for (int i = 0; i < 6; i++)
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
