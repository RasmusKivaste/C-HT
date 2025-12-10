using UnityEngine;
using Unity.Netcode;

 /// <summary>

 /// </summary>

 </summary>
[RequireComponent(typeof(NetworkObject))]
public class JumpscareTrigger : NetworkBehaviour
{
    [Header("Jumpscare Settings")]
    [SerializeField] private JumpscareType jumpscareType = JumpscareType.Visual;
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] [Range(0f, 1f)] private float triggerChance = 1f;
    
    [Header("Visual Jumpscare")]
    [SerializeField] private Sprite jumpscareImage;
    [SerializeField] private float duration = 1f;
    
    [Header("Audio Jumpscare")]
    [SerializeField] private AudioClip jumpscareSound;
    [SerializeField] private float volume = 2f;
    
    [Header("Effects")]
    [SerializeField] private float sanityDamage = 15f;
    [SerializeField] private float knockbackForce = 500f;
    
    private bool hasTriggered = false;
    private NetworkVariable<bool> networkHasTriggered = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public enum JumpscareType
    {
        Visual,
        Audio,
        Environmental,
        Enemy
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        networkHasTriggered.OnValueChanged += OnTriggeredChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        networkHasTriggered.OnValueChanged -= OnTriggeredChanged;
        base.OnNetworkDespawn();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (hasTriggered && oneTimeOnly) return;
        
        // Kontrolli, kas on mängija
        if (other.CompareTag("Player"))
        {
            // Juhuslik tõenäosus
            if (Random.Range(0f, 1f) < triggerChance)
            {
                TriggerJumpscareServerRpc(other.GetComponent<NetworkObject>().OwnerClientId);
                hasTriggered = true;
                networkHasTriggered.Value = true;
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TriggerJumpscareServerRpc(ulong clientId)
    {
        TriggerJumpscareClientRpc(clientId);
    }
    
    [ClientRpc]
    private void TriggerJumpscareClientRpc(ulong clientId)
    {
        // Leia mängija
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        
        if (networkManager.LocalClientId == clientId)
        {
            // Käivita jumpscare
            JumpscareManager.Instance?.TriggerJumpscare(this);
            
            // Kahjusta mõistust
            if (TryGetComponent<PlayerSanity>(out var sanity))
            {
                sanity.OnJumpscare();
            }
            
            // Knockback
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 knockbackDirection = -transform.forward;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }
    
    private void OnTriggeredChanged(bool oldValue, bool newValue)
    {
        hasTriggered = newValue;
    }
    
    // Public meetodid
    public JumpscareType GetJumpscareType()
    {
        return jumpscareType;
    }
    
    public Sprite GetJumpscareImage()
    {
        return jumpscareImage;
    }
    
    public AudioClip GetJumpscareSound()
    {
        return jumpscareSound;
    }
    
    public float GetDuration()
    {
        return duration;
    }
    
    public float GetVolume()
    {
        return volume;
    }
}

