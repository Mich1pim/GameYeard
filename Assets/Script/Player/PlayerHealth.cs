using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private int healthOnRespawn = 3;

    [Header("Возрождение")]
    [SerializeField] private Transform bedSpawnPoint;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    public event Action OnDied;

    private bool _isDead = false;

    private void Awake()
    {
        Instance = this;
        CurrentHealth = maxHealth;
    }

    public void SetHealth(int value)
    {
        CurrentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    public void TakeDamage(int dmg)
    {
        if (_isDead) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - dmg);
        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        OnDied?.Invoke();
        if (DeathFade.Instance == null)
            Respawn();
    }

    public void Respawn()
    {
        ClearInventory();
        ClearMoney();
        CurrentHealth = healthOnRespawn;
        RespawnAtBed();
        _isDead = false;
    }

    private void ClearInventory()
    {
        if (InventoryManager.Instance == null) return;
        ClearSlots(InventoryManager.Instance.inventorySlots);
        ClearSlots(InventoryManager.Instance.craftSlots);
        ClearSlots(InventoryManager.Instance.endCraftSlots);
    }

    private void ClearSlots(InventorySlot[] slots)
    {
        if (slots == null) return;
        foreach (InventorySlot slot in slots)
        {
            InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null)
                Destroy(item.gameObject);
        }
    }

    private void ClearMoney()
    {
        if (Player.Instance != null)
            Player.Instance.coin = 0;
    }

    private void RespawnAtBed()
    {
        if (bedSpawnPoint == null)
        {
            Debug.LogWarning("PlayerHealth: ссылка на кровать не назначена в инспекторе!");
            return;
        }
        transform.position = bedSpawnPoint.position;
    }
}
