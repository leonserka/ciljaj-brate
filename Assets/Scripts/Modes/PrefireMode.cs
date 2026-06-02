using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Prefire", fileName = "PrefireMode")]
public class PrefireMode : GameModeSO
{
    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();

        var prefireManager = PrefireManager.Instance;
        if (prefireManager == null)
            prefireManager = Object.FindAnyObjectByType<PrefireManager>();

        prefireManager?.Begin();
    }

    public override void OnExit(ModeManager manager)
    {
        var prefireManager = PrefireManager.Instance;
        prefireManager?.Stop();
    }

    public override void OnTargetKilled(ModeManager manager, Target target) { }

    public override void OnShot(ModeManager manager, bool hitTarget) { }
}
