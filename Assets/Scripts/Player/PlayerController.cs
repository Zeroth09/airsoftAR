using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Photon.Pun;
using Photon.Realtime;
using AirsoftARBattle.Core;
using AirsoftARBattle.Weapons;

namespace AirsoftARBattle.Player
{
    /// <summary>
    /// Controller utama untuk player dengan AR integration
    /// Mengelola movement, camera, dan positioning
    /// </summary>
    public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private bool allowMovement = true;
        
        [Header("AR Components")]
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        [SerializeField] private ARCamera arCamera;
        [SerializeField] private ARRaycastManager raycastManager;
        
        [Header("Player Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerModel;
        [SerializeField] private Transform cameraRig;
        
        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private bool invertMouseY = false;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        
        // Movement variables
        private Vector3 moveDirection;
        private bool isRunning = false;
        private float currentSpeed;
        private Vector3 velocity;
        
        // Camera variables
        private float mouseX;
        private float mouseY;
        private float rotationX = 0f;
        
        // Network variables
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        
        // Components
        private PlayerHealth playerHealth;
        private WeaponController weaponController;
        private GPSLocationManager gpsManager;
        private ARGameManager gameManager;
        
        // GPS positioning
        private Vector2 lastGPSPosition;
        private bool isGPSPositioned = false;
        
        // Events
        public System.Action<Vector3> OnPlayerMoved;
        public System.Action<bool> OnRunStateChanged;
        
        private void Start()
        {
            InitializePlayer();
            SetupComponents();
            SetupARCamera();
        }
        
        private void Update()
        {
            if (photonView.IsMine)
            {
                HandleInput();
                HandleMovement();
                HandleCameraRotation();
                UpdateGPSPosition();
            }
            else
            {
                // Network interpolation
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
        }
        
        /// <summary>
        /// Initialize player
        /// </summary>
        private void InitializePlayer()
        {
            // Lock cursor untuk FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Setup character controller
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
                if (characterController == null)
                {
                    characterController = gameObject.AddComponent<CharacterController>();
                }
            }
            
            currentSpeed = moveSpeed;
        }
        
        /// <summary>
        /// Setup components
        /// </summary>
        private void SetupComponents()
        {
            playerHealth = GetComponent<PlayerHealth>();
            weaponController = GetComponentInChildren<WeaponController>();
            gpsManager = FindObjectOfType<GPSLocationManager>();
            gameManager = FindObjectOfType<ARGameManager>();
            
            // Setup GPS events
            if (gpsManager != null)
            {
                gpsManager.OnPlayerPositionUpdated += OnGPSPositionUpdated;
                gpsManager.OnBattleAreaStatusChanged += OnBattleAreaChanged;
            }
        }
        
        /// <summary>
        /// Setup AR camera
        /// </summary>
        private void SetupARCamera()
        {
            if (arSessionOrigin == null)
            {
                arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            }
            
            if (arCamera == null)
            {
                arCamera = FindObjectOfType<ARCamera>();
            }
            
            if (raycastManager == null)
            {
                raycastManager = FindObjectOfType<ARRaycastManager>();
            }
            
            // Position AR camera pada player
            if (arSessionOrigin != null && cameraRig != null)
            {
                arSessionOrigin.transform.SetParent(cameraRig);
                arSessionOrigin.transform.localPosition = Vector3.zero;
                arSessionOrigin.transform.localRotation = Quaternion.identity;
            }
        }
        
        /// <summary>
        /// Handle player input
        /// </summary>
        private void HandleInput()
        {
            if (!allowMovement) return;
            
            // Movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            moveDirection = new Vector3(horizontal, 0, vertical).normalized;
            
            // Running
            isRunning = Input.GetKey(runKey);
            currentSpeed = isRunning ? runSpeed : moveSpeed;
            
            // Mouse input
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Pause input
            if (Input.GetKeyDown(pauseKey))
            {
                if (gameManager != null)
                {
                    gameManager.PauseGame();
                }
            }
        }
        
        /// <summary>
        /// Handle player movement
        /// </summary>
        private void HandleMovement()
        {
            if (!allowMovement || characterController == null) return;
            
            // Transform movement direction ke world space
            Vector3 move = transform.TransformDirection(moveDirection) * currentSpeed;
            
            // Apply gravity
            if (!characterController.isGrounded)
            {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                velocity.y = -2f; // Small downward force
            }
            
            move.y = velocity.y;
            
            // Move player
            characterController.Move(move * Time.deltaTime);
            
            // Trigger events
            if (moveDirection.magnitude > 0.1f)
            {
                OnPlayerMoved?.Invoke(transform.position);
            }
            
            if (isRunning != (currentSpeed == runSpeed))
            {
                OnRunStateChanged?.Invoke(isRunning);
            }
        }
        
        /// <summary>
        /// Handle camera rotation
        /// </summary>
        private void HandleCameraRotation()
        {
            // Horizontal rotation (Y-axis)
            transform.Rotate(Vector3.up * mouseX);
            
            // Vertical rotation (X-axis)
            rotationX -= invertMouseY ? -mouseY : mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);
            
            if (cameraRig != null)
            {
                cameraRig.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }
        }
        
        /// <summary>
        /// Update GPS position
        /// </summary>
        private void UpdateGPSPosition()
        {
            if (gpsManager != null && gpsManager.IsLocationReady())
            {
                Vector2 currentGPSPos = gpsManager.GetLocalPosition();
                
                if (!isGPSPositioned)
                {
                    // First GPS position - set player position
                    Vector3 gpsWorldPos = new Vector3(currentGPSPos.x, transform.position.y, currentGPSPos.y);
                    transform.position = gpsWorldPos;
                    isGPSPositioned = true;
                    Debug.Log($"Player positioned via GPS: {gpsWorldPos}");
                }
                
                lastGPSPosition = currentGPSPos;
            }
        }
        
        /// <summary>
        /// GPS position updated callback
        /// </summary>
        private void OnGPSPositionUpdated(Vector2 position)
        {
            if (photonView.IsMine)
            {
                // Update network position based on GPS
                Vector3 newPosition = new Vector3(position.x, transform.position.y, position.y);
                
                // Smooth GPS updates untuk avoid jitter
                transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 2f);
            }
        }
        
