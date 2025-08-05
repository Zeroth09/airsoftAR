using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AirsoftARBattle.Core
{
    /// <summary>
    /// Manager untuk audio system game
    /// Mengelola music, sound effects, dan audio settings
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource weaponSource;
        [SerializeField] private AudioSource ambientSource;
        
        [Header("Music")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;
        
        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;
        [SerializeField] private AudioClip errorSound;
        
        [Header("Weapon Sounds")]
        [SerializeField] private AudioClip rifleShootSound;
        [SerializeField] private AudioClip pistolShootSound;
        [SerializeField] private AudioClip sniperShootSound;
        [SerializeField] private AudioClip shotgunShootSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptyClipSound;
        
        [Header("Player Sounds")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip respawnSound;
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioClip jumpSound;
        
        [Header("Game Sounds")]
        [SerializeField] private AudioClip countdownSound;
        [SerializeField] private AudioClip gameStartSound;
        [SerializeField] private AudioClip gameEndSound;
        [SerializeField] private AudioClip killSound;
        [SerializeField] private AudioClip scoreSound;
        
        [Header("Ambient Sounds")]
        [SerializeField] private AudioClip windAmbient;
        [SerializeField] private AudioClip battleAmbient;
        
        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.8f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float weaponVolume = 1f;
        [SerializeField] private float ambientVolume = 0.6f;
        [SerializeField] private bool muteOnBackground = true;
        
        // Audio pools untuk performance
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // Audio state
        private bool isMuted = false;
        private AudioClip currentMusic;
        private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
        
        // Singleton instance
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            LoadAudioSettings();
            PlayMainMenuMusic();
        }
        
        private void Update()
        {
            UpdateSoundCooldowns();
            CleanupFinishedAudioSources();
        }
        
        /// <summary>
        /// Initialize audio manager
        /// </summary>
        private void InitializeAudioManager()
        {
            // Setup audio sources jika tidak ada
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (weaponSource == null)
            {
                GameObject weaponObj = new GameObject("WeaponSource");
                weaponObj.transform.SetParent(transform);
                weaponSource = weaponObj.AddComponent<AudioSource>();
                weaponSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            // Initialize audio source pool
            CreateAudioSourcePool(10);
            
            Debug.Log("Audio Manager initialized");
        }
        
        /// <summary>
        /// Create audio source pool untuk performance
        /// </summary>
        private void CreateAudioSourcePool(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject poolObj = new GameObject($"PooledAudioSource_{i}");
                poolObj.transform.SetParent(transform);
                AudioSource source = poolObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                poolObj.SetActive(false);
                audioSourcePool.Enqueue(source);
            }
        }
        
        /// <summary>
        /// Get pooled audio source
        /// </summary>
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                AudioSource source = audioSourcePool.Dequeue();
                source.gameObject.SetActive(true);
                activeAudioSources.Add(source);
                return source;
            }
            
            // Create new jika pool habis
            GameObject newObj = new GameObject("TempAudioSource");
            newObj.transform.SetParent(transform);
            AudioSource newSource = newObj.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            activeAudioSources.Add(newSource);
            return newSource;
        }
        
        /// <summary>
        /// Return audio source ke pool
        /// </summary>
        private void ReturnToPool(AudioSource source)
        {
            if (source != null)
            {
                activeAudioSources.Remove(source);
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
                audioSourcePool.Enqueue(source);
            }
        }
        
        /// <summary>
        /// Cleanup finished audio sources
        /// </summary>
        private void CleanupFinishedAudioSources()
        {
            for (int i = activeAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeAudioSources[i];
                if (source != null && !source.isPlaying)
                {
                    ReturnToPool(source);
                }
            }
        }
        
        /// <summary>
        /// Play music
        /// </summary>
        public void PlayMusic(AudioClip clip, bool fadeIn = true)
        {
            if (clip == null || musicSource == null) return;
            
            if (currentMusic == clip && musicSource.isPlaying) return;
            
            currentMusic = clip;
            
            if (fadeIn && musicSource.isPlaying)
            {
                StartCoroutine(FadeMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
        }
        
        /// <summary>
        /// Fade music transition
        /// </summary>
        private IEnumerator FadeMusic(AudioClip newClip)
        {
            float fadeTime = 1f;
            float currentVolume = musicSource.volume;
            
            // Fade out
            while (musicSource.volume > 0)
            {
                musicSource.volume -= currentVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
            
            // Change clip
            musicSource.clip = newClip;
            musicSource.Play();
            
            // Fade in
            while (musicSource.volume < musicVolume * masterVolume)
            {
                musicSource.volume += (musicVolume * masterVolume) * Time.deltaTime / fadeTime;
                yield return null;
            }
            
            musicSource.volume = musicVolume * masterVolume;
        }
        
        /// <summary>
        /// Play sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || isMuted) return;
            
            // Check cooldown
            string clipName = clip.name;
            if (soundCooldowns.ContainsKey(clipName) && soundCooldowns[clipName] > 0)
                return;
            
            AudioSource source = GetPooledAudioSource();
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volume;
            source.pitch = pitch;
            source.Play();
            
            // Set cooldown
            soundCooldowns[clipName] = 0.1f; // 100ms cooldown
        }
        
        /// <summary>
        /// Play weapon sound
        /// </summary>
        public void PlayWeaponSound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || isMuted) return;
            
            weaponSource.clip = clip;
            weaponSource.volume = weaponVolume * masterVolume * volume;
            weaponSource.Play();
        }
        
        /// <summary>
        /// Play 3D sound at position
        /// </summary>
        public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float maxDistance = 20f)
        {
            if (clip == null || isMuted) return;
            
            AudioSource source = GetPooledAudioSource();
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volume;
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();
        }
        
        /// <summary>
        /// Play ambient sound
        /// </summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null || ambientSource == null) return;
            
            ambientSource.clip = clip;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }
        
        /// <summary>
        /// Stop ambient sound
        /// </summary>
        public void StopAmbient()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }
        
        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                currentMusic = null;
            }
        }
        
        /// <summary>
        /// Update sound cooldowns
        /// </summary>
        private void UpdateSoundCooldowns()
        {
            List<string> keysToUpdate = new List<string>(soundCooldowns.Keys);
            foreach (string key in keysToUpdate)
            {
                if (soundCooldowns[key] > 0)
                {
                    soundCooldowns[key] -= Time.deltaTime;
                    if (soundCooldowns[key] <= 0)
                    {
                        soundCooldowns.Remove(key);
                    }
                }
            }
        }
        
        // Convenience methods untuk common sounds
        public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
        public void PlayBattleMusic() => PlayMusic(battleMusic);
        public void PlayVictoryMusic() => PlayMusic(victoryMusic);
        public void PlayDefeatMusic() => PlayMusic(defeatMusic);
        
        public void PlayButtonClick() => PlaySFX(buttonClickSound);
        public void PlayButtonHover() => PlaySFX(buttonHoverSound, 0.5f);
        public void PlayPanelOpen() => PlaySFX(panelOpenSound);
        public void PlayPanelClose() => PlaySFX(panelCloseSound);
        public void PlayError() => PlaySFX(errorSound);
        
        public void PlayWeaponShoot(string weaponType)
        {
            AudioClip clip = null;
            switch (weaponType.ToLower())
            {
                case "rifle": clip = rifleShootSound; break;
                case "pistol": clip = pistolShootSound; break;
                case "sniper": clip = sniperShootSound; break;
                case "shotgun": clip = shotgunShootSound; break;
            }
            
            if (clip != null)
                PlayWeaponSound(clip);
        }
        
        public void PlayReload() => PlayWeaponSound(reloadSound);
        public void PlayEmptyClip() => PlayWeaponSound(emptyClipSound);
        
        public void PlayHit() => PlaySFX(hitSound);
        public void PlayDeath() => PlaySFX(deathSound);
        public void PlayRespawn() => PlaySFX(respawnSound);
        public void PlayFootstep() => PlaySFX(footstepSound, 0.3f);
        public void PlayJump() => PlaySFX(jumpSound);
        
        public void PlayCountdown() => PlaySFX(countdownSound);
        public void PlayGameStart() => PlaySFX(gameStartSound);
        public void PlayGameEnd() => PlaySFX(gameEndSound);
        public void PlayKill() => PlaySFX(killSound);
        public void PlayScore() => PlaySFX(scoreSound);
        
        // Volume controls
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            SaveAudioSettings();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
            SaveAudioSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveAudioSettings();
        }
        
        public void SetWeaponVolume(float volume)
        {
            weaponVolume = Mathf.Clamp01(volume);
            SaveAudioSettings();
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
            SaveAudioSettings();
        }
        
        /// <summary>
        /// Update all volumes
        /// </summary>
        private void UpdateAllVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
        }
        
        /// <summary>
        /// Mute/unmute audio
        /// </summary>
        public void SetMuted(bool muted)
        {
            isMuted = muted;
            AudioListener.volume = muted ? 0f : masterVolume;
        }
        
        /// <summary>
        /// Save audio settings
        /// </summary>
        private void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("WeaponVolume", weaponVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load audio settings
        /// </summary>
        private void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            weaponVolume = PlayerPrefs.GetFloat("WeaponVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.6f);
            
            UpdateAllVolumes();
        }
        
        /// <summary>
        /// Handle application focus untuk mute on background
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (muteOnBackground)
            {
                AudioListener.pause = !hasFocus;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (muteOnBackground)
            {
                AudioListener.pause = pauseStatus;
            }
        }
    }
}