using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk membuat dan mengelola prefabs
    /// Menangani PlayerPrefab, WeaponPrefabs, EffectPrefabs
    /// </summary>
    public class PrefabManager : MonoBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerModelPrefab;
        
        [Header("Weapon Prefabs")]
        [SerializeField] private GameObject riflePrefab;
        [SerializeField] private GameObject pistolPrefab;
        [SerializeField] private GameObject sniperPrefab;
        [SerializeField] private GameObject shotgunPrefab;
        
        [Header("Effect Prefabs")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private GameObject respawnEffectPrefab;
        [SerializeField] private GameObject bloodEffectPrefab;
        
        [Header("UI Prefabs")]
        [SerializeField] private GameObject damageIndicatorPrefab;
        [SerializeField] private GameObject killFeedEntryPrefab;
        [SerializeField] private GameObject crosshairPrefab;
        
        [Header("Environment Prefabs")]
        [SerializeField] private GameObject spawnPointPrefab;
        [SerializeField] private GameObject flagPrefab;
        [SerializeField] private GameObject powerUpPrefab;
        
        [Header("Audio Prefabs")]
        [SerializeField] private GameObject audioSourcePrefab;
        [SerializeField] private GameObject spatialAudioPrefab;
        
        // Prefab pools untuk performance
        private Dictionary<string, Queue<GameObject>> prefabPools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> prefabTemplates = new Dictionary<string, GameObject>();
        
        // Static instance untuk global access
        public static PrefabManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePrefabManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Initialize prefab manager
        /// </summary>
        private void InitializePrefabManager()
        {
            InitializePrefabTemplates();
            CreatePrefabPools();
            
            Debug.Log("Prefab Manager initialized with pools");
        }
        
        /// <summary>
        /// Initialize prefab templates
        /// </summary>
        private void InitializePrefabTemplates()
        {
            // Player prefabs
            if (playerPrefab != null) prefabTemplates["Player"] = playerPrefab;
            if (playerModelPrefab != null) prefabTemplates["PlayerModel"] = playerModelPrefab;
            
            // Weapon prefabs
            if (riflePrefab != null) prefabTemplates["Rifle"] = riflePrefab;
            if (pistolPrefab != null) prefabTemplates["Pistol"] = pistolPrefab;
            if (sniperPrefab != null) prefabTemplates["Sniper"] = sniperPrefab;
            if (shotgunPrefab != null) prefabTemplates["Shotgun"] = shotgunPrefab;
            
            // Effect prefabs
            if (muzzleFlashPrefab != null) prefabTemplates["MuzzleFlash"] = muzzleFlashPrefab;
            if (hitEffectPrefab != null) prefabTemplates["HitEffect"] = hitEffectPrefab;
            if (deathEffectPrefab != null) prefabTemplates["DeathEffect"] = deathEffectPrefab;
            if (respawnEffectPrefab != null) prefabTemplates["RespawnEffect"] = respawnEffectPrefab;
            if (bloodEffectPrefab != null) prefabTemplates["BloodEffect"] = bloodEffectPrefab;
            
            // UI prefabs
            if (damageIndicatorPrefab != null) prefabTemplates["DamageIndicator"] = damageIndicatorPrefab;
            if (killFeedEntryPrefab != null) prefabTemplates["KillFeedEntry"] = killFeedEntryPrefab;
            if (crosshairPrefab != null) prefabTemplates["Crosshair"] = crosshairPrefab;
            
            // Environment prefabs
            if (spawnPointPrefab != null) prefabTemplates["SpawnPoint"] = spawnPointPrefab;
            if (flagPrefab != null) prefabTemplates["Flag"] = flagPrefab;
            if (powerUpPrefab != null) prefabTemplates["PowerUp"] = powerUpPrefab;
            
            // Audio prefabs
            if (audioSourcePrefab != null) prefabTemplates["AudioSource"] = audioSourcePrefab;
            if (spatialAudioPrefab != null) prefabTemplates["SpatialAudio"] = spatialAudioPrefab;
        }
        
        /// <summary>
        /// Create prefab pools untuk performance
        /// </summary>
        private void CreatePrefabPools()
        {
            // Create pools untuk effect prefabs yang sering digunakan
            CreatePool("MuzzleFlash", 20);
            CreatePool("HitEffect", 30);
            CreatePool("BloodEffect", 15);
            CreatePool("DamageIndicator", 10);
            CreatePool("AudioSource", 15);
            CreatePool("SpatialAudio", 10);
        }
        
        /// <summary>
        /// Create pool untuk prefab tertentu
        /// </summary>
        private void CreatePool(string prefabName, int poolSize)
        {
            if (!prefabTemplates.ContainsKey(prefabName)) return;
            
            Queue<GameObject> pool = new Queue<GameObject>();
            
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefabTemplates[prefabName]);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
            }
            
            prefabPools[prefabName] = pool;
        }
        
        /// <summary>
        /// Spawn prefab dengan pooling
        /// </summary>
        public GameObject SpawnPrefab(string prefabName, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject obj = null;
            
            // Try get from pool first
            if (prefabPools.ContainsKey(prefabName) && prefabPools[prefabName].Count > 0)
            {
                obj = prefabPools[prefabName].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                
                if (parent != null)
                {
                    obj.transform.SetParent(parent);
                }
                
                obj.SetActive(true);
            }
            else if (prefabTemplates.ContainsKey(prefabName))
            {
                // Create new instance jika pool kosong
                obj = Instantiate(prefabTemplates[prefabName], position, rotation, parent);
            }
            
            return obj;
        }
        
        /// <summary>
        /// Return prefab ke pool
        /// </summary>
        public void ReturnToPool(string prefabName, GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            if (prefabPools.ContainsKey(prefabName))
            {
                prefabPools[prefabName].Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        
        /// <summary>
        /// Spawn player prefab untuk networking
        /// </summary>
        public GameObject SpawnNetworkPlayer(Vector3 position, Quaternion rotation)
        {
            if (playerPrefab != null)
            {
                return PhotonNetwork.Instantiate(playerPrefab.name, position, rotation);
            }
            
            Debug.LogError("Player prefab not found!");
            return null;
        }
        
        /// <summary>
        /// Create complete player prefab dengan semua components
        /// </summary>
        [ContextMenu("Create Player Prefab")]
        public GameObject CreatePlayerPrefab()
        {
            GameObject player = new GameObject("Player");
            
            // Add essential components
            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 1.8f;
            characterController.center = new Vector3(0, 0.9f, 0);
            
            // Add player scripts
            player.AddComponent<AirsoftARBattle.Player.PlayerController>();
            player.AddComponent<AirsoftARBattle.Player.PlayerHealth>();
            
            // Add networking
            PhotonView photonView = player.AddComponent<PhotonView>();
            photonView.observeUsingOnSerializePhotonView = true;
            
            // Create camera rig
            GameObject cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(player.transform);
            cameraRig.transform.localPosition = new Vector3(0, 1.6f, 0);
            
            // Create weapon holder
            GameObject weaponHolder = new GameObject("WeaponHolder");
            weaponHolder.transform.SetParent(cameraRig.transform);
            weaponHolder.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            
            // Add weapon controller
            weaponHolder.AddComponent<AirsoftARBattle.Weapons.WeaponController>();
            
            // Create player model holder
            if (playerModelPrefab != null)
            {
                GameObject model = Instantiate(playerModelPrefab, player.transform);
                model.name = "PlayerModel";
            }
            
            Debug.Log("Player prefab created successfully!");
            return player;
        }
        
        /// <summary>
        /// Create weapon prefab
        /// </summary>
        [ContextMenu("Create Weapon Prefabs")]
        public void CreateWeaponPrefabs()
        {
            CreateWeaponPrefab("Rifle", 30, 25f, 0.1f, 100f);
            CreateWeaponPrefab("Pistol", 15, 20f, 0.2f, 50f);
            CreateWeaponPrefab("Sniper", 10, 80f, 1f, 200f);
            CreateWeaponPrefab("Shotgun", 8, 60f, 0.8f, 30f);
            
            Debug.Log("Weapon prefabs created!");
        }
        
        /// <summary>
        /// Create weapon prefab dengan stats
        /// </summary>
        private GameObject CreateWeaponPrefab(string weaponName, int ammo, float damage, float fireRate, float range)
        {
            GameObject weapon = new GameObject($"{weaponName}Weapon");
            
            // Add weapon controller
            AirsoftARBattle.Weapons.WeaponController weaponController = weapon.AddComponent<AirsoftARBattle.Weapons.WeaponController>();
            
            // Create weapon model
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "WeaponModel";
            model.transform.SetParent(weapon.transform);
            model.transform.localScale = new Vector3(0.1f, 0.05f, 0.5f);
            
            // Create muzzle point
            GameObject muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(weapon.transform);
            muzzle.transform.localPosition = new Vector3(0, 0, 0.5f);
            
            // Add audio source
            weapon.AddComponent<AudioSource>();
            
            return weapon;
        }
        
        /// <summary>
        /// Create effect prefabs
        /// </summary>
        [ContextMenu("Create Effect Prefabs")]
        public void CreateEffectPrefabs()
        {
            CreateMuzzleFlashEffect();
            CreateHitEffect();
            CreateBloodEffect();
            CreateDeathEffect();
            CreateRespawnEffect();
            
            Debug.Log("Effect prefabs created!");
        }
        
        /// <summary>
        /// Create muzzle flash effect
        /// </summary>
        private GameObject CreateMuzzleFlashEffect()
        {
            GameObject effect = new GameObject("MuzzleFlashEffect");
            
            // Add particle system
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.1f;
            main.startSpeed = 5f;
            main.startColor = Color.yellow;
            main.maxParticles = 20;
            
            // Add light
            Light light = effect.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.yellow;
            light.intensity = 2f;
            light.range = 5f;
            
            // Auto destroy
            effect.AddComponent<AutoDestroy>().lifetime = 0.2f;
            
            return effect;
        }
        
        /// <summary>
        /// Create hit effect
        /// </summary>
        private GameObject CreateHitEffect()
        {
            GameObject effect = new GameObject("HitEffect");
            
            // Add particle system
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startColor = Color.white;
            main.maxParticles = 10;
            
            effect.AddComponent<AutoDestroy>().lifetime = 1f;
            
            return effect;
        }
        
        /// <summary>
        /// Create blood effect
        /// </summary>
        private GameObject CreateBloodEffect()
        {
            GameObject effect = new GameObject("BloodEffect");
            
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startColor = Color.red;
            main.maxParticles = 15;
            
            effect.AddComponent<AutoDestroy>().lifetime = 2f;
            
            return effect;
        }
        
        /// <summary>
        /// Create death effect
        /// </summary>
        private GameObject CreateDeathEffect()
        {
            GameObject effect = new GameObject("DeathEffect");
            
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startColor = Color.gray;
            main.maxParticles = 30;
            
            effect.AddComponent<AudioSource>();
            effect.AddComponent<AutoDestroy>().lifetime = 3f;
            
            return effect;
        }
        
        /// <summary>
        /// Create respawn effect
        /// </summary>
        private GameObject CreateRespawnEffect()
        {
            GameObject effect = new GameObject("RespawnEffect");
            
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startColor = Color.cyan;
            main.maxParticles = 25;
            
            // Add light
            Light light = effect.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.cyan;
            light.intensity = 1f;
            light.range = 3f;
            
            effect.AddComponent<AutoDestroy>().lifetime = 2f;
            
            return effect;
        }
        
        /// <summary>
        /// Get prefab template
        /// </summary>
        public GameObject GetPrefabTemplate(string prefabName)
        {
            return prefabTemplates.ContainsKey(prefabName) ? prefabTemplates[prefabName] : null;
        }
        
        /// <summary>
        /// Check if prefab exists
        /// </summary>
        public bool HasPrefab(string prefabName)
        {
            return prefabTemplates.ContainsKey(prefabName);
        }
        
        /// <summary>
        /// Get all prefab names
        /// </summary>
        public string[] GetAllPrefabNames()
        {
            string[] names = new string[prefabTemplates.Count];
            prefabTemplates.Keys.CopyTo(names, 0);
            return names;
        }
    }
    
    /// <summary>
    /// Component untuk auto destroy objects
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        public float lifetime = 2f;
        
        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}