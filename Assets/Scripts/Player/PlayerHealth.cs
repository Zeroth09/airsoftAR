using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace AirsoftARBattle.Player
{
    /// <summary>
    /// Sistem health dan damage untuk player
    /// Mengelola health, damage, dan respawn system
    /// </summary>
    public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float respawnTime = 5f;
        [SerializeField] private float invincibilityTime = 2f;
        
        [Header("UI")]
        [SerializeField] private GameObject healthBarUI;
        [SerializeField] private GameObject damageIndicator;
        
        [Header("Effects")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private GameObject respawnEffectPrefab;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip respawnSound;
        
        [Header("Visual")]
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material damageMaterial;
        [SerializeField] private Material invincibleMaterial;
        
        // Private variables
        private bool isDead = false;
        private bool isInvincible = false;
        private float lastDamageTime;
        private AudioSource audioSource;
        private PlayerController playerController;
        private WeaponController weaponController;
        
        // Network variables
        private float networkHealth;
        private bool networkIsDead;
        
        // Events
        public System.Action<float> OnHealthChanged;
        public System.Action OnPlayerDied;
        public System.Action OnPlayerRespawned;
        
        private void Start()
        {
            InitializePlayer();
        }
        
        private void Update()
        {
            if (photonView.IsMine)
            {
                UpdateHealthUI();
                CheckInvincibility();
            }
        }
        
        /// <summary>
        /// Inisialisasi player
        /// </summary>
        private void InitializePlayer()
        {
            currentHealth = maxHealth;
            networkHealth = maxHealth;
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            playerController = GetComponent<PlayerController>();
            weaponController = GetComponentInChildren<WeaponController>();
            
            UpdateHealthUI();
        }
        
        /// <summary>
        /// Take damage dari weapon
        /// </summary>
        public void TakeDamage(float damage, Player attacker)
        {
            if (isDead || isInvincible) return;
            
            currentHealth -= damage;
            lastDamageTime = Time.time;
            
            // Network damage
            photonView.RPC("NetworkTakeDamage", RpcTarget.All, damage, attacker);
            
            // Local effects
            PlayDamageEffects();
            
            // Check if dead
            if (currentHealth <= 0)
            {
                Die(attacker);
            }
            
            UpdateHealthUI();
        }
        
        /// <summary>
        /// Network damage RPC
        /// </summary>
        [PunRPC]
        private void NetworkTakeDamage(float damage, Player attacker)
        {
            if (!photonView.IsMine)
            {
                PlayDamageEffects();
            }
        }
        
        /// <summary>
        /// Play damage effects
        /// </summary>
        private void PlayDamageEffects()
        {
            // Sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            // Visual feedback
            StartCoroutine(DamageFlash());
            
            // Damage indicator
            if (damageIndicator != null)
            {
                damageIndicator.SetActive(true);
                StartCoroutine(HideDamageIndicator());
            }
        }
        
        /// <summary>
        /// Damage flash effect
        /// </summary>
        private IEnumerator DamageFlash()
        {
            if (playerRenderer != null && damageMaterial != null)
            {
                Material originalMaterial = playerRenderer.material;
                playerRenderer.material = damageMaterial;
                
                yield return new WaitForSeconds(0.1f);
                
                playerRenderer.material = originalMaterial;
            }
        }
        
        /// <summary>
        /// Hide damage indicator
        /// </summary>
        private IEnumerator HideDamageIndicator()
        {
            yield return new WaitForSeconds(1f);
            if (damageIndicator != null)
            {
                damageIndicator.SetActive(false);
            }
        }
        
        /// <summary>
        /// Player die
        /// </summary>
        private void Die(Player attacker)
        {
            if (isDead) return;
            
            isDead = true;
            networkIsDead = true;
            
            // Network death
            photonView.RPC("NetworkDie", RpcTarget.All, attacker);
            
            // Local death effects
            PlayDeathEffects();
            
            // Disable player controls
            if (playerController != null)
            {
                playerController.SetPlayerActive(false);
            }
            
            if (weaponController != null)
            {
                weaponController.enabled = false;
            }
            
            // Start respawn timer
            StartCoroutine(RespawnCoroutine());
            
            OnPlayerDied?.Invoke();
        }
        
        /// <summary>
        /// Network die RPC
        /// </summary>
        [PunRPC]
        private void NetworkDie(Player attacker)
        {
            if (!photonView.IsMine)
            {
                PlayDeathEffects();
            }
        }
        
        /// <summary>
        /// Play death effects
        /// </summary>
        private void PlayDeathEffects()
        {
            // Death effect
            if (deathEffectPrefab != null)
            {
                GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(deathEffect, 3f);
            }
            
            // Sound
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            // Visual
            if (playerRenderer != null)
            {
                playerRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// Respawn coroutine
        /// </summary>
        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(respawnTime);
            
            Respawn();
        }
        
        /// <summary>
        /// Respawn player
        /// </summary>
        private void Respawn()
        {
            if (!isDead) return;
            
            isDead = false;
            networkIsDead = false;
            currentHealth = maxHealth;
            networkHealth = maxHealth;
            
            // Network respawn
            photonView.RPC("NetworkRespawn", RpcTarget.All);
            
            // Local respawn effects
            PlayRespawnEffects();
            
            // Enable player controls
            if (playerController != null)
            {
                playerController.SetPlayerActive(true);
            }
            
            if (weaponController != null)
            {
                weaponController.enabled = true;
            }
            
            // Set invincible
            StartCoroutine(InvincibilityCoroutine());
            
            UpdateHealthUI();
            
            OnPlayerRespawned?.Invoke();
        }
        
        /// <summary>
        /// Network respawn RPC
        /// </summary>
        [PunRPC]
        private void NetworkRespawn()
        {
            if (!photonView.IsMine)
            {
                PlayRespawnEffects();
            }
        }
        
        /// <summary>
        /// Play respawn effects
        /// </summary>
        private void PlayRespawnEffects()
        {
            // Respawn effect
            if (respawnEffectPrefab != null)
            {
                GameObject respawnEffect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
                Destroy(respawnEffect, 2f);
            }
            
            // Sound
            if (respawnSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(respawnSound);
            }
            
            // Visual
            if (playerRenderer != null)
            {
                playerRenderer.enabled = true;
            }
        }
        
        /// <summary>
        /// Invincibility coroutine
        /// </summary>
        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;
            
            // Visual invincibility
            if (playerRenderer != null && invincibleMaterial != null)
            {
                Material originalMaterial = playerRenderer.material;
                playerRenderer.material = invincibleMaterial;
                
                yield return new WaitForSeconds(invincibilityTime);
                
                playerRenderer.material = originalMaterial;
            }
            else
            {
                yield return new WaitForSeconds(invincibilityTime);
            }
            
            isInvincible = false;
        }
        
        /// <summary>
        /// Check invincibility
        /// </summary>
        private void CheckInvincibility()
        {
            if (isInvincible && Time.time - lastDamageTime > invincibilityTime)
            {
                isInvincible = false;
            }
        }
        
        /// <summary>
        /// Update health UI
        /// </summary>
        private void UpdateHealthUI()
        {
            if (healthBarUI != null)
            {
                // Update health bar
                UnityEngine.UI.Slider healthSlider = healthBarUI.GetComponent<UnityEngine.UI.Slider>();
                if (healthSlider != null)
                {
                    healthSlider.value = currentHealth / maxHealth;
                }
                
                // Update health text
                TMPro.TextMeshProUGUI healthText = healthBarUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (healthText != null)
                {
                    healthText.text = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
                }
            }
            
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        /// <summary>
        /// Get current health
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        /// <summary>
        /// Get max health
        /// </summary>
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        /// <summary>
        /// Check if dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }
        
        /// <summary>
        /// Check if invincible
        /// </summary>
        public bool IsInvincible()
        {
            return isInvincible;
        }
        
        /// <summary>
        /// Heal player
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            UpdateHealthUI();
        }
        
        /// <summary>
        /// Network synchronization
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send data
                stream.SendNext(currentHealth);
                stream.SendNext(isDead);
            }
            else
            {
                // Receive data
                networkHealth = (float)stream.ReceiveNext();
                networkIsDead = (bool)stream.ReceiveNext();
            }
        }
        
        /// <summary>
        /// Force respawn (untuk admin/cheat)
        /// </summary>
        public void ForceRespawn()
        {
            if (isDead)
            {
                Respawn();
            }
        }
    }
} 