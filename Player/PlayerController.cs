using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Mängija liikumise ja kontrolli skript
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Flashlight")]
    [SerializeField] private GameObject flashlight;
    [SerializeField] private float batteryLife = 300f; // 5 minutit sekundites
    [SerializeField] private float batteryDrainRate = 1f; // 1 sekund sekundis
    
    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private float currentSpeed;
    private float currentBattery;
    private bool isFlashlightOn = false;
    
    // Input variables
    private Vector2 moveInput;
    private Vector2 mouseInput;
    private bool jumpInput;
    private bool runInput;
    private bool flashlightToggleInput;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Ainult oma mängija jaoks
        if (!IsOwner)
        {
            // Keela teiste mängijate kaamerad
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
                playerCamera.GetComponent<AudioListener>().enabled = false;
            }
            return;
        }
        
        // Seadista kaamera
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Algne patarei
        currentBattery = batteryLife;
        
        // Taskulamp alguses väljas
        if (flashlight != null)
        {
            flashlight.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Ainult oma mängija jaoks
        if (!IsOwner) return;
        
        HandleInput();
        HandleMovement();
        HandleMouseLook();
        HandleFlashlight();
    }
    
    private void HandleInput()
    {
        // Liikumine
        moveInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        
        // Jooksmine
        runInput = Input.GetKey(KeyCode.LeftShift);
        
        // Hüppamine
        jumpInput = Input.GetButtonDown("Jump");
        
        // Taskulamp
        flashlightToggleInput = Input.GetKeyDown(KeyCode.F);
        
        // Mouse input
        mouseInput = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }
    
    private void HandleMovement()
    {
        // Kontrolli, kas mängija on maas
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Väike allavajutus, et kindlaks teha, et on maas
        }
        
        // Kiirus
        currentSpeed = runInput ? runSpeed : walkSpeed;
        
        // Liikumise suund
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move = move.normalized * currentSpeed;
        
        // Hüppamine
        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Gravitatsioon
        velocity.y += gravity * Time.deltaTime;
        
        // Liiguta mängijat
        controller.Move((move + velocity * Vector3.up) * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        // Horisontaalne pööramine (mängija keha)
        transform.Rotate(Vector3.up * mouseInput.x * mouseSensitivity);
        
        // Vertikaalne pööramine (kaamera)
        if (playerCamera != null)
        {
            float verticalRotation = -mouseInput.y * mouseSensitivity;
            float currentRotation = playerCamera.transform.localEulerAngles.x;
            
            // Normaliseeri nurk -180 kuni 180 vahele
            if (currentRotation > 180f)
                currentRotation -= 360f;
            
            float newRotation = Mathf.Clamp(currentRotation + verticalRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localEulerAngles = new Vector3(newRotation, 0f, 0f);
        }
    }
    
    private void HandleFlashlight()
    {
        // Taskulambi sisse/välja lülitamine
        if (flashlightToggleInput)
        {
            ToggleFlashlight();
        }
        
        // Patarei vähenemine
        if (isFlashlightOn && currentBattery > 0)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;
            
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOffFlashlight();
            }
        }
    }
    
    private void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        
        if (flashlight != null)
        {
            flashlight.SetActive(isFlashlightOn);
        }
        
        // Serverile teadaanne (kui vaja)
        ToggleFlashlightServerRpc(isFlashlightOn);
    }
    
    [ServerRpc]
    private void ToggleFlashlightServerRpc(bool isOn)
    {
        ToggleFlashlightClientRpc(isOn);
    }
    
    [ClientRpc]
    private void ToggleFlashlightClientRpc(bool isOn)
    {
        if (IsOwner) return; // Oma mängija juba tegi seda
        
        isFlashlightOn = isOn;
        if (flashlight != null)
        {
            flashlight.SetActive(isOn);
        }
    }
    
    // Public meetodid
    public float GetBatteryPercentage()
    {
        return currentBattery / batteryLife;
    }
    
    public bool IsFlashlightOn()
    {
        return isFlashlightOn;
    }
    
    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0f, batteryLife);
    }
}

