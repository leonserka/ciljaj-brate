using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public static PlayerHealth Instance { get; private set; }

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        Instance = this;
        CurrentHealth = maxHealth;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
            Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
