using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using AirsoftARBattle.Core;

namespace AirsoftARBattle.UI
{
    /// <summary>
    /// Manager untuk leaderboard dan statistics tracking
    /// Mengelola scoring, ranking, dan player statistics
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        [Header("Leaderboard UI")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private TextMeshProUGUI gameStatsTitle;
        
        [Header("Live Stats")]
        [SerializeField] private TextMeshProUGUI playerRankText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI playerKillsText;
        [SerializeField] private TextMeshProUGUI playerDeathsText;
        [SerializeField] private TextMeshProUGUI playerKDRText;
        
        [Header("End Game Stats")]
        [SerializeField] private GameObject endGameStatsPanel;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private Transform finalStatsContent;
        [SerializeField] private GameObject finalStatsEntryPrefab;
        
        [Header("Team Stats")]
        [SerializeField] private GameObject teamStatsPanel;
        [SerializeField] private TextMeshProUGUI redTeamScoreText;
        [SerializeField] private TextMeshProUGUI blueTeamScoreText;
        [SerializeField] private Transform redTeamContent;
        [SerializeField] private Transform blueTeamContent;
        
        [Header("Settings")]
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private bool showLiveLeaderboard = true;
        [SerializeField] private int maxLeaderboardEntries = 8;
        
        // Components
        private GameModeManager gameModeManager;
        private TeamManager teamManager;
        private AudioManager audioManager;
        
        // Leaderboard data
        private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
        private Dictionary<int, PlayerStats> playerStats = new Dictionary<int, PlayerStats>();
        private List<GameObject> leaderboardUIElements = new List<GameObject>();
        
        // Update tracking
        private float lastUpdateTime;
        private bool isLeaderboardVisible = false;
        
        [System.Serializable]
        public class PlayerStats
        {
            public string playerName;
            public int score;
            public int kills;
            public int deaths;
            public float kdr;
            public int rank;
            public bool isAlive;
            public TeamManager.Team team;
            public float playTime;
            public int assists;
            public int headshots;
            public float accuracy;
        }
        
        [System.Serializable]
        public class LeaderboardEntry
        {
            public Player player;
            public PlayerStats stats;
            public GameObject uiElement;
        }
        
        private void Start()
        {
            InitializeLeaderboard();
            SetupEventListeners();
        }
        
        private void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateLeaderboard();
                lastUpdateTime = Time.time;
            }
            
            // Toggle leaderboard dengan Tab key
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleLeaderboard();
            }
        }
        
        /// <summary>
        /// Initialize leaderboard
        /// </summary>
        private void InitializeLeaderboard()
        {
            gameModeManager = FindObjectOfType<GameModeManager>();
            teamManager = FindObjectOfType<TeamManager>();
            audioManager = AudioManager.Instance;
            
            // Hide panels initially
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            if (endGameStatsPanel != null) endGameStatsPanel.SetActive(false);
            if (teamStatsPanel != null) teamStatsPanel.SetActive(false);
            
            // Initialize player stats
            InitializePlayerStats();
            
            Debug.Log("Leaderboard Manager initialized");
        }
        
        /// <summary>
        /// Setup event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            if (gameModeManager != null)
            {
                gameModeManager.OnPlayerScoreChanged += OnPlayerScoreChanged;
                gameModeManager.OnPlayerKilled += OnPlayerKilled;
                gameModeManager.OnGameWon += OnGameWon;
                gameModeManager.OnTeamWon += OnTeamWon;
            }
            
            if (teamManager != null)
            {
                teamManager.OnPlayerJoinedTeam += OnPlayerJoinedTeam;
                teamManager.OnTeamScoreChanged += OnTeamScoreChanged;
            }
        }
        
        /// <summary>
        /// Initialize player stats untuk semua players
        /// </summary>
        private void InitializePlayerStats()
        {
            playerStats.Clear();
            
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                PlayerStats stats = new PlayerStats
                {
                    playerName = player.NickName,
                    score = 0,
                    kills = 0,
                    deaths = 0,
                    kdr = 0f,
                    rank = 0,
                    isAlive = true,
                    team = teamManager?.GetPlayerTeam(player) ?? TeamManager.Team.None,
                    playTime = 0f,
                    assists = 0,
                    headshots = 0,
                    accuracy = 0f
                };
                
                playerStats[player.ActorNumber] = stats;
            }
        }
        
        /// <summary>
        /// Update leaderboard
        /// </summary>
        private void UpdateLeaderboard()
        {
            UpdatePlayerStats();
            SortLeaderboard();
            
            if (isLeaderboardVisible)
            {
                UpdateLeaderboardUI();
            }
            
            UpdateLiveStats();
            UpdateTeamStats();
        }
        
        /// <summary>
        /// Update player stats
        /// </summary>
        private void UpdatePlayerStats()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (playerStats.ContainsKey(player.ActorNumber))
                {
                    PlayerStats stats = playerStats[player.ActorNumber];
                    
                    // Update from game mode manager
                    if (gameModeManager != null)
                    {
                        stats.score = gameModeManager.GetPlayerScore(player);
                        stats.kills = gameModeManager.GetPlayerKills(player);
                        stats.deaths = gameModeManager.GetPlayerDeaths(player);
                    }
                    
                    // Calculate KDR
                    stats.kdr = stats.deaths > 0 ? (float)stats.kills / stats.deaths : stats.kills;
                    
                    // Update team
                    stats.team = teamManager?.GetPlayerTeam(player) ?? TeamManager.Team.None;
                    
                    // Update play time
                    stats.playTime += Time.deltaTime;
                }
            }
        }
        
        /// <summary>
        /// Sort leaderboard berdasarkan score
        /// </summary>
        private void SortLeaderboard()
        {
            leaderboardEntries.Clear();
            
            var sortedPlayers = PhotonNetwork.PlayerList
                .Where(p => playerStats.ContainsKey(p.ActorNumber))
                .OrderByDescending(p => playerStats[p.ActorNumber].score)
                .ThenByDescending(p => playerStats[p.ActorNumber].kills)
                .ThenBy(p => playerStats[p.ActorNumber].deaths)
                .ToList();
            
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                Player player = sortedPlayers[i];
                PlayerStats stats = playerStats[player.ActorNumber];
                stats.rank = i + 1;
                
                LeaderboardEntry entry = new LeaderboardEntry
                {
                    player = player,
                    stats = stats
                };
                
                leaderboardEntries.Add(entry);
            }
        }
        
        /// <summary>
        /// Update leaderboard UI
        /// </summary>
        private void UpdateLeaderboardUI()
        {
            if (leaderboardContent == null || leaderboardEntryPrefab == null) return;
            
            // Clear existing entries
            ClearLeaderboardUI();
            
            // Create new entries
            int entriesToShow = Mathf.Min(leaderboardEntries.Count, maxLeaderboardEntries);
            for (int i = 0; i < entriesToShow; i++)
            {
                LeaderboardEntry entry = leaderboardEntries[i];
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                
                UpdateLeaderboardEntryUI(entryObj, entry);
                leaderboardUIElements.Add(entryObj);
            }
            
            // Update title
            if (gameStatsTitle != null)
            {
                string mode = gameModeManager?.GetCurrentGameMode().ToString() ?? "Game";
                gameStatsTitle.text = $"{mode} - Leaderboard";
            }
        }
        
        /// <summary>
        /// Update leaderboard entry UI
        /// </summary>
        private void UpdateLeaderboardEntryUI(GameObject entryObj, LeaderboardEntry entry)
        {
            // Find UI components
            TextMeshProUGUI rankText = entryObj.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entryObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI killsText = entryObj.transform.Find("KillsText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI deathsText = entryObj.transform.Find("DeathsText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI kdrText = entryObj.transform.Find("KDRText")?.GetComponent<TextMeshProUGUI>();
            Image teamColor = entryObj.transform.Find("TeamColor")?.GetComponent<Image>();
            
            // Update texts
            if (rankText != null) rankText.text = entry.stats.rank.ToString();
            if (nameText != null) nameText.text = entry.stats.playerName;
            if (scoreText != null) scoreText.text = entry.stats.score.ToString();
            if (killsText != null) killsText.text = entry.stats.kills.ToString();
            if (deathsText != null) deathsText.text = entry.stats.deaths.ToString();
            if (kdrText != null) kdrText.text = entry.stats.kdr.ToString("F2");
            
            // Update team color
            if (teamColor != null && teamManager != null)
            {
                teamColor.color = teamManager.GetTeamColor(entry.stats.team);
            }
            
            // Highlight local player
            if (entry.player == PhotonNetwork.LocalPlayer)
            {
                entryObj.GetComponent<Image>()?.color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
        
        /// <summary>
        /// Clear leaderboard UI
        /// </summary>
        private void ClearLeaderboardUI()
        {
            foreach (GameObject obj in leaderboardUIElements)
            {
                if (obj != null) Destroy(obj);
            }
            leaderboardUIElements.Clear();
        }
        
        /// <summary>
        /// Update live stats untuk local player
        /// </summary>
        private void UpdateLiveStats()
        {
            Player localPlayer = PhotonNetwork.LocalPlayer;
            if (localPlayer == null || !playerStats.ContainsKey(localPlayer.ActorNumber)) return;
            
            PlayerStats stats = playerStats[localPlayer.ActorNumber];
            
            if (playerRankText != null) playerRankText.text = $"#{stats.rank}";
            if (playerScoreText != null) playerScoreText.text = stats.score.ToString();
            if (playerKillsText != null) playerKillsText.text = stats.kills.ToString();
            if (playerDeathsText != null) playerDeathsText.text = stats.deaths.ToString();
            if (playerKDRText != null) playerKDRText.text = stats.kdr.ToString("F2");
        }
        
        /// <summary>
        /// Update team stats
        /// </summary>
        private void UpdateTeamStats()
        {
            if (teamManager == null) return;
            
            var gameMode = gameModeManager?.GetCurrentGameMode();
            bool isTeamMode = gameMode == GameModeManager.GameMode.TeamBattle || 
                             gameMode == GameModeManager.GameMode.CaptureTheFlag;
            
            if (teamStatsPanel != null)
            {
                teamStatsPanel.SetActive(isTeamMode);
            }
            
            if (!isTeamMode) return;
            
            // Update team scores
            if (redTeamScoreText != null)
            {
                redTeamScoreText.text = teamManager.GetTeamScore(TeamManager.Team.Red).ToString();
            }
            
            if (blueTeamScoreText != null)
            {
                blueTeamScoreText.text = teamManager.GetTeamScore(TeamManager.Team.Blue).ToString();
            }
        }
        
        /// <summary>
        /// Toggle leaderboard visibility
        /// </summary>
        public void ToggleLeaderboard()
        {
            isLeaderboardVisible = !isLeaderboardVisible;
            
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(isLeaderboardVisible);
            }
            
            if (isLeaderboardVisible)
            {
                UpdateLeaderboardUI();
                
                if (audioManager != null)
                {
                    audioManager.PlayPanelOpen();
                }
            }
            else
            {
                if (audioManager != null)
                {
                    audioManager.PlayPanelClose();
                }
            }
        }
        
        /// <summary>
        /// Show end game stats
        /// </summary>
        public void ShowEndGameStats(Player winner = null, TeamManager.Team winningTeam = TeamManager.Team.None)
        {
            if (endGameStatsPanel != null)
            {
                endGameStatsPanel.SetActive(true);
            }
            
            // Update winner text
            if (winnerText != null)
            {
                if (winner != null)
                {
                    winnerText.text = $"🏆 {winner.NickName} Wins!";
                }
                else if (winningTeam != TeamManager.Team.None)
                {
                    winnerText.text = $"🏆 {winningTeam} Team Wins!";
                }
                else
                {
                    winnerText.text = "Game Over - Draw!";
                }
            }
            
            // Show final leaderboard
            UpdateFinalStats();
        }
        
        /// <summary>
        /// Update final stats
        /// </summary>
        private void UpdateFinalStats()
        {
            if (finalStatsContent == null || finalStatsEntryPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in finalStatsContent)
            {
                Destroy(child.gameObject);
            }
            
            // Create final stats entries
            foreach (LeaderboardEntry entry in leaderboardEntries)
            {
                GameObject entryObj = Instantiate(finalStatsEntryPrefab, finalStatsContent);
                UpdateLeaderboardEntryUI(entryObj, entry);
            }
        }
        
        // Event handlers
        private void OnPlayerScoreChanged(Player player, int newScore)
        {
            if (playerStats.ContainsKey(player.ActorNumber))
            {
                playerStats[player.ActorNumber].score = newScore;
            }
        }
        
        private void OnPlayerKilled(Player victim)
        {
            if (playerStats.ContainsKey(victim.ActorNumber))
            {
                playerStats[victim.ActorNumber].isAlive = false;
            }
        }
        
        private void OnGameWon(Player winner)
        {
            ShowEndGameStats(winner);
        }
        
        private void OnTeamWon(TeamManager.Team winningTeam)
        {
            ShowEndGameStats(null, winningTeam);
        }
        
        private void OnPlayerJoinedTeam(Player player, TeamManager.Team team)
        {
            if (playerStats.ContainsKey(player.ActorNumber))
            {
                playerStats[player.ActorNumber].team = team;
            }
        }
        
        private void OnTeamScoreChanged(TeamManager.Team team, int newScore)
        {
            // Team score updated in UpdateTeamStats
        }
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public PlayerStats GetPlayerStats(Player player)
        {
            return playerStats.ContainsKey(player.ActorNumber) ? playerStats[player.ActorNumber] : null;
        }
        
        /// <summary>
        /// Get current leaderboard
        /// </summary>
        public List<LeaderboardEntry> GetLeaderboard()
        {
            return new List<LeaderboardEntry>(leaderboardEntries);
        }
        
        private void OnDestroy()
        {
            // Cleanup event listeners
            if (gameModeManager != null)
            {
                gameModeManager.OnPlayerScoreChanged -= OnPlayerScoreChanged;
                gameModeManager.OnPlayerKilled -= OnPlayerKilled;
                gameModeManager.OnGameWon -= OnGameWon;
                gameModeManager.OnTeamWon -= OnTeamWon;
            }
            
            if (teamManager != null)
            {
                teamManager.OnPlayerJoinedTeam -= OnPlayerJoinedTeam;
                teamManager.OnTeamScoreChanged -= OnTeamScoreChanged;
            }
        }
    }
}