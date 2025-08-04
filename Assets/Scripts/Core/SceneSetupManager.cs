using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Photon.Pun;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk setup scene dengan AR components
    /// Mengatur konfigurasi optimal untuk AR gameplay
    /// </summary>
    public class SceneSetupManager : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private bool autoSetupScene = true;
        [SerializeField] private bool setupARCamera = true;
        [SerializeField] private bool setupManagers = true;
        [SerializeField] private bool setupUI = true;
        
        [Header("AR Components")]
        [SerializeField] private GameObject arSessionPrefab;
        [SerializeField] private GameObject arCameraPrefab;
        [SerializeField] private GameObject arSessionOriginPrefab;
        
        [Header("Manager Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject teamManagerPrefab;
        [SerializeField] private GameObject gameModeManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject gpsManagerPrefab;
        [SerializeField] private GameObject arPlaneManagerPrefab;
        
        [Header("UI Prefabs")]
        [SerializeField] private GameObject gameUIPrefab;
        [SerializeField] private GameObject leaderboardManagerPrefab;
        
        [Header("Player Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        
        [Header("Environment")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Camera fallbackCamera;
        
        // Scene components
        private ARSession arSession;
        private ARSessionOrigin arSessionOrigin;
        private ARCamera arCamera;
        private ARPlaneManager arPlaneManager;
        private ARRaycastManager raycastManager;
        
        // Game managers
        private ARGameManager gameManager;
        private TeamManager teamManager;
        private GameModeManager gameModeManager;
        private AudioManager audioManager;
        private GPSLocationManager gpsManager;
        private ARPlaneManager customArPlaneManager;
        
        private void Awake()
        {
            if (autoSetupScene)
            {
                SetupScene();
            }
        }
        
        /// <summary>
        /// Setup complete scene
        /// </summary>
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("Setting up AR Airsoft Battle scene...");
            
            if (setupARCamera)
            {
                SetupARComponents();
            }
            
            if (setupManagers)
            {
                SetupGameManagers();
            }
            
            if (setupUI)
            {
                SetupUIComponents();
            }
            
            SetupEnvironment();
            ConfigureQualitySettings();
            
            Debug.Log("Scene setup completed!");
        }
        
        /// <summary>
        /// Setup AR components
        /// </summary>
        private void SetupARComponents()
        {
            Debug.Log("Setting up AR components...");
            
            // Create AR Session
            if (FindObjectOfType<ARSession>() == null)
            {
                GameObject arSessionObj;
                if (arSessionPrefab != null)
                {
                    arSessionObj = Instantiate(arSessionPrefab);
                }
                else
                {
                    arSessionObj = new GameObject("AR Session");
                    arSessionObj.AddComponent<ARSession>();
                }
                arSession = arSessionObj.GetComponent<ARSession>();
            }
            
            // Create AR Session Origin
            if (FindObjectOfType<ARSessionOrigin>() == null)
            {
                GameObject arOriginObj;
                if (arSessionOriginPrefab != null)
                {
                    arOriginObj = Instantiate(arSessionOriginPrefab);
                }
                else
                {
                    arOriginObj = CreateARSessionOrigin();
                }
                arSessionOrigin = arOriginObj.GetComponent<ARSessionOrigin>();
            }
            
            // Configure AR managers
            ConfigureARManagers();
            
            // Disable fallback camera jika ada
            if (fallbackCamera != null)
            {
                fallbackCamera.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Create AR Session Origin dengan semua components
        /// </summary>
        private GameObject CreateARSessionOrigin()
        {
            GameObject arOriginObj = new GameObject("AR Session Origin");
            
            // Add AR Session Origin
            ARSessionOrigin sessionOrigin = arOriginObj.AddComponent<ARSessionOrigin>();
            
            // Create AR Camera
            GameObject arCameraObj = new GameObject("AR Camera");
            arCameraObj.transform.SetParent(arOriginObj.transform);
            
            Camera camera = arCameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            
            ARCameraManager cameraManager = arCameraObj.AddComponent<ARCameraManager>();
            ARCameraBackground cameraBackground = arCameraObj.AddComponent<ARCameraBackground>();
            arCamera = arCameraObj.AddComponent<ARCamera>();
            
            sessionOrigin.camera = arCamera;
            
            // Add AR managers
            arPlaneManager = arOriginObj.AddComponent<ARPlaneManager>();
            raycastManager = arOriginObj.AddComponent<ARRaycastManager>();
            
            // Add other AR managers
            arOriginObj.AddComponent<ARPointCloudManager>();
            arOriginObj.AddComponent<ARAnchorManager>();
            
            return arOriginObj;
        }
        
        /// <summary>
        /// Configure AR managers
        /// </summary>
        private void ConfigureARManagers()
        {
            // Configure AR Plane Manager
            if (arPlaneManager != null)
            {
                arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                arPlaneManager.enabled = true;
            }
            
            // Configure AR Raycast Manager
            if (raycastManager != null)
            {
                raycastManager.enabled = true;
            }
        }
        
        /// <summary>
        /// Setup game managers
        /// </summary>
        private void SetupGameManagers()
        {
            Debug.Log("Setting up game managers...");
            
            // AR Game Manager
            if (FindObjectOfType<ARGameManager>() == null)
            {
                GameObject gameManagerObj;
                if (gameManagerPrefab != null)
                {
                    gameManagerObj = Instantiate(gameManagerPrefab);
                }
                else
                {
                    gameManagerObj = new GameObject("AR Game Manager");
                    gameManagerObj.AddComponent<ARGameManager>();
                }
                gameManager = gameManagerObj.GetComponent<ARGameManager>();
            }
            
            // Team Manager
            if (FindObjectOfType<TeamManager>() == null)
            {
                GameObject teamManagerObj;
                if (teamManagerPrefab != null)
                {
                    teamManagerObj = Instantiate(teamManagerPrefab);
                }
                else
                {
                    teamManagerObj = new GameObject("Team Manager");
                    teamManagerObj.AddComponent<TeamManager>();
                    teamManagerObj.AddComponent<PhotonView>(); // For networking
                }
                teamManager = teamManagerObj.GetComponent<TeamManager>();
            }
            
            // Game Mode Manager
            if (FindObjectOfType<GameModeManager>() == null)
            {
                GameObject gameModeObj;
                if (gameModeManagerPrefab != null)
                {
                    gameModeObj = Instantiate(gameModeManagerPrefab);
                }
                else
                {
                    gameModeObj = new GameObject("Game Mode Manager");
                    gameModeObj.AddComponent<GameModeManager>();
                }
                gameModeManager = gameModeObj.GetComponent<GameModeManager>();
            }
            
            // Audio Manager
            if (FindObjectOfType<AudioManager>() == null)
            {
                GameObject audioObj;
                if (audioManagerPrefab != null)
                {
                    audioObj = Instantiate(audioManagerPrefab);
                }
                else
                {
                    audioObj = new GameObject("Audio Manager");
                    audioObj.AddComponent<AudioManager>();
                }
                audioManager = audioObj.GetComponent<AudioManager>();
            }
            
            // GPS Manager
            if (FindObjectOfType<GPSLocationManager>() == null)
            {
                GameObject gpsObj;
                if (gpsManagerPrefab != null)
                {
                    gpsObj = Instantiate(gpsManagerPrefab);
                }
                else
                {
                    gpsObj = new GameObject("GPS Location Manager");
                    gpsObj.AddComponent<GPSLocationManager>();
                }
                gpsManager = gpsObj.GetComponent<GPSLocationManager>();
            }
            
            // Custom AR Plane Manager
            if (FindObjectOfType<AirsoftARBattle.Core.ARPlaneManager>() == null)
            {
                GameObject arPlaneObj;
                if (arPlaneManagerPrefab != null)
                {
                    arPlaneObj = Instantiate(arPlaneManagerPrefab);
                }
                else
                {
                    arPlaneObj = new GameObject("AR Plane Manager");
                    arPlaneObj.AddComponent<AirsoftARBattle.Core.ARPlaneManager>();
                }
                customArPlaneManager = arPlaneObj.GetComponent<AirsoftARBattle.Core.ARPlaneManager>();
            }
        }
        
        /// <summary>
        /// Setup UI components
        /// </summary>
        private void SetupUIComponents()
        {
            Debug.Log("Setting up UI components...");
            
            // Create Canvas jika belum ada
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("Main Canvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Game UI
            if (gameUIPrefab != null && FindObjectOfType<AirsoftARBattle.UI.GameUI>() == null)
            {
                Instantiate(gameUIPrefab, mainCanvas.transform);
            }
            
            // Leaderboard Manager
            if (leaderboardManagerPrefab != null && FindObjectOfType<AirsoftARBattle.UI.LeaderboardManager>() == null)
            {
                Instantiate(leaderboardManagerPrefab, mainCanvas.transform);
            }
            
            // Event System jika belum ada
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        /// <summary>
        /// Setup environment
        /// </summary>
        private void SetupEnvironment()
        {
            Debug.Log("Setting up environment...");
            
            // Configure lighting
            if (directionalLight == null)
            {
                directionalLight = FindObjectOfType<Light>();
            }
            
            if (directionalLight != null)
            {
                directionalLight.type = LightType.Directional;
                directionalLight.intensity = 1f;
                directionalLight.shadows = LightShadows.Soft;
            }
            
            // Set render settings
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
        }
        
        /// <summary>
        /// Configure quality settings optimal untuk AR
        /// </summary>
        private void ConfigureQualitySettings()
        {
            Debug.Log("Configuring quality settings for AR...");
            
            // Set target frame rate
            Application.targetFrameRate = 60;
            
            // Quality settings untuk mobile AR
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowDistance = 50f;
            QualitySettings.shadows = ShadowQuality.All;
            
            // Physics settings
            Physics.defaultSolverIterations = 6;
            Physics.defaultSolverVelocityIterations = 1;
            
            // AR specific settings
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        
        /// <summary>
        /// Create player prefab di scene
        /// </summary>
        public void CreatePlayerPrefab()
        {
            if (playerPrefab != null)
            {
                Debug.Log("Creating player prefab...");
                
                GameObject player = Instantiate(playerPrefab);
                player.name = "Player";
                
                // Add PhotonView untuk networking
                if (player.GetComponent<PhotonView>() == null)
                {
                    PhotonView pv = player.AddComponent<PhotonView>();
                    pv.observeUsingOnSerializePhotonView = true;
                    pv.ObservedComponents.Add(player.GetComponent<AirsoftARBattle.Player.PlayerController>());
                }
            }
        }
        
        /// <summary>
        /// Validate scene setup
        /// </summary>
        [ContextMenu("Validate Scene Setup")]
        public void ValidateSceneSetup()
        {
            Debug.Log("Validating scene setup...");
            
            bool isValid = true;
            
            // Check AR components
            if (FindObjectOfType<ARSession>() == null)
            {
                Debug.LogError("Missing ARSession");
                isValid = false;
            }
            
            if (FindObjectOfType<ARSessionOrigin>() == null)
            {
                Debug.LogError("Missing ARSessionOrigin");
                isValid = false;
            }
            
            if (FindObjectOfType<ARCamera>() == null)
            {
                Debug.LogError("Missing ARCamera");
                isValid = false;
            }
            
            // Check managers
            if (FindObjectOfType<ARGameManager>() == null)
            {
                Debug.LogError("Missing ARGameManager");
                isValid = false;
            }
            
            if (FindObjectOfType<TeamManager>() == null)
            {
                Debug.LogWarning("Missing TeamManager - required for team modes");
            }
            
            if (FindObjectOfType<AudioManager>() == null)
            {
                Debug.LogWarning("Missing AudioManager - no audio will play");
            }
            
            // Check UI
            if (FindObjectOfType<Canvas>() == null)
            {
                Debug.LogError("Missing Canvas for UI");
                isValid = false;
            }
            
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.LogError("Missing EventSystem for UI interaction");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("✅ Scene setup is valid!");
            }
            else
            {
                Debug.LogError("❌ Scene setup has issues. Please fix them before playing.");
            }
        }
        
        /// <summary>
        /// Quick setup untuk testing
        /// </summary>
        [ContextMenu("Quick Test Setup")]
        public void QuickTestSetup()
        {
            SetupScene();
            CreatePlayerPrefab();
            ValidateSceneSetup();
        }
        
        /// <summary>
        /// Clear scene setup
        /// </summary>
        [ContextMenu("Clear Scene Setup")]
        public void ClearSceneSetup()
        {
            // Remove AR components
            DestroyComponentOfType<ARSession>();
            DestroyComponentOfType<ARSessionOrigin>();
            
            // Remove managers
            DestroyComponentOfType<ARGameManager>();
            DestroyComponentOfType<TeamManager>();
            DestroyComponentOfType<GameModeManager>();
            DestroyComponentOfType<AudioManager>();
            DestroyComponentOfType<GPSLocationManager>();
            DestroyComponentOfType<AirsoftARBattle.Core.ARPlaneManager>();
            
            Debug.Log("Scene setup cleared");
        }
        
        /// <summary>
        /// Helper method untuk destroy component
        /// </summary>
        private void DestroyComponentOfType<T>() where T : Component
        {
            T component = FindObjectOfType<T>();
            if (component != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(component.gameObject);
                }
                else
                {
                    DestroyImmediate(component.gameObject);
                }
            }
        }
        
        private void OnValidate()
        {
            // Auto validate saat inspector berubah
            if (Application.isPlaying)
            {
                ValidateSceneSetup();
            }
        }
    }
}