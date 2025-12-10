using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Mängija mõistuse (sanity) süsteem
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PlayerSanity : NetworkBehaviour
{
    [Header("Sanity Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity;
    
    [Header("Sanity Drain")]
    [SerializeField] private float darknessDrainRate = 2f; // Sekundis
    [SerializeField] private float jumpscareDrain = 15f;
    
    [Header("Low Sanity Effects")]
    [SerializeField] private float lowSanityThreshold = 30f;
    [SerializeField] private float cameraShakeIntensity = 0.1f;
    
    // Network variables
    private NetworkVariable<float> networkSanity = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    // References
    private PlayerController playerController;
    private Camera playerCamera;
    
    // Events
    public System.Action<float> OnSanityChanged;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            currentSanity = maxSanity;
            networkSanity.Value = maxSanity;
        }
        
        playerController = GetComponent<PlayerController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Kuula mõistuse muutusi
        networkSanity.OnValueChanged += OnSanityValueChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        networkSanity.OnValueChanged -= OnSanityValueChanged;
        base.OnNetworkDespawn();
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // Mõistuse vähenemine pimeduses
        if (playerController != null && !playerController.IsFlashlightOn())
        {
            DrainSanity(darknessDrainRate * Time.deltaTime);
        }
    }
    
    private void OnSanityValueChanged(float oldValue, float newValue)
    {
        currentSanity = newValue;
        OnSanityChanged?.Invoke(currentSanity / maxSanity);
        
        // Madal mõistus efektid
        if (currentSanity < lowSanityThreshold)
        {
            ApplyLowSanityEffects();
        }
        else
        {
            RemoveLowSanityEffects();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DrainSanityServerRpc(float amount)
    {
        DrainSanity(amount);
    }
    
    private void DrainSanity(float amount)
    {
        if (currentSanity <= 0f) return;
        
        currentSanity = Mathf.Max(0f, currentSanity - amount);
        networkSanity.Value = currentSanity;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RestoreSanityServerRpc(float amount)
    {
        RestoreSanity(amount);
    }
    
    private void RestoreSanity(float amount)
    {
        currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
        networkSanity.Value = currentSanity;
    }
    
    public void OnJumpscare()
    {
        DrainSanityServerRpc(jumpscareDrain);
    }
    
    private void ApplyLowSanityEffects()
    {
        // Camera shake
        if (playerCamera != null && IsOwner)
        {
            // TODO: Implementeeri camera shake
            // Võib kasutada Cinemachine või oma süsteemi
        }
        
        // Visuaalsed efektid
        // TODO: Lisa post-processing efektid (vignette, color grading)
    }
    
    private void RemoveLowSanityEffects()
    {
        // Eemalda efektid
        // TODO: Eemalda camera shake ja post-processing
    }
    
    // Public meetodid
    public float GetSanity()
    {
        return currentSanity;
    }
    
    public float GetSanityPercentage()
    {
        return currentSanity / maxSanity;
    }
    
    public bool IsLowSanity()
    {
        return currentSanity < lowSanityThreshold;
    }
}

