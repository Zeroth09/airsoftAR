using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace AirsoftARBattle.Networking
{
    /// <summary>
    /// Konfigurasi Photon untuk multiplayer networking
    /// </summary>
    public class PhotonConfig : MonoBehaviourPunCallbacks
    {
        [Header("Photon Settings")]
        [SerializeField] private string gameVersion = "1.0";
        [SerializeField] private string appId = "your-photon-app-id";
        
        [Header("Room Settings")]
        [SerializeField] private int maxPlayersPerRoom = 8;
        [SerializeField] private string roomName = "AirsoftARBattle";
        
        private void Start()
        {
            SetupPhoton();
        }
        
        /// <summary>
        /// Setup Photon networking
        /// </summary>
        private void SetupPhoton()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = gameVersion;
            
            // Connect to Photon
            PhotonNetwork.ConnectUsingSettings();
        }
        
        /// <summary>
        /// Join room
        /// </summary>
        public void JoinRoom()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }
        
        /// <summary>
        /// Create room
        /// </summary>
        public void CreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };
            
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        
        // Photon Callbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server");
        }
        
        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        }
        
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to join random room, creating new room");
            CreateRoom();
        }
    }
} 