        /// <summary>
        /// Battle area status changed callback
        /// </summary>
        private void OnBattleAreaChanged(bool isInArea)
        {
            if (!isInArea)
            {
                // Player keluar dari battle area
                Debug.LogWarning("Player is outside battle area!");
                // Implementasi warning UI atau force return ke area
            }
        }
        
        /// <summary>
        /// Enable/disable movement
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            allowMovement = enabled;
            
            if (!enabled)
            {
                moveDirection = Vector3.zero;
                velocity = Vector3.zero;
            }
        }
        
        /// <summary>
        /// Set mouse sensitivity
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        /// <summary>
        /// Get current movement speed
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        
        /// <summary>
        /// Check if player is moving
        /// </summary>
        public bool IsMoving()
        {
            return moveDirection.magnitude > 0.1f;
        }
        
        /// <summary>
        /// Check if player is running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }
        
        /// <summary>
        /// Get GPS position
        /// </summary>
        public Vector2 GetGPSPosition()
        {
            return lastGPSPosition;
        }
        
        /// <summary>
        /// Photon network data synchronization
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send data
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(isRunning);
                stream.SendNext(lastGPSPosition);
            }
            else
            {
                // Receive data
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                isRunning = (bool)stream.ReceiveNext();
                lastGPSPosition = (Vector2)stream.ReceiveNext();
            }
        }
        
        /// <summary>
        /// Cleanup saat destroy
        /// </summary>
        private void OnDestroy()
        {
            if (gpsManager != null)
            {
                gpsManager.OnPlayerPositionUpdated -= OnGPSPositionUpdated;
                gpsManager.OnBattleAreaStatusChanged -= OnBattleAreaChanged;
            }
            
            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}