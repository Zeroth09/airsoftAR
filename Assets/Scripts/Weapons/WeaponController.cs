using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace AirsoftARBattle.Weapons
{
    /// <summary>
    /// Controller untuk senjata airsoft dengan AR tracking
    /// Mengelola aiming, shooting, dan weapon physics
    /// </summary>
    public class WeaponController : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponType weaponType = WeaponType.Rifle;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 100f;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private float reloadTime = 2f;
        
        [Header("AR Tracking")]
        [SerializeField] private Transform weaponModel;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private LayerMask targetLayerMask = 1;
        
        [Header("Effects")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip reloadSound;
        
        [Header("UI")]
        [SerializeField] private GameObject crosshairUI;
        [SerializeField] private GameObject ammoUI;
        
        // Private variables
        private int currentAmmo;
        private bool isReloading = false;
        private bool canShoot = true;
        private float lastShootTime;
        private AudioSource audioSource;
        private Camera playerCamera;
        
        // Network variables
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool networkIsShooting;
        
        public enum WeaponType
        {
            Pistol,
            Rifle,
            Sniper,
            Shotgun
        }
        
        private void Start()
        {
            InitializeWeapon();
        }
        
        private void Update()
        {
            if (photonView.IsMine)
            {
                HandleInput();
                UpdateWeaponTracking();
            }
            else
            {
                // Smooth interpolation untuk network
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
        }
        
        /// <summary>
        /// Inisialisasi weapon
        /// </summary>
        private void InitializeWeapon()
        {
            currentAmmo = maxAmmo;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Setup camera reference
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
            
            // Setup UI
            if (crosshairUI != null)
                crosshairUI.SetActive(true);
            
            UpdateAmmoUI();
        }
        
        /// <summary>
        /// Handle input untuk shooting dan reloading
        /// </summary>
        private void HandleInput()
        {
            // Shooting
            if (Input.GetButton("Fire1") && canShoot && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
            
            // Reloading
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            {
                StartReload();
            }
        }
        
        /// <summary>
        /// Update weapon tracking dengan AR
        /// </summary>
        private void UpdateWeaponTracking()
        {
            if (weaponModel != null && playerCamera != null)
            {
                // Track weapon position berdasarkan camera
                Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
                weaponModel.position = Vector3.Lerp(weaponModel.position, targetPosition, Time.deltaTime * 5f);
                
                // Align weapon dengan camera direction
                weaponModel.rotation = Quaternion.Lerp(weaponModel.rotation, playerCamera.transform.rotation, Time.deltaTime * 3f);
            }
        }
        
        /// <summary>
        /// Shoot weapon
        /// </summary>
        private void Shoot()
        {
            if (Time.time - lastShootTime < fireRate)
                return;
                
            lastShootTime = Time.time;
            currentAmmo--;
            
            // Network shooting
            photonView.RPC("NetworkShoot", RpcTarget.All);
            
            // Local effects
            PlayShootEffects();
            
            // Raycast untuk hit detection
            PerformRaycast();
            
            UpdateAmmoUI();
        }
        
        /// <summary>
        /// Network shoot RPC
        /// </summary>
        [PunRPC]
        private void NetworkShoot()
        {
            if (!photonView.IsMine)
            {
                PlayShootEffects();
            }
        }
        
        /// <summary>
        /// Perform raycast untuk hit detection
        /// </summary>
        private void PerformRaycast()
        {
            if (playerCamera == null) return;
            
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, range, targetLayerMask))
            {
                // Hit player
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, photonView.Owner);
                }
                
                // Hit effect
                if (hitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hitEffect, 2f);
                }
            }
        }
        
        /// <summary>
        /// Play shoot effects
        /// </summary>
        private void PlayShootEffects()
        {
            // Muzzle flash
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(muzzleFlash, 0.1f);
            }
            
            // Sound
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
            
            // Recoil effect
            StartCoroutine(RecoilEffect());
        }
        
        /// <summary>
        /// Recoil effect
        /// </summary>
        private IEnumerator RecoilEffect()
        {
            if (weaponModel != null)
            {
                Vector3 originalPosition = weaponModel.localPosition;
                Vector3 recoilPosition = originalPosition + Vector3.back * 0.1f;
                
                weaponModel.localPosition = recoilPosition;
                
                yield return new WaitForSeconds(0.1f);
                
                weaponModel.localPosition = originalPosition;
            }
        }
        
        /// <summary>
        /// Start reload
        /// </summary>
        private void StartReload()
        {
            if (isReloading) return;
            
            isReloading = true;
            canShoot = false;
            
            // Play reload sound
            if (reloadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(reloadSound);
            }
            
            StartCoroutine(ReloadCoroutine());
        }
        
        /// <summary>
        /// Reload coroutine
        /// </summary>
        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadTime);
            
            currentAmmo = maxAmmo;
            isReloading = false;
            canShoot = true;
            
            UpdateAmmoUI();
        }
        
        /// <summary>
        /// Update ammo UI
        /// </summary>
        private void UpdateAmmoUI()
        {
            if (ammoUI != null)
            {
                // Update ammo display
                TMPro.TextMeshProUGUI ammoText = ammoUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (ammoText != null)
                {
                    ammoText.text = $"{currentAmmo}/{maxAmmo}";
                }
            }
        }
        
        /// <summary>
        /// Get current ammo
        /// </summary>
        public int GetCurrentAmmo()
        {
            return currentAmmo;
        }
        
        /// <summary>
        /// Get max ammo
        /// </summary>
        public int GetMaxAmmo()
        {
            return maxAmmo;
        }
        
        /// <summary>
        /// Check if reloading
        /// </summary>
        public bool IsReloading()
        {
            return isReloading;
        }
        
        /// <summary>
        /// Network synchronization
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send data
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(networkIsShooting);
            }
            else
            {
                // Receive data
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                networkIsShooting = (bool)stream.ReceiveNext();
            }
        }
        
        /// <summary>
        /// Set weapon type
        /// </summary>
        public void SetWeaponType(WeaponType type)
        {
            weaponType = type;
            
            // Update weapon stats berdasarkan type
            switch (type)
            {
                case WeaponType.Pistol:
                    damage = 20f;
                    range = 50f;
                    fireRate = 0.3f;
                    maxAmmo = 15;
                    break;
                case WeaponType.Rifle:
                    damage = 25f;
                    range = 100f;
                    fireRate = 0.1f;
                    maxAmmo = 30;
                    break;
                case WeaponType.Sniper:
                    damage = 100f;
                    range = 200f;
                    fireRate = 1f;
                    maxAmmo = 5;
                    break;
                case WeaponType.Shotgun:
                    damage = 40f;
                    range = 30f;
                    fireRate = 0.8f;
                    maxAmmo = 8;
                    break;
            }
            
            currentAmmo = maxAmmo;
            UpdateAmmoUI();
        }
    }
} 