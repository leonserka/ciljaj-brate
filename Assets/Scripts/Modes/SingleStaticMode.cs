using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Single Static", fileName = "SingleStaticMode")]
public class SingleStaticMode : GameModeSO
{
    [SerializeField, Range(0.05f, 0.1f)] private float respawnDelay = 0.05f;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();
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
