using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour, IShootable
{
    [SerializeField] private int maxHealth = 100;

    public event Action<Target> Died;

    private Collider[] _colliders;
    private int _hitCount;
    private int _health;
    private bool _dead;

    public int Health => _health;
    public bool IsDead => _dead;
    public string LastHitLabel { get; set; }

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
        _health = maxHealth;
    }

    public void OnShot(RaycastHit hit, Vector3 fromDirection, int damage = 0)
    {
        if (_dead) return;

        _hitCount++;

        if (damage > 0)
        {
            _health -= damage;
            if (_health > 0) return;
        }

        _dead = true;
        foreach (Collider c in _colliders)
            c.enabled = false;

        Died?.Invoke(this);
        Destroy(gameObject);
    }
}
