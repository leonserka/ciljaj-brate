using UnityEngine;

public abstract class GameModeSO : ScriptableObject
{
    [SerializeField] private string modeName = "Game Mode";

    public string ModeName => modeName;

    public abstract void OnEnter(ModeManager manager);
    public abstract void OnExit(ModeManager manager);
    public abstract void OnTargetKilled(ModeManager manager, Target target);
    public abstract void OnShot(ModeManager manager, bool hitTarget);
}
