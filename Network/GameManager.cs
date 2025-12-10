using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Mängu manager, mis haldab mängu olekut ja loogikat
/// </summary>
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private float gameTimeLimit = 3600f; // 60 minutit sekundites
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int maxPlayers = 4;
    
    // Network variables
    private NetworkVariable<float> gameTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private NetworkVariable<bool> gameEnded = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    // Events
    public System.Action OnGameStart;
    public System.Action OnGameEnd;
    public System.Action<bool> OnGameWin; // true = võit, false = kaotus
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        gameTime.OnValueChanged += OnGameTimeChanged;
        gameStarted.OnValueChanged += OnGameStartedChanged;
        gameEnded.OnValueChanged += OnGameEndedChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        gameTime.OnValueChanged -= OnGameTimeChanged;
        gameStarted.OnValueChanged -= OnGameStartedChanged;
        gameEnded.OnValueChanged -= OnGameEndedChanged;
        base.OnNetworkDespawn();
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // Mängu aja jälgimine
        if (gameStarted.Value && !gameEnded.Value)
        {
            gameTime.Value += Time.deltaTime;
            
            // Kontrolli, kas aeg läbi
            if (gameTime.Value >= gameTimeLimit)
            {
                EndGame(false); // Kaotus - aeg läbi
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (gameStarted.Value) return;
        
        // Kontrolli mängijate arvu
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        if (playerCount < minPlayers)
        {
            Debug.LogWarning($"Vaja vähemalt {minPlayers} mängijat!");
            return;
        }
        
        gameStarted.Value = true;
        gameTime.Value = 0f;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc(bool won)
    {
        EndGame(won);
    }
    
    private void EndGame(bool won)
    {
        if (gameEnded.Value) return;
        
        gameEnded.Value = true;
        OnGameWin?.Invoke(won);
        OnGameEnd?.Invoke();
    }
    
    private void OnGameTimeChanged(float oldValue, float newValue)
    {
        // TODO: Uuenda UI'd
    }
    
    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            OnGameStart?.Invoke();
        }
    }
    
    private void OnGameEndedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            OnGameEnd?.Invoke();
        }
    }
    
    // Public meetodid
    public float GetGameTime()
    {
        return gameTime.Value;
    }
    
    public bool IsGameStarted()
    {
        return gameStarted.Value;
    }
    
    public bool IsGameEnded()
    {
        return gameEnded.Value;
    }
}

