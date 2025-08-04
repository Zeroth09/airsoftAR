# 🎯 Airsoft AR Battle

Permainan airsoft gun dengan augmented reality untuk battle player vs player yang seru dan menegangkan!

## 🚀 Fitur Utama

- **AR Weapon Tracking** - Deteksi gerakan senjata real-time
- **Multiplayer Battle** - Battle PvP dengan pemain lain
- **GPS Positioning** - Battle area berdasarkan lokasi GPS
- **Team System** - Mode tim dengan strategi
- **Score System** - Tracking hit/miss dan statistik
- **Modern UI/UX** - Interface yang clean dan aesthetic

## 🛠️ Teknologi

- **Unity 2022.3 LTS** - Game engine utama
- **AR Foundation** - Augmented Reality framework
- **Photon Networking** - Multiplayer networking
- **GPS & Location Services** - Positioning system
- **C#** - Programming language

## 📱 Platform Support

- Android (ARCore)
- iOS (ARKit)
- Cross-platform compatibility

## 🎮 Game Modes

1. **Deathmatch** - Free for all battle
2. **Team Battle** - Red vs Blue teams
3. **Capture The Flag** - Strategic objective mode
4. **Survival** - Last man standing

## 🎯 How to Play

1. **Setup Weapon** - Kalibrasi senjata airsoft kamu
2. **Join Battle** - Masuk ke server multiplayer
3. **Aim & Shoot** - Tembak lawan dengan AR targeting
4. **Score Points** - Dapatkan poin untuk setiap hit
5. **Win Battle** - Menjadi pemenang!

## 🏗️ Project Structure

```
AirsoftARBattle/
├── Assets/
│   ├── Scripts/          # C# scripts
│   ├── Prefabs/          # Game objects
│   ├── Materials/         # Visual materials
│   ├── Textures/          # UI textures
│   └── Scenes/           # Game scenes
├── Documentation/         # Project docs
└── Builds/              # Build files
```

## 🎨 UI/UX Features

- Clean modern design
- Intuitive controls
- Real-time feedback
- Smooth animations
- Responsive layout

## 🔧 Development Setup

1. Install Unity 2022.3 LTS
2. Clone repository ini
3. Open project di Unity
4. Setup AR Foundation
5. Configure Photon networking
6. Build untuk target platform

## 📊 Performance

- 60 FPS target
- Low latency networking
- Optimized AR tracking
- Battery efficient

## 🎯 Development Status

- [x] **Project setup** - Unity 2022.3 LTS with AR Foundation
- [x] **AR camera implementation** - Complete AR system with plane detection
- [x] **Weapon tracking system** - Full weapon controller with AR tracking
- [x] **Multiplayer networking** - Photon PUN2 integration with team system
- [x] **UI/UX design** - Modern, clean interface with real-time leaderboard
- [x] **GPS integration** - Real-world positioning system
- [x] **Core Systems Implemented:**
  - ✅ Player Controller with AR camera integration
  - ✅ Team Manager (Red vs Blue teams)
  - ✅ Game Mode Manager (Deathmatch, Team Battle, CTF, Survival)
  - ✅ Audio Manager with spatial audio
  - ✅ Leaderboard & Statistics tracking
  - ✅ GPS Location Manager
  - ✅ AR Plane Detection & Ground tracking
  - ✅ Scene Setup Manager
  - ✅ Prefab Manager with pooling
- [ ] **Testing & optimization** - Performance testing on target devices
- [ ] **Release preparation** - Final build optimization and app store submission

## 🏗️ Project Architecture

```
AirsoftARBattle/
├── Assets/
│   └── Scripts/
│       ├── Core/                    # Core game systems
│       │   ├── ARGameManager.cs     # Main game manager
│       │   ├── TeamManager.cs       # Team system (Red vs Blue)
│       │   ├── GameModeManager.cs   # Game modes implementation
│       │   ├── AudioManager.cs      # Audio system
│       │   ├── GPSLocationManager.cs # GPS positioning
│       │   ├── ARPlaneManager.cs    # AR plane detection
│       │   ├── SceneSetupManager.cs # Scene configuration
│       │   └── PrefabManager.cs     # Prefab management
│       ├── Player/                  # Player systems
│       │   ├── PlayerController.cs  # Player movement & AR camera
│       │   └── PlayerHealth.cs      # Health & damage system
│       ├── Weapons/                 # Weapon systems
│       │   └── WeaponController.cs  # Weapon mechanics & AR tracking
│       ├── UI/                      # User interface
│       │   ├── GameUI.cs           # Main game interface
│       │   └── LeaderboardManager.cs # Statistics & rankings
│       └── Networking/              # Multiplayer networking
│           └── PhotonConfig.cs      # Photon configuration
├── ProjectSettings/                 # Unity project settings
└── Documentation/                   # Additional documentation
```

---

**Dibuat dengan ❤️ untuk para airsoft enthusiast!**

*"Real airsoft, virtual battle, unlimited fun!"* 