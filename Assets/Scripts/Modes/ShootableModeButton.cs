using UnityEngine;

public class ShootableModeButton : MonoBehaviour, IShootable
{
    private GameModeSO _mode;
    private Renderer _bgRenderer;
    private static readonly Color NormalColor = new Color(0.12f, 0.12f, 0.14f);
    private static readonly Color HitColor = new Color(0.45f, 0.04f, 0.10f);

    public void Init(GameModeSO mode)
    {
        _mode = mode;
    }

    private void Awake()
    {
        _bgRenderer = GetComponentInChildren<Renderer>();
    }

    public void OnShot(RaycastHit hit, Vector3 fromDirection)
    {
        if (_mode == null) return;

        var manager = ModeManager.Current;
        if (manager == null) return;

        manager.SwitchMode(_mode);

        if (_bgRenderer != null)
            _bgRenderer.material.color = HitColor;

        Invoke(nameof(ResetColor), 0.3f);
    }

    private void ResetColor()
    {
        if (_bgRenderer != null)
            _bgRenderer.material.color = NormalColor;
    }
}
