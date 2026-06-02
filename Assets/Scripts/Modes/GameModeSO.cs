using UnityEngine;

public abstract class GameModeSO : ScriptableObject
{
    [SerializeField] private string modeName = "Game Mode";
    [SerializeField] private float duration;

    public string ModeName => modeName;
    public float Duration => duration;

    public abstract void OnEnter(ModeManager manager);
    public abstract void OnExit(ModeManager manager);
    public abstract void OnTargetKilled(ModeManager manager, Target target);
    public abstract void OnShot(ModeManager manager, bool hitTarget);
    public virtual void OnUpdate(ModeManager manager) { }
}
