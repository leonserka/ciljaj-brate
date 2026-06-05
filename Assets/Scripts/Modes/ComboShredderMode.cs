using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Gridshot", fileName = "GridshotMode")]
public class ComboShredderMode : GameModeSO
{
    [SerializeField] private int gridCols = 4;
    [SerializeField] private int gridRows = 4;
    [SerializeField] private Vector3 gridArea = new Vector3(2.4f, 2.4f, 0f);
    [SerializeField] private int targetCount = 3;
    [SerializeField] private Vector3 spawnerPosition = new Vector3(0f, 2.2f, 7f);

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        manager.Spawner.transform.position = spawnerPosition;
        manager.Spawner.SetupGrid(gridCols, gridRows, gridArea);

        for (int i = 0; i < targetCount; i++)
            manager.SpawnTarget();
    }

    public override void OnExit(ModeManager manager)
    {
        manager.ClearTargets();
    }

    public override void OnTargetKilled(ModeManager manager, Target target)
    {
        manager.SpawnTarget();
    }

    public override void OnShot(ModeManager manager, bool hitTarget)
    {
    }
}
