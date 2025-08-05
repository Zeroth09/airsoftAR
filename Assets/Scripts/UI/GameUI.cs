using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AirsoftARBattle.UI
{
    /// <summary>
    /// UI Manager untuk game AR Airsoft Battle
    /// Mengelola semua UI elements dengan design modern dan aesthetic
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Game HUD")]
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        
        [Header("Crosshair")]
        [SerializeField] private GameObject crosshairPanel;
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Color normalCrosshairColor = Color.white;
        [SerializeField] private Color targetCrosshairColor = Color.red;
        
        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsPauseButton;
        [SerializeField] private Button quitPauseButton;
        
        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;
        
        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Button closeSettingsButton;
        
        [Header("Loading")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        
        [Header("Effects")]
        [SerializeField] private CanvasGroup fadePanel;
        [SerializeField] private float fadeDuration = 0.5f;
        
        // Private variables
        private bool isPaused = false;
        private bool isSettingsOpen = false;
        private Coroutine fadeCoroutine;
        
        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            ShowMainMenu();
        }
        
        /// <summary>
        /// Inisialisasi UI
        /// </summary>
        private void InitializeUI()
        {
            // Setup default colors
            if (crosshairImage != null)
            {
                crosshairImage.color = normalCrosshairColor;
            }
            
            // Setup panels
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            
            // Setup fade panel
            if (fadePanel != null)
            {
                fadePanel.alpha = 0f;
                fadePanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Setup button listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            // Pause menu buttons
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            
            if (settingsPauseButton != null)
                settingsPauseButton.onClick.AddListener(OnSettingsButtonClicked);
            
            if (quitPauseButton != null)
                quitPauseButton.onClick.AddListener(OnQuitButtonClicked);
            
            // Game over buttons
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            
            // Settings buttons
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
            
            // Settings sliders
            if (sensitivitySlider != null)
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            
            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        }
        
        /// <summary>
        /// Show main menu
        /// </summary>
        public void ShowMainMenu()
        {
            StartCoroutine(FadeToPanel(mainMenuPanel));
            
            if (titleText != null)
            {
                titleText.text = "🎯 Airsoft AR Battle";
                StartCoroutine(AnimateTitle());
            }
        }
        
        /// <summary>
        /// Show game HUD
        /// </summary>
        public void ShowGameHUD()
        {
            StartCoroutine(FadeToPanel(gameHUDPanel));
            
            if (crosshairPanel != null)
                crosshairPanel.SetActive(true);
        }
        
        /// <summary>
        /// Show pause menu
        /// </summary>
        public void ShowPauseMenu()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// Hide pause menu
        /// </summary>
        public void HidePauseMenu()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
        
        /// <summary>
        /// Show game over
        /// </summary>
        public void ShowGameOver(int finalScore, bool isWinner)
        {
            if (gameOverTitle != null)
            {
                gameOverTitle.text = isWinner ? "🏆 Victory!" : "💀 Game Over";
            }
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }
            
            StartCoroutine(FadeToPanel(gameOverPanel));
        }
        
        /// <summary>
        /// Show settings
        /// </summary>
        public void ShowSettings()
        {
            isSettingsOpen = true;
            settingsPanel.SetActive(true);
            
            // Load current settings
            LoadSettings();
        }
        
        /// <summary>
        /// Hide settings
        /// </summary>
        public void HideSettings()
        {
            isSettingsOpen = false;
            settingsPanel.SetActive(false);
            
            // Save settings
            SaveSettings();
        }
        
        /// <summary>
        /// Show loading screen
        /// </summary>
        public void ShowLoading(string message = "Loading...")
        {
            loadingPanel.SetActive(true);
            
            if (loadingText != null)
                loadingText.text = message;
            
            if (loadingProgressBar != null)
                loadingProgressBar.value = 0f;
        }
        
        /// <summary>
        /// Update loading progress
        /// </summary>
        public void UpdateLoadingProgress(float progress, string message = null)
        {
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;
            
            if (loadingText != null && message != null)
                loadingText.text = message;
        }
        
        /// <summary>
        /// Hide loading screen
        /// </summary>
        public void HideLoading()
        {
            loadingPanel.SetActive(false);
        }
        
        /// <summary>
        /// Update health UI
        /// </summary>
        public void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
                
                // Change color based on health
                Color healthColor = Color.Lerp(Color.red, Color.green, healthBar.value);
                healthBar.fillRect.GetComponent<Image>().color = healthColor;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
            }
        }
        
        /// <summary>
        /// Update ammo UI
        /// </summary>
        public void UpdateAmmoUI(int currentAmmo, int maxAmmo)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
                
                // Change color when low ammo
                if (currentAmmo <= maxAmmo * 0.2f)
                {
                    ammoText.color = Color.red;
                }
                else
                {
                    ammoText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Update score UI
        /// </summary>
        public void UpdateScoreUI(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        /// <summary>
        /// Update time UI
        /// </summary>
        public void UpdateTimeUI(float timeRemaining)
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timeText.text = $"{minutes:00}:{seconds:00}";
                
                // Change color when time is running out
                if (timeRemaining <= 30f)
                {
                    timeText.color = Color.red;
                }
                else
                {
                    timeText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Update player count UI
        /// </summary>
        public void UpdatePlayerCountUI(int currentPlayers, int maxPlayers)
        {
            if (playerCountText != null)
            {
                playerCountText.text = $"Players: {currentPlayers}/{maxPlayers}";
            }
        }
        
        /// <summary>
        /// Update crosshair
        /// </summary>
        public void UpdateCrosshair(bool isTargeting)
        {
            if (crosshairImage != null)
            {
                crosshairImage.color = isTargeting ? targetCrosshairColor : normalCrosshairColor;
                
                // Scale effect when targeting
                float targetScale = isTargeting ? 1.2f : 1f;
                crosshairImage.transform.localScale = Vector3.Lerp(
                    crosshairImage.transform.localScale, 
                    Vector3.one * targetScale, 
                    Time.deltaTime * 5f
                );
            }
        }
        
        /// <summary>
        /// Show damage indicator
        /// </summary>
        public void ShowDamageIndicator()
        {
            StartCoroutine(DamageIndicatorEffect());
        }
        
        /// <summary>
        /// Damage indicator effect
        /// </summary>
        private IEnumerator DamageIndicatorEffect()
        {
            if (fadePanel != null)
            {
                fadePanel.gameObject.SetActive(true);
                fadePanel.color = Color.red;
                fadePanel.alpha = 0.3f;
                
                yield return new WaitForSeconds(0.1f);
                
                fadePanel.alpha = 0f;
                fadePanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Fade to panel
        /// </summary>
        private IEnumerator FadeToPanel(GameObject targetPanel)
        {
            if (fadePanel != null)
            {
                fadePanel.gameObject.SetActive(true);
                
                // Fade out
                float elapsedTime = 0f;
                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                    yield return null;
                }
                
                // Switch panels
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                
                if (targetPanel != null)
                    targetPanel.SetActive(true);
                
                // Fade in
                elapsedTime = 0f;
                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                    yield return null;
                }
                
                fadePanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Animate title
        /// </summary>
        private IEnumerator AnimateTitle()
        {
            if (titleText != null)
            {
                Vector3 originalScale = titleText.transform.localScale;
                
                while (true)
                {
                    // Pulse effect
                    float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.1f;
                    titleText.transform.localScale = originalScale * scale;
                    
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// Load settings
        /// </summary>
        private void LoadSettings()
        {
            if (sensitivitySlider != null)
                sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
            
            if (volumeSlider != null)
                volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            
            if (vibrationToggle != null)
                vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        }
        
        /// <summary>
        /// Save settings
        /// </summary>
        private void SaveSettings()
        {
            if (sensitivitySlider != null)
                PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
            
            if (volumeSlider != null)
                PlayerPrefs.SetFloat("Volume", volumeSlider.value);
            
            if (vibrationToggle != null)
                PlayerPrefs.SetInt("Vibration", vibrationToggle.isOn ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        // Button event handlers
        private void OnPlayButtonClicked()
        {
            ShowLoading("Connecting to server...");
            // Trigger game start
        }
        
        private void OnSettingsButtonClicked()
        {
            ShowSettings();
        }
        
        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        private void OnResumeButtonClicked()
        {
            HidePauseMenu();
        }
        
        private void OnPlayAgainButtonClicked()
        {
            ShowLoading("Starting new game...");
            // Trigger new game
        }
        
        private void OnMainMenuButtonClicked()
        {
            ShowMainMenu();
        }
        
        private void OnCloseSettingsButtonClicked()
        {
            HideSettings();
        }
        
        private void OnSensitivityChanged(float value)
        {
            // Update sensitivity setting
        }
        
        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }
        
        private void OnVibrationChanged(bool value)
        {
            // Update vibration setting
        }
        
        /// <summary>
        /// Check if paused
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }
        
        /// <summary>
        /// Check if settings open
        /// </summary>
        public bool IsSettingsOpen()
        {
            return isSettingsOpen;
        }
    }
} 