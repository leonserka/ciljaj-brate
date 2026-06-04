using UnityEngine;

[CreateAssetMenu(menuName = "Ciljaj Brate/Modes/Strafetrack", fileName = "StrafetrackMode")]
public class StrafetrackMode : GameModeSO
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 2.6f, 7f);
    [SerializeField] private float targetRadius = 0.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float boundsX = 4f;
    [SerializeField] private float boundsY = 1.5f;

    [System.NonSerialized] private TrackingTarget _target;

    public override void OnEnter(ModeManager manager)
    {
        manager.ResetSession();
        manager.ClearTargets();

        var go = targetPrefab != null
            ? Instantiate(targetPrefab, spawnPosition, Quaternion.identity)
            : GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "TrackingTarget";
        go.transform.position = spawnPosition;
        go.transform.localScale = Vector3.one * targetRadius * 2f;

        _target = go.GetComponent<TrackingTarget>();
        if (_target == null) _target = go.AddComponent<TrackingTarget>();
        _target.Init(moveSpeed, boundsX, boundsY);
    }

    public override void OnExit(ModeManager manager)
    {
        if (_target != null)
            Object.Destroy(_target.gameObject);
        _target = null;
    }

    public override void OnUpdate(ModeManager manager)
    {
        if (_target == null || manager.Stats == null) return;
        manager.Stats.RecordTrackingFrame(_target.IsOnTarget, Time.deltaTime);
    }

    public override void OnTargetKilled(ModeManager manager, Target target) { }
    public override void OnShot(ModeManager manager, bool hitTarget) { }
}
