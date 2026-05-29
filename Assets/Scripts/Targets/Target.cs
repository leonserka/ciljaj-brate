using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour, IShootable
{
    public event Action<Target> Died;

    private Collider[] _colliders;
    private int _hitCount;
    private bool _dead;

    public int HitCount => _hitCount;

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    public void OnShot(RaycastHit hit, Vector3 fromDirection)
    {
        if (_dead)
            return;

        _hitCount++;
        _dead = true;

        foreach (Collider targetCollider in _colliders)
            targetCollider.enabled = false;

        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
