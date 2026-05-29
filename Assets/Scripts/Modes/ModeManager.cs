using UnityEngine;

public class ModeManager : MonoBehaviour
{
    [SerializeField] private GameModeSO activeMode;
    [SerializeField] private TargetSpawner targetSpawner;
    [SerializeField] private StatsManager statsManager;

    public static ModeManager Instance { get; private set; }
    public static ModeManager Current => Instance != null ? Instance : FindAnyObjectByType<ModeManager>();

    public string CurrentModeName => activeMode != null ? activeMode.ModeName : "No Mode";
    public StatsManager Stats => statsManager;
    public GameModeSO ActiveMode => activeMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (targetSpawner == null)
            targetSpawner = FindAnyObjectByType<TargetSpawner>();

        if (statsManager == null)
            statsManager = FindAnyObjectByType<StatsManager>();
    }

    private void Start()
    {
        if (RoundController.Instance == null)
            activeMode?.OnEnter(this);
    }

    private void OnDestroy()
    {
        if (activeMode != null && Instance == this)
            activeMode.OnExit(this);

        if (Instance == this)
            Instance = null;
    }

    public TargetSpawner Spawner => targetSpawner;

    public void SwitchMode(GameModeSO newMode)
    {
        if (activeMode != null)
            activeMode.OnExit(this);

        activeMode = newMode;

        if (activeMode != null)
            activeMode.OnEnter(this);
    }

    public void HandleShot(bool hitTarget)
    {
        statsManager?.RecordShot(hitTarget);
        activeMode?.OnShot(this, hitTarget);
    }

    public void ResetSession()
    {
        statsManager?.ResetSession();
    }

    public Target SpawnTarget()
    {
        Target target = targetSpawner != null ? targetSpawner.Spawn() : null;
        SubscribeTarget(target);
        return target;
    }

    public void SpawnTargetAfter(float delay)
    {
        if (targetSpawner == null)
            return;

        targetSpawner.SpawnAfter(delay, SubscribeTarget);
    }

    public void ClearTargets()
    {
        targetSpawner?.DespawnAll();
    }

    private void SubscribeTarget(Target target)
    {
        if (target != null)
            target.Died += HandleTargetKilled;
    }

    private void HandleTargetKilled(Target target)
    {
        target.Died -= HandleTargetKilled;
        targetSpawner?.Forget(target);
        activeMode?.OnTargetKilled(this, target);
    }
}
