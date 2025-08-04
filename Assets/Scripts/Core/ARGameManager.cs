using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager utama untuk game AR Airsoft Battle
    /// Mengelola semua sistem game termasuk AR, networking, dan game state
    /// </summary>
    public class ARGameManager : MonoBehaviourPunCallbacks
    {
        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        [SerializeField] private ARCameraManager arCameraManager;
        
        [Header("Game Settings")]
        [SerializeField] private GameMode currentGameMode = GameMode.Deathmatch;
        [SerializeField] private int maxPlayers = 8;
        [SerializeField] private float battleTime = 300f; // 5 menit
        
        [Header("UI References")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        
        // Game State
        private GameState currentState = GameState.MainMenu;
        private float currentBattleTime;
        private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<float> OnBattleTimeChanged;
        public System.Action<int, int> OnPlayerScoreChanged;
        
        public enum GameMode
        {
            Deathmatch,
            TeamBattle,
            CaptureTheFlag,
            Survival
        }
        
        public enum GameState
        {
            MainMenu,
            Connecting,
            InGame,
            Paused,
            GameOver
        }
        
        private void Start()
        {
            InitializeAR();
            SetupPhoton();
            ShowMainMenu();
        }
        
        private void Update()
        {
            if (currentState == GameState.InGame)
            {
                UpdateBattleTime();
            }
        }
        
        /// <summary>
        /// Inisialisasi sistem AR
        /// </summary>
        private void InitializeAR()
        {
            if (arSession == null)
            {
                arSession = FindObjectOfType<ARSession>();
            }
            
            if (arSessionOrigin == null)
            {
                arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            }
            
            if (arCameraManager == null)
            {
                arCameraManager = FindObjectOfType<ARCameraManager>();
            }
            
            // Setup AR permissions
            StartCoroutine(RequestARPermissions());
        }
        
        /// <summary>
        /// Request permission untuk AR
        /// </summary>
        private System.Collections.IEnumerator RequestARPermissions()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Camera);
            
            if (Application.HasUserAuthorization(UserAuthorization.Camera))
            {
                Debug.Log("Camera permission granted");
                arSession.enabled = true;
            }
            else
            {
                Debug.LogError("Camera permission denied");
            }
        }
        
        /// <summary>
        /// Setup Photon networking
        /// </summary>
        private void SetupPhoton()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.SendRate = 30;
            PhotonNetwork.SerializationRate = 30;
        }
        
        /// <summary>
        /// Mulai game baru
        /// </summary>
        public void StartNewGame()
        {
            ChangeGameState(GameState.Connecting);
            
            // Connect ke Photon server
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                JoinRandomRoom();
            }
        }
        
        /// <summary>
        /// Join room random untuk multiplayer
        /// </summary>
        private void JoinRandomRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = maxPlayers,
                IsVisible = true,
                IsOpen = true
            };
            
            PhotonNetwork.JoinRandomRoom(roomOptions);
        }
        
        /// <summary>
        /// Change game state
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;
                case GameState.InGame:
                    ShowGameUI();
                    StartBattle();
                    break;
                case GameState.Paused:
                    ShowPauseUI();
                    break;
                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }
        
        /// <summary>
        /// Update battle time
        /// </summary>
        private void UpdateBattleTime()
        {
            currentBattleTime -= Time.deltaTime;
            OnBattleTimeChanged?.Invoke(currentBattleTime);
            
            if (currentBattleTime <= 0)
            {
                EndBattle();
            }
        }
        
        /// <summary>
        /// Mulai battle
        /// </summary>
        private void StartBattle()
        {
            currentBattleTime = battleTime;
            players.Clear();
            
            // Spawn player
            if (PhotonNetwork.IsMessageQueueRunning)
            {
                SpawnPlayer();
            }
        }
        
        /// <summary>
        /// Spawn player di game
        /// </summary>
        private void SpawnPlayer()
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            PhotonNetwork.Instantiate("PlayerPrefab", spawnPosition, Quaternion.identity);
        }
        
        /// <summary>
        /// Dapatkan posisi spawn random
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            // Implementasi spawn position berdasarkan GPS atau AR plane
            return new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
        }
        
        /// <summary>
        /// End battle
        /// </summary>
        private void EndBattle()
        {
            ChangeGameState(GameState.GameOver);
        }
        
        /// <summary>
        /// Show main menu UI
        /// </summary>
        private void ShowMainMenu()
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(true);
            if (gameUI != null)
                gameUI.SetActive(false);
            if (pauseUI != null)
                pauseUI.SetActive(false);
        }
        
        /// <summary>
        /// Show game UI
        /// </summary>
        private void ShowGameUI()
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);
            if (gameUI != null)
                gameUI.SetActive(true);
            if (pauseUI != null)
                pauseUI.SetActive(false);
        }
        
        /// <summary>
        /// Show pause UI
        /// </summary>
        private void ShowPauseUI()
        {
            if (pauseUI != null)
                pauseUI.SetActive(true);
        }
        
        /// <summary>
        /// Show game over
        /// </summary>
        private void ShowGameOver()
        {
            // Implementasi game over UI
            Debug.Log("Game Over!");
        }
        
        /// <summary>
        /// Pause game
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.InGame)
            {
                ChangeGameState(GameState.Paused);
                Time.timeScale = 0;
            }
        }
        
        /// <summary>
        /// Resume game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeGameState(GameState.InGame);
                Time.timeScale = 1;
            }
        }
        
        /// <summary>
        /// Quit game
        /// </summary>
        public void QuitGame()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        // Photon Callbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server");
            JoinRandomRoom();
        }
        
        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
            ChangeGameState(GameState.InGame);
        }
        
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to join random room, creating new room");
            PhotonNetwork.CreateRoom(null);
        }
        
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Player {newPlayer.NickName} joined the room");
        }
        
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player {otherPlayer.NickName} left the room");
        }
    }
    
    /// <summary>
    /// Data player untuk tracking
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public int playerId;
        public string playerName;
        public int score;
        public int kills;
        public int deaths;
        public bool isAlive;
        public Vector3 position;
    }
} 