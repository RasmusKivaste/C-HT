using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Mängija tervise süsteem
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Death Settings")]
    [SerializeField] private float respawnTime = 5f;
    
    // Network variables
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    // Events
    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;
    public System.Action OnRespawn;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            currentHealth = maxHealth;
            networkHealth.Value = maxHealth;
        }
        
        // Kuula tervise muutusi
        networkHealth.OnValueChanged += OnHealthValueChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        networkHealth.OnValueChanged -= OnHealthValueChanged;
        base.OnNetworkDespawn();
    }
    
    private void OnHealthValueChanged(float oldValue, float newValue)
    {
        currentHealth = newValue;
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (currentHealth <= 0f) return; // Juba surnud
        
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        networkHealth.Value = currentHealth;
        
        if (currentHealth <= 0f)
        {
            HandleDeath();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        if (currentHealth <= 0f) return; // Surnud mängijat ei saa ravida
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        networkHealth.Value = currentHealth;
    }
    
    private void HandleDeath()
    {
        OnDeath?.Invoke();
        
        // Keela mängija kontrollid
        if (TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = false;
        }
        
        // Alusta respawn timerit
        Invoke(nameof(Respawn), respawnTime);
    }
    
    private void Respawn()
    {
        // Taasta tervis
        if (IsServer)
        {
            currentHealth = maxHealth;
            networkHealth.Value = maxHealth;
        }
        
        // Taasta kontrollid
        if (TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = true;
        }
        
        // Teleport spawn point'i
        // TODO: Implementeeri spawn point süsteem
        
        OnRespawn?.Invoke();
    }
    
    // Public meetodid
    public float GetHealth()
    {
        return currentHealth;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
}

