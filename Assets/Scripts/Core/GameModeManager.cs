using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using AirsoftARBattle.Core;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk berbagai game modes
    /// Mengelola Deathmatch, Team Battle, Capture The Flag, dan Survival
    /// </summary>
    public class GameModeManager : MonoBehaviourPunCallbacks
    {
        [Header("Game Mode Settings")]
        [SerializeField] private GameMode currentGameMode = GameMode.Deathmatch;
        [SerializeField] private int scoreToWin = 10;
        [SerializeField] private float gameTime = 300f; // 5 menit
        [SerializeField] private int maxPlayers = 8;
        
        [Header("Deathmatch Settings")]
        [SerializeField] private int killScore = 1;
        [SerializeField] private int deathPenalty = 0;
        
        [Header("Team Battle Settings")]
        [SerializeField] private int teamKillScore = 1;
        [SerializeField] private int teamScoreToWin = 20;
        
        [Header("Capture The Flag Settings")]
        [SerializeField] private GameObject redFlag;
        [SerializeField] private GameObject blueFlag;
        [SerializeField] private Transform redFlagSpawn;
        [SerializeField] private Transform blueFlagSpawn;
        [SerializeField] private int flagCaptureScore = 3;
        [SerializeField] private int flagsToWin = 3;
        
        [Header("Survival Settings")]
        [SerializeField] private float survivalTime = 180f; // 3 menit
        [SerializeField] private int eliminations = 1; // Lives per player
        
        // Components
        private TeamManager teamManager;
        private AudioManager audioManager;
        private ARGameManager gameManager;
        
        // Game state
        private bool isGameActive = false;
        private float currentGameTime;
        private Dictionary<int, int> playerScores = new Dictionary<int, int>();
        private Dictionary<int, int> playerKills = new Dictionary<int, int>();
        private Dictionary<int, int> playerDeaths = new Dictionary<int, int>();
        private Dictionary<int, bool> playerAlive = new Dictionary<int, bool>();
        
        // CTF specific
        private bool redFlagTaken = false;
        private bool blueFlagTaken = false;
        private Player redFlagCarrier = null;
        private Player blueFlagCarrier = null;
        private int redFlagCaptures = 0;
        private int blueFlagCaptures = 0;
        
        // Survival specific
        private List<Player> alivePlayers = new List<Player>();
        private Dictionary<int, int> playerLives = new Dictionary<int, int>();
        
        public enum GameMode
        {
            Deathmatch,
            TeamBattle,
            CaptureTheFlag,
            Survival
        }
        
        // Events
        public System.Action<GameMode> OnGameModeChanged;
        public System.Action<Player, int> OnPlayerScoreChanged;
        public System.Action<Player> OnPlayerKilled;
        public System.Action<Player> OnPlayerEliminated;
        public System.Action<Player> OnGameWon;
        public System.Action<TeamManager.Team> OnTeamWon;
        public System.Action<Player, bool> OnFlagTaken; // Player, isRedFlag
        public System.Action<TeamManager.Team> OnFlagCaptured;
        
        private void Start()
        {
            InitializeGameModeManager();
        }
        
        private void Update()
        {
            if (isGameActive)
            {
                UpdateGameTime();
                CheckWinConditions();
            }
        }
        
        /// <summary>
        /// Initialize game mode manager
        /// </summary>
        private void InitializeGameModeManager()
        {
            teamManager = FindObjectOfType<TeamManager>();
            audioManager = AudioManager.Instance;
            gameManager = FindObjectOfType<ARGameManager>();
            
            // Setup flags for CTF
            SetupCTFFlags();
            
            Debug.Log($"Game Mode Manager initialized: {currentGameMode}");
        }
        
        /// <summary>
        /// Setup CTF flags
        /// </summary>
        private void SetupCTFFlags()
        {
            if (redFlag != null && redFlagSpawn != null)
            {
                redFlag.transform.position = redFlagSpawn.position;
                redFlag.SetActive(false);
            }
            
            if (blueFlag != null && blueFlagSpawn != null)
            {
                blueFlag.transform.position = blueFlagSpawn.position;
                blueFlag.SetActive(false);
            }
        }
        
        /// <summary>
        /// Start game dengan mode tertentu
        /// </summary>
        public void StartGame(GameMode mode)
        {
            currentGameMode = mode;
            isGameActive = true;
            
            // Reset game state
            ResetGameState();
            
            // Setup mode specific
            switch (mode)
            {
                case GameMode.Deathmatch:
                    StartDeathmatch();
                    break;
                case GameMode.TeamBattle:
                    StartTeamBattle();
                    break;
                case GameMode.CaptureTheFlag:
                    StartCaptureTheFlag();
                    break;
                case GameMode.Survival:
                    StartSurvival();
                    break;
            }
            
            OnGameModeChanged?.Invoke(mode);
            
            if (audioManager != null)
            {
                audioManager.PlayGameStart();
                audioManager.PlayBattleMusic();
            }
            
            Debug.Log($"Game started: {mode}");
        }
        
        /// <summary>
        /// Reset game state
        /// </summary>
        private void ResetGameState()
        {
            currentGameTime = gameTime;
            playerScores.Clear();
            playerKills.Clear();
            playerDeaths.Clear();
            playerAlive.Clear();
            
            // Initialize player data
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                playerScores[player.ActorNumber] = 0;
                playerKills[player.ActorNumber] = 0;
                playerDeaths[player.ActorNumber] = 0;
                playerAlive[player.ActorNumber] = true;
            }
        }
        
        /// <summary>
        /// Start Deathmatch mode
        /// </summary>
        private void StartDeathmatch()
        {
            currentGameTime = gameTime;
            Debug.Log("Deathmatch started - Free for all!");
        }
        
        /// <summary>
        /// Start Team Battle mode
        /// </summary>
        private void StartTeamBattle()
        {
            if (teamManager != null)
            {
                teamManager.StartTeamGame();
            }
            
            currentGameTime = gameTime;
            Debug.Log("Team Battle started - Red vs Blue!");
        }
        
        /// <summary>
        /// Start Capture The Flag mode
        /// </summary>
        private void StartCaptureTheFlag()
        {
            if (teamManager != null)
            {
                teamManager.StartTeamGame();
            }
            
            // Activate flags
            if (redFlag != null) redFlag.SetActive(true);
            if (blueFlag != null) blueFlag.SetActive(true);
            
            redFlagTaken = false;
            blueFlagTaken = false;
            redFlagCarrier = null;
            blueFlagCarrier = null;
            redFlagCaptures = 0;
            blueFlagCaptures = 0;
            
            currentGameTime = gameTime;
            Debug.Log("Capture The Flag started!");
        }
        
        /// <summary>
        /// Start Survival mode
        /// </summary>
        private void StartSurvival()
        {
            currentGameTime = survivalTime;
            alivePlayers.Clear();
            playerLives.Clear();
            
            // Initialize player lives
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                alivePlayers.Add(player);
                playerLives[player.ActorNumber] = eliminations;
            }
            
            Debug.Log($"Survival started - {alivePlayers.Count} players alive!");
        }
        
        /// <summary>
        /// Handle player killed
        /// </summary>
        public void OnPlayerKilled(Player victim, Player killer)
        {
            if (!isGameActive) return;
            
            // Update deaths
            if (playerDeaths.ContainsKey(victim.ActorNumber))
            {
                playerDeaths[victim.ActorNumber]++;
            }
            
            // Update kills and scores
            if (killer != null && killer != victim)
            {
                if (playerKills.ContainsKey(killer.ActorNumber))
                {
                    playerKills[killer.ActorNumber]++;
                }
                
                // Mode specific scoring
                switch (currentGameMode)
                {
                    case GameMode.Deathmatch:
                        AddPlayerScore(killer, killScore);
                        break;
                        
                    case GameMode.TeamBattle:
                        // Check friendly fire
                        if (teamManager != null && !teamManager.AreTeammates(killer, victim))
                        {
                            AddPlayerScore(killer, teamKillScore);
                            var killerTeam = teamManager.GetPlayerTeam(killer);
                            teamManager.AddTeamScore(killerTeam, teamKillScore);
                        }
                        break;
                        
                    case GameMode.CaptureTheFlag:
                        // Same as team battle
                        if (teamManager != null && !teamManager.AreTeammates(killer, victim))
                        {
                            AddPlayerScore(killer, killScore);
                        }
                        
                        // Drop flag if carrier was killed
                        if (victim == redFlagCarrier)
                        {
                            DropFlag(TeamManager.Team.Red);
                        }
                        else if (victim == blueFlagCarrier)
                        {
                            DropFlag(TeamManager.Team.Blue);
                        }
                        break;
                        
                    case GameMode.Survival:
                        // Reduce lives
                        if (playerLives.ContainsKey(victim.ActorNumber))
                        {
                            playerLives[victim.ActorNumber]--;
                            if (playerLives[victim.ActorNumber] <= 0)
                            {
                                EliminatePlayer(victim);
                            }
                        }
                        break;
                }
            }
            
            OnPlayerKilled?.Invoke(victim);
            
            if (audioManager != null)
            {
                audioManager.PlayKill();
            }
        }
        
        /// <summary>
        /// Add score untuk player
        /// </summary>
        private void AddPlayerScore(Player player, int points)
        {
            if (playerScores.ContainsKey(player.ActorNumber))
            {
                playerScores[player.ActorNumber] += points;
                OnPlayerScoreChanged?.Invoke(player, playerScores[player.ActorNumber]);
                
                if (audioManager != null)
                {
                    audioManager.PlayScore();
                }
            }
        }
        
        /// <summary>
        /// Eliminate player dari survival
        /// </summary>
        private void EliminatePlayer(Player player)
        {
            alivePlayers.Remove(player);
            playerAlive[player.ActorNumber] = false;
            
            OnPlayerEliminated?.Invoke(player);
            
            Debug.Log($"Player {player.NickName} eliminated! {alivePlayers.Count} players remaining.");
        }
        
        /// <summary>
        /// Handle flag pickup
        /// </summary>
        public void OnFlagPickedUp(Player player, TeamManager.Team flagTeam)
        {
            if (currentGameMode != GameMode.CaptureTheFlag) return;
            
            if (teamManager == null) return;
            
            var playerTeam = teamManager.GetPlayerTeam(player);
            
            // Can't pick up own team's flag
            if (playerTeam == flagTeam) return;
            
            if (flagTeam == TeamManager.Team.Red && !redFlagTaken)
            {
                redFlagTaken = true;
                redFlagCarrier = player;
                OnFlagTaken?.Invoke(player, true);
            }
            else if (flagTeam == TeamManager.Team.Blue && !blueFlagTaken)
            {
                blueFlagTaken = true;
                blueFlagCarrier = player;
                OnFlagTaken?.Invoke(player, false);
            }
            
            Debug.Log($"Player {player.NickName} picked up {flagTeam} flag!");
        }
        
        /// <summary>
        /// Handle flag capture
        /// </summary>
        public void OnFlagCaptured(Player player, TeamManager.Team flagTeam)
        {
            if (currentGameMode != GameMode.CaptureTheFlag) return;
            
            if (teamManager == null) return;
            
            var playerTeam = teamManager.GetPlayerTeam(player);
            
            // Score capture
            AddPlayerScore(player, flagCaptureScore);
            teamManager.AddTeamScore(playerTeam, flagCaptureScore);
            
            // Update captures
            if (flagTeam == TeamManager.Team.Red)
            {
                blueFlagCaptures++; // Blue team captured red flag
                ReturnFlag(TeamManager.Team.Red);
            }
            else if (flagTeam == TeamManager.Team.Blue)
            {
                redFlagCaptures++; // Red team captured blue flag
                ReturnFlag(TeamManager.Team.Blue);
            }
            
            OnFlagCaptured?.Invoke(playerTeam);
            
            if (audioManager != null)
            {
                audioManager.PlayScore();
            }
            
            Debug.Log($"Flag captured by {playerTeam} team! Score: Red {redFlagCaptures}, Blue {blueFlagCaptures}");
        }
        
        /// <summary>
        /// Drop flag
        /// </summary>
        private void DropFlag(TeamManager.Team flagTeam)
        {
            if (flagTeam == TeamManager.Team.Red)
            {
                redFlagTaken = false;
                redFlagCarrier = null;
                // Return flag to spawn after delay
                StartCoroutine(ReturnFlagAfterDelay(TeamManager.Team.Red, 30f));
            }
            else if (flagTeam == TeamManager.Team.Blue)
            {
                blueFlagTaken = false;
                blueFlagCarrier = null;
                StartCoroutine(ReturnFlagAfterDelay(TeamManager.Team.Blue, 30f));
            }
        }
        
        /// <summary>
        /// Return flag ke spawn
        /// </summary>
        private void ReturnFlag(TeamManager.Team flagTeam)
        {
            if (flagTeam == TeamManager.Team.Red)
            {
                redFlagTaken = false;
                redFlagCarrier = null;
                if (redFlag != null && redFlagSpawn != null)
                {
                    redFlag.transform.position = redFlagSpawn.position;
                }
            }
            else if (flagTeam == TeamManager.Team.Blue)
            {
                blueFlagTaken = false;
                blueFlagCarrier = null;
                if (blueFlag != null && blueFlagSpawn != null)
                {
                    blueFlag.transform.position = blueFlagSpawn.position;
                }
            }
        }
        
        /// <summary>
        /// Return flag after delay
        /// </summary>
        private IEnumerator ReturnFlagAfterDelay(TeamManager.Team flagTeam, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnFlag(flagTeam);
        }
        
        /// <summary>
        /// Update game time
        /// </summary>
        private void UpdateGameTime()
        {
            currentGameTime -= Time.deltaTime;
            
            if (currentGameTime <= 0)
            {
                EndGame();
            }
        }
        
        /// <summary>
        /// Check win conditions
        /// </summary>
        private void CheckWinConditions()
        {
            switch (currentGameMode)
            {
                case GameMode.Deathmatch:
                    CheckDeathmatchWin();
                    break;
                case GameMode.TeamBattle:
                    CheckTeamBattleWin();
                    break;
                case GameMode.CaptureTheFlag:
                    CheckCTFWin();
                    break;
                case GameMode.Survival:
                    CheckSurvivalWin();
                    break;
            }
        }
        
        /// <summary>
        /// Check deathmatch win condition
        /// </summary>
        private void CheckDeathmatchWin()
        {
            foreach (var score in playerScores)
            {
                if (score.Value >= scoreToWin)
                {
                    Player winner = PhotonNetwork.CurrentRoom.GetPlayer(score.Key);
                    if (winner != null)
                    {
                        EndGame(winner);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Check team battle win condition
        /// </summary>
        private void CheckTeamBattleWin()
        {
            if (teamManager != null)
            {
                teamManager.CheckWinCondition(teamScoreToWin);
            }
        }
        
        /// <summary>
        /// Check CTF win condition
        /// </summary>
        private void CheckCTFWin()
        {
            if (redFlagCaptures >= flagsToWin)
            {
                EndGame(TeamManager.Team.Red);
            }
            else if (blueFlagCaptures >= flagsToWin)
            {
                EndGame(TeamManager.Team.Blue);
            }
        }
        
        /// <summary>
        /// Check survival win condition
        /// </summary>
        private void CheckSurvivalWin()
        {
            if (alivePlayers.Count <= 1)
            {
                if (alivePlayers.Count == 1)
                {
                    EndGame(alivePlayers[0]);
                }
                else
                {
                    EndGame(); // Draw
                }
            }
        }
        
        /// <summary>
        /// End game dengan winner
        /// </summary>
        public void EndGame(Player winner = null)
        {
            isGameActive = false;
            
            if (winner != null)
            {
                OnGameWon?.Invoke(winner);
            }
            
            if (audioManager != null)
            {
                audioManager.PlayGameEnd();
                if (winner != null)
                {
                    audioManager.PlayVictoryMusic();
                }
            }
            
            Debug.Log($"Game ended. Winner: {winner?.NickName ?? "Draw"}");
        }
        
        /// <summary>
        /// End game dengan winning team
        /// </summary>
        public void EndGame(TeamManager.Team winningTeam)
        {
            isGameActive = false;
            OnTeamWon?.Invoke(winningTeam);
            
            if (audioManager != null)
            {
                audioManager.PlayGameEnd();
                audioManager.PlayVictoryMusic();
            }
            
            Debug.Log($"Game ended. Winning team: {winningTeam}");
        }
        
        /// <summary>
        /// Get player score
        /// </summary>
        public int GetPlayerScore(Player player)
        {
            return playerScores.ContainsKey(player.ActorNumber) ? playerScores[player.ActorNumber] : 0;
        }
        
        /// <summary>
        /// Get player kills
        /// </summary>
        public int GetPlayerKills(Player player)
        {
            return playerKills.ContainsKey(player.ActorNumber) ? playerKills[player.ActorNumber] : 0;
        }
        
        /// <summary>
        /// Get player deaths
        /// </summary>
        public int GetPlayerDeaths(Player player)
        {
            return playerDeaths.ContainsKey(player.ActorNumber) ? playerDeaths[player.ActorNumber] : 0;
        }
        
        /// <summary>
        /// Get current game time
        /// </summary>
        public float GetCurrentGameTime()
        {
            return currentGameTime;
        }
        
        /// <summary>
        /// Get game mode
        /// </summary>
        public GameMode GetCurrentGameMode()
        {
            return currentGameMode;
        }
        
        /// <summary>
        /// Check if game is active
        /// </summary>
        public bool IsGameActive()
        {
            return isGameActive;
        }
    }
}