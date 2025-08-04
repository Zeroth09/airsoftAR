using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk sistem team battle Red vs Blue
    /// Mengelola team assignment, scoring, dan objectives
    /// </summary>
    public class TeamManager : MonoBehaviourPunCallbacks
    {
        [Header("Team Settings")]
        [SerializeField] private int maxPlayersPerTeam = 4;
        [SerializeField] private bool autoBalance = true;
        [SerializeField] private bool allowTeamSwitch = false;
        
        [Header("Team Colors")]
        [SerializeField] private Color redTeamColor = Color.red;
        [SerializeField] private Color blueTeamColor = Color.blue;
        [SerializeField] private Material redTeamMaterial;
        [SerializeField] private Material blueTeamMaterial;
        
        [Header("Spawn Areas")]
        [SerializeField] private Transform[] redTeamSpawns;
        [SerializeField] private Transform[] blueTeamSpawns;
        [SerializeField] private float spawnRadius = 5f;
        
        // Team data
        private Dictionary<int, Team> playerTeams = new Dictionary<int, Team>();
        private Dictionary<Team, List<Player>> teamPlayers = new Dictionary<Team, List<Player>>();
        private Dictionary<Team, int> teamScores = new Dictionary<Team, int>();
        private Dictionary<Team, List<Vector3>> teamSpawnPoints = new Dictionary<Team, List<Vector3>>();
        
        // Game state
        private bool isTeamGameActive = false;
        private Team winningTeam = Team.None;
        
        public enum Team
        {
            None,
            Red,
            Blue
        }
        
        // Events
        public System.Action<Player, Team> OnPlayerJoinedTeam;
        public System.Action<Player, Team> OnPlayerLeftTeam;
        public System.Action<Team, int> OnTeamScoreChanged;
        public System.Action<Team> OnTeamWon;
        public System.Action OnTeamsBalanced;
        
        private void Start()
        {
            InitializeTeamManager();
        }
        
        /// <summary>
        /// Initialize team manager
        /// </summary>
        private void InitializeTeamManager()
        {
            // Initialize team data
            teamPlayers[Team.Red] = new List<Player>();
            teamPlayers[Team.Blue] = new List<Player>();
            teamScores[Team.Red] = 0;
            teamScores[Team.Blue] = 0;
            
            // Setup spawn points
            SetupSpawnPoints();
            
            Debug.Log("Team Manager initialized");
        }
        
        /// <summary>
        /// Setup spawn points untuk setiap team
        /// </summary>
        private void SetupSpawnPoints()
        {
            teamSpawnPoints[Team.Red] = new List<Vector3>();
            teamSpawnPoints[Team.Blue] = new List<Vector3>();
            
            // Red team spawns
            if (redTeamSpawns != null)
            {
                foreach (var spawn in redTeamSpawns)
                {
                    if (spawn != null)
                    {
                        teamSpawnPoints[Team.Red].Add(spawn.position);
                    }
                }
            }
            
            // Blue team spawns
            if (blueTeamSpawns != null)
            {
                foreach (var spawn in blueTeamSpawns)
                {
                    if (spawn != null)
                    {
                        teamSpawnPoints[Team.Blue].Add(spawn.position);
                    }
                }
            }
        }
        
        /// <summary>
        /// Assign player ke team
        /// </summary>
        public void AssignPlayerToTeam(Player player, Team team)
        {
            if (player == null || team == Team.None) return;
            
            // Remove dari team sebelumnya
            RemovePlayerFromCurrentTeam(player);
            
            // Check team capacity
            if (teamPlayers[team].Count >= maxPlayersPerTeam)
            {
                Debug.LogWarning($"Team {team} is full!");
                return;
            }
            
            // Add ke team baru
            playerTeams[player.ActorNumber] = team;
            teamPlayers[team].Add(player);
            
            // Network update
            photonView.RPC("NetworkPlayerJoinedTeam", RpcTarget.All, player.ActorNumber, (int)team);
            
            OnPlayerJoinedTeam?.Invoke(player, team);
            
            Debug.Log($"Player {player.NickName} assigned to {team} team");
        }
        
        /// <summary>
        /// Auto assign player ke team berdasarkan balance
        /// </summary>
        public Team AutoAssignPlayerToTeam(Player player)
        {
            Team assignedTeam = GetLeastPopulatedTeam();
            AssignPlayerToTeam(player, assignedTeam);
            return assignedTeam;
        }
        
        /// <summary>
        /// Remove player dari current team
        /// </summary>
        private void RemovePlayerFromCurrentTeam(Player player)
        {
            if (playerTeams.ContainsKey(player.ActorNumber))
            {
                Team currentTeam = playerTeams[player.ActorNumber];
                teamPlayers[currentTeam].Remove(player);
                playerTeams.Remove(player.ActorNumber);
                
                OnPlayerLeftTeam?.Invoke(player, currentTeam);
            }
        }
        
        /// <summary>
        /// Get team dengan populasi paling sedikit
        /// </summary>
        private Team GetLeastPopulatedTeam()
        {
            int redCount = teamPlayers[Team.Red].Count;
            int blueCount = teamPlayers[Team.Blue].Count;
            
            if (redCount <= blueCount)
                return Team.Red;
            else
                return Team.Blue;
        }
        
        /// <summary>
        /// Get player team
        /// </summary>
        public Team GetPlayerTeam(Player player)
        {
            if (player == null) return Team.None;
            
            if (playerTeams.ContainsKey(player.ActorNumber))
            {
                return playerTeams[player.ActorNumber];
            }
            
            return Team.None;
        }
        
        /// <summary>
        /// Get player team by actor number
        /// </summary>
        public Team GetPlayerTeam(int actorNumber)
        {
            if (playerTeams.ContainsKey(actorNumber))
            {
                return playerTeams[actorNumber];
            }
            
            return Team.None;
        }
        
        /// <summary>
        /// Check apakah players adalah teammates
        /// </summary>
        public bool AreTeammates(Player player1, Player player2)
        {
            if (player1 == null || player2 == null) return false;
            
            Team team1 = GetPlayerTeam(player1);
            Team team2 = GetPlayerTeam(player2);
            
            return team1 != Team.None && team1 == team2;
        }
        
        /// <summary>
        /// Get team spawn position
        /// </summary>
        public Vector3 GetTeamSpawnPosition(Team team)
        {
            if (!teamSpawnPoints.ContainsKey(team) || teamSpawnPoints[team].Count == 0)
            {
                return Vector3.zero;
            }
            
            // Random spawn point dari team
            List<Vector3> spawns = teamSpawnPoints[team];
            Vector3 baseSpawn = spawns[Random.Range(0, spawns.Count)];
            
            // Add random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );
            
            return baseSpawn + randomOffset;
        }
        
        /// <summary>
        /// Add score untuk team
        /// </summary>
        public void AddTeamScore(Team team, int points)
        {
            if (team == Team.None || !isTeamGameActive) return;
            
            teamScores[team] += points;
            
            // Network update
            photonView.RPC("NetworkTeamScoreChanged", RpcTarget.All, (int)team, teamScores[team]);
            
            OnTeamScoreChanged?.Invoke(team, teamScores[team]);
            
            Debug.Log($"{team} team score: {teamScores[team]}");
        }
        
        /// <summary>
        /// Get team score
        /// </summary>
        public int GetTeamScore(Team team)
        {
            if (teamScores.ContainsKey(team))
            {
                return teamScores[team];
            }
            
            return 0;
        }
        
        /// <summary>
        /// Check win condition
        /// </summary>
        public void CheckWinCondition(int scoreToWin)
        {
            if (!isTeamGameActive) return;
            
            foreach (var team in teamScores.Keys)
            {
                if (teamScores[team] >= scoreToWin)
                {
                    TeamWon(team);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Team won
        /// </summary>
        private void TeamWon(Team team)
        {
            winningTeam = team;
            isTeamGameActive = false;
            
            photonView.RPC("NetworkTeamWon", RpcTarget.All, (int)team);
            
            OnTeamWon?.Invoke(team);
            
            Debug.Log($"{team} team won the game!");
        }
        
        /// <summary>
        /// Start team game
        /// </summary>
        public void StartTeamGame()
        {
            isTeamGameActive = true;
            
            // Reset scores
            teamScores[Team.Red] = 0;
            teamScores[Team.Blue] = 0;
            winningTeam = Team.None;
            
            // Auto balance teams
            if (autoBalance)
            {
                BalanceTeams();
            }
            
            Debug.Log("Team game started");
        }
        
        /// <summary>
        /// End team game
        /// </summary>
        public void EndTeamGame()
        {
            isTeamGameActive = false;
            
            Debug.Log("Team game ended");
        }
        
        /// <summary>
        /// Balance teams
        /// </summary>
        public void BalanceTeams()
        {
            List<Player> allPlayers = PhotonNetwork.PlayerList.ToList();
            
            // Clear current teams
            foreach (var team in teamPlayers.Keys.ToList())
            {
                teamPlayers[team].Clear();
            }
            playerTeams.Clear();
            
            // Redistribute players
            for (int i = 0; i < allPlayers.Count; i++)
            {
                Team assignTeam = (i % 2 == 0) ? Team.Red : Team.Blue;
                AssignPlayerToTeam(allPlayers[i], assignTeam);
            }
            
            OnTeamsBalanced?.Invoke();
            
            Debug.Log("Teams have been balanced");
        }
        
        /// <summary>
        /// Get team color
        /// </summary>
        public Color GetTeamColor(Team team)
        {
            switch (team)
            {
                case Team.Red: return redTeamColor;
                case Team.Blue: return blueTeamColor;
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Get team material
        /// </summary>
        public Material GetTeamMaterial(Team team)
        {
            switch (team)
            {
                case Team.Red: return redTeamMaterial;
                case Team.Blue: return blueTeamMaterial;
                default: return null;
            }
        }
        
        /// <summary>
        /// Get team players
        /// </summary>
        public List<Player> GetTeamPlayers(Team team)
        {
            if (teamPlayers.ContainsKey(team))
            {
                return new List<Player>(teamPlayers[team]);
            }
            
            return new List<Player>();
        }
        
        /// <summary>
        /// Get team info
        /// </summary>
        public string GetTeamInfo()
        {
            return $"Red: {teamPlayers[Team.Red].Count}/{maxPlayersPerTeam} | Blue: {teamPlayers[Team.Blue].Count}/{maxPlayersPerTeam}";
        }
        
        // Network RPCs
        [PunRPC]
        private void NetworkPlayerJoinedTeam(int actorNumber, int teamInt)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            Team team = (Team)teamInt;
            
            if (player != null && !photonView.IsMine)
            {
                playerTeams[actorNumber] = team;
                if (!teamPlayers[team].Contains(player))
                {
                    teamPlayers[team].Add(player);
                }
                
                OnPlayerJoinedTeam?.Invoke(player, team);
            }
        }
        
        [PunRPC]
        private void NetworkTeamScoreChanged(int teamInt, int newScore)
        {
            Team team = (Team)teamInt;
            teamScores[team] = newScore;
            
            if (!photonView.IsMine)
            {
                OnTeamScoreChanged?.Invoke(team, newScore);
            }
        }
        
        [PunRPC]
        private void NetworkTeamWon(int teamInt)
        {
            Team team = (Team)teamInt;
            winningTeam = team;
            isTeamGameActive = false;
            
            if (!photonView.IsMine)
            {
                OnTeamWon?.Invoke(team);
            }
        }
        
        // Photon Callbacks
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            RemovePlayerFromCurrentTeam(otherPlayer);
            
            if (autoBalance && isTeamGameActive)
            {
                BalanceTeams();
            }
        }
    }
}