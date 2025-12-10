using UnityEngine;

/// <summary>
/// Lihtne mängija kontroller WASD liikumise ja hiire vaatamisega
/// Töötab kohe ilma Netcode'i või muude eeltingimusteta
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Liikumise Seaded")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Hiire Seaded")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Komponendid")]
    [Tooltip("Mängija kaamera. Jäta tühjaks, et kasutada automaatset otsingut.")]
    [SerializeField] private Camera playerCamera = null;
    [Tooltip("Kaamera Transform. Tavaliselt pole vaja täita.")]
    [SerializeField] private Transform cameraTransform = null; // Kui kaamera on eraldi objekt (mittekohustuslik)
    
    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Otsi kaamera, kui seda pole määratud
        if (playerCamera == null)
        {
            // Proovi leida Camera lastes (Player'i all)
            playerCamera = GetComponentInChildren<Camera>();
            
            // Kui ei leia, proovi leida Scene'ist MainCamera
            if (playerCamera == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null && mainCam.transform.IsChildOf(transform))
                {
                    playerCamera = mainCam;
                }
            }
        }
        
        // Kui kaamera on määratud, aga cameraTransform mitte
        if (cameraTransform == null && playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }
        
        // Kui kaamera puudub, anna viga
        if (playerCamera == null)
        {
            Debug.LogWarning("SimplePlayerController: Player Camera puudub! Lisa Camera GameObject Player'i alla (nt. 'Cameraholder' GameObject Camera komponendiga).");
        }
        else
        {
            Debug.Log($"SimplePlayerController: Camera leitud: {playerCamera.name}");
        }
        
        // Lukusta kursor ekraani keskele
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Kontrolli, et controller on olemas
        if (controller == null) return;
        
        HandleMouseLook();
        HandleMovement();
        
        // ESC klahv vabastab kursori
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Vajutades hiire nuppu, lukusta kursor uuesti
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void HandleMouseLook()
    {
        // Hiire liikumine
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Horisontaalne pööramine (mängija keha)
        transform.Rotate(Vector3.up * mouseX);
        
        // Vertikaalne pööramine (kaamera üles/alla) - ainult kui kaamera on olemas
        if (playerCamera != null || cameraTransform != null)
        {
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            
            // Pööra kaamera vertikaalselt
            if (cameraTransform != null)
            {
                cameraTransform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
            }
            else if (playerCamera != null)
            {
                // Proovi kasutada playerCamera transform'i otse
                playerCamera.transform.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
            }
        }
    }
    
    private void HandleMovement()
    {
        // Kontrolli, kas mängija on maas
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Väike allavajutus, et kindlaks teha, et on maas
        }
        
        // WASD liikumine
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        
        // Liikumise suund mängija vaate järgi
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = move.normalized;
        
        // Jooks
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        move *= currentSpeed;
        
        // Hüppamine
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Gravitatsioon
        velocity.y += gravity * Time.deltaTime;
        
        // Liiguta mängijat
        controller.Move((move + velocity * Vector3.up) * Time.deltaTime);
    }
    
    // Public meetodid, kui vaja ligipääsu muudele skriptidele
    public float GetCurrentSpeed()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        return isRunning ? runSpeed : walkSpeed;
    }
    
    public bool IsGrounded()
    {
        return controller.isGrounded;
    }
}

