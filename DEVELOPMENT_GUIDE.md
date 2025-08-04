# 🔧 AR Airsoft Battle - Development Guide

Panduan lengkap untuk pengembangan dan setup proyek AR Airsoft Battle.

## 📋 Prerequisites

### Software Requirements
- **Unity 2022.3 LTS** atau lebih baru
- **Visual Studio 2022** atau **VS Code** dengan C# extension
- **Android Studio** (untuk Android builds)
- **Xcode** (untuk iOS builds, macOS only)

### Hardware Requirements
- **Development Machine:**
  - 8GB+ RAM
  - 2GB+ storage
  - Dedicated GPU recommended

- **Target Devices:**
  - **Android:** ARCore compatible device (Android 7.0+)
  - **iOS:** ARKit compatible device (iOS 11.0+)
  - Gyroscope, accelerometer, camera

## 🚀 Setup Project

### 1. Clone Repository
```bash
git clone https://github.com/your-username/airsoft-ar-battle.git
cd airsoft-ar-battle
```

### 2. Unity Setup
1. Open Unity Hub
2. Add project dari folder yang di-clone
3. Pastikan Unity 2022.3 LTS terinstall
4. Open project

### 3. Package Dependencies
Packages yang dibutuhkan (sudah included di project):
- **AR Foundation** (4.2.7+)
- **ARCore XR Plugin** (untuk Android)
- **ARKit XR Plugin** (untuk iOS)
- **PUN2 - Photon Unity Networking** (2.43+)
- **TextMeshPro**
- **Input System**

### 4. Platform Setup

#### Android Setup
1. Switch ke Android platform: `File > Build Settings > Android > Switch Platform`
2. Setup XR settings: `Edit > Project Settings > XR Plug-in Management`
3. Enable ARCore untuk Android
4. Configure Player settings:
   - Company Name & Product Name
   - Bundle Identifier (contoh: com.yourcompany.airsoftarbattle)
   - Minimum API Level: Android 7.0 (API 24)
   - Target API Level: Android 13 (API 33)

#### iOS Setup
1. Switch ke iOS platform: `File > Build Settings > iOS > Switch Platform`
2. Enable ARKit untuk iOS
3. Configure Player settings:
   - Bundle Identifier
   - Deployment Target: iOS 11.0+
   - Camera Usage Description

## 🏗️ Project Structure

### Core Systems

#### 1. AR Game Manager (`ARGameManager.cs`)
Manager utama yang mengelola:
- AR session lifecycle
- Game state management
- Player spawning
- Scene transitions

#### 2. Team Manager (`TeamManager.cs`)
Sistem team battle:
- Red vs Blue team assignment
- Team balancing
- Team-specific spawning
- Score tracking per team

#### 3. Game Mode Manager (`GameModeManager.cs`)
Implementasi game modes:
- **Deathmatch:** Free-for-all battle
- **Team Battle:** Red vs Blue teams
- **Capture The Flag:** Objective-based gameplay
- **Survival:** Last man standing

#### 4. Audio Manager (`AudioManager.cs`)
Sistem audio lengkap:
- Music management dengan fade transitions
- SFX dengan pooling system
- 3D spatial audio
- Volume controls
- Background muting

#### 5. GPS Location Manager (`GPSLocationManager.cs`)
Real-world positioning:
- GPS coordinate tracking
- Battle area boundaries
- World-to-local coordinate conversion
- Location-based spawning

### Player Systems

#### Player Controller (`PlayerController.cs`)
- AR camera integration
- Movement dengan GPS positioning
- Input handling (touch/mouse)
- Network synchronization

#### Player Health (`PlayerHealth.cs`)
- Health management
- Damage system
- Respawning mechanics
- Visual feedback

### Weapon System

#### Weapon Controller (`WeaponController.cs`)
- AR weapon tracking
- Raycast shooting mechanics
- Different weapon types (Rifle, Pistol, Sniper, Shotgun)
- Ammo management
- Network synchronization

### UI System

#### Game UI (`GameUI.cs`)
- Modern, clean interface
- Real-time HUD (health, ammo, score)
- Menu system dengan smooth transitions
- Crosshair dan targeting

#### Leaderboard Manager (`LeaderboardManager.cs`)
- Real-time statistics tracking
- Player rankings
- Team statistics
- End-game summary

## 🛠️ Development Workflow

### 1. Scene Setup
Gunakan `SceneSetupManager.cs` untuk setup otomatis:

```csharp
// Di Inspector, klik "Setup Scene" atau gunakan context menu
SceneSetupManager sceneSetup = FindObjectOfType<SceneSetupManager>();
sceneSetup.SetupScene();
```

Ini akan setup:
- AR Session & AR Session Origin
- AR Camera dengan background
- Game managers
- UI Canvas
- Event System

### 2. Testing Workflow

#### Editor Testing
1. Setup mock AR environment
2. Use fallback camera untuk testing non-AR features
3. Test networking dengan multiple instances

#### Device Testing
1. Build ke device dengan AR support
2. Test GPS functionality outdoor
3. Test multiplayer dengan multiple devices

### 3. Debugging

#### Common Issues
- **AR tidak start:** Check ARCore/ARKit installation
- **GPS tidak bekerja:** Enable location permissions
- **Network issues:** Check Photon App ID
- **Performance issues:** Check profiler, optimize rendering

#### Debug Tools
- Unity Console untuk logs
- AR Foundation debug visualization
- Photon Statistics GUI
- GPS coordinates display

## 📦 Building

### Android Build

1. **Setup Build Settings:**
   ```
   File > Build Settings
   - Platform: Android
   - Texture Compression: ASTC
   - Development Build: Checked (untuk debugging)
   ```

2. **Player Settings:**
   ```
   Edit > Project Settings > Player
   - Company Name: Your Company
   - Product Name: Airsoft AR Battle
   - Bundle Identifier: com.yourcompany.airsoftarbattle
   - Version: 1.0.0
   - Bundle Version Code: 1
   - Minimum API Level: Android 7.0 (API 24)
   - Target API Level: Android 13 (API 33)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64 (recommended)
   ```

3. **XR Settings:**
   ```
   Edit > Project Settings > XR Plug-in Management
   - Provider: ARCore (Android)
   - Initialize XR on Startup: Checked
   ```

4. **Build:**
   - Click "Build" untuk APK
   - Atau "Build and Run" untuk install langsung

### iOS Build

1. **Setup Build Settings:**
   ```
   File > Build Settings
   - Platform: iOS
   - Development Build: Checked
   ```

2. **Player Settings:**
   ```
   - Bundle Identifier: com.yourcompany.airsoftarbattle
   - Deployment Target: 11.0
   - Architecture: ARM64
   - Camera Usage Description: "This app uses camera for AR gameplay"
   - Location Usage Description: "This app uses location for real-world positioning"
   ```

3. **XR Settings:**
   ```
   - Provider: ARKit (iOS)
   ```

4. **Build:**
   - Build to Xcode project
   - Open di Xcode untuk final build dan deployment

## 🔧 Configuration

### Photon Networking Setup

1. **Get Photon App ID:**
   - Register di [Photon Engine](https://www.photonengine.com/)
   - Create PUN2 application
   - Copy App ID

2. **Configure di Unity:**
   ```
   Window > Photon Unity Networking > PUN Wizard
   - Setup Photon
   - Enter App ID
   ```

3. **Network Settings:**
   ```csharp
   // Di PhotonConfig.cs
   private string appId = "your-photon-app-id";
   private int maxPlayersPerRoom = 8;
   ```

### GPS Configuration

```csharp
// Di GPSLocationManager.cs
[SerializeField] private float accuracyThreshold = 10f; // Meter
[SerializeField] private float maxBattleRadius = 500f; // Meter
[SerializeField] private bool useDynamicCenter = true;
```

### Audio Configuration

```csharp
// Di AudioManager.cs
[SerializeField] private float masterVolume = 1f;
[SerializeField] private float musicVolume = 0.8f;
[SerializeField] private float sfxVolume = 1f;
```

## 🎮 Gameplay Features

### Game Modes

#### 1. Deathmatch
- Free-for-all battle
- First to reach score limit wins
- Individual scoring

#### 2. Team Battle
- Red vs Blue teams
- Team scoring
- Auto team balancing

#### 3. Capture The Flag
- Team-based objective gameplay
- Capture enemy flag
- Strategic positioning

#### 4. Survival
- Last man standing
- Limited lives
- Shrinking battle area

### AR Features

- **Real-world positioning** dengan GPS
- **Ground plane detection** untuk spawning
- **AR weapon tracking** dengan device camera
- **Mixed reality gameplay** di real world

### Multiplayer Features

- **8 players** maximum per room
- **Real-time synchronization** player movement & actions
- **Team-based communication**
- **Cross-platform compatibility** (Android & iOS)

## 🔒 Security & Performance

### Performance Optimization

- **Object pooling** untuk effects dan bullets
- **LOD system** untuk 3D models
- **Occlusion culling** untuk distant objects
- **Texture compression** untuk mobile
- **60 FPS target** dengan dynamic quality adjustment

### Security Considerations

- **Anti-cheat measures** di server side
- **Input validation** untuk network messages
- **Secure communication** dengan Photon Cloud
- **No sensitive data** stored locally

## 📱 Platform Specific

### Android Specific

- **Permissions:** Camera, Location, Internet
- **Target API 33** untuk Play Store compatibility
- **ARCore session** management
- **Battery optimization** exemption

### iOS Specific

- **Info.plist entries** untuk Camera dan Location
- **ARKit session** management
- **App Store guidelines** compliance
- **TestFlight** untuk beta testing

## 🐛 Troubleshooting

### Common Issues

1. **AR tidak initialize:**
   - Check device compatibility
   - Verify permissions granted
   - Restart AR session

2. **GPS tidak accurate:**
   - Test outdoor dengan clear sky
   - Wait untuk GPS fix
   - Check location permissions

3. **Network lag:**
   - Check internet connection
   - Monitor Photon statistics
   - Optimize network messages

4. **Performance issues:**
   - Use Unity Profiler
   - Check draw calls
   - Optimize textures

### Debug Commands

```csharp
// Enable debug logging
Debug.unityLogger.logEnabled = true;

// Show AR debug visualization
arSessionOrigin.GetComponent<ARPlaneManager>().enabled = true;

// Display network statistics
PhotonNetwork.EnableCloseConnection = true;
```

## 📚 Resources

### Documentation
- [Unity AR Foundation Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/manual/index.html)
- [Photon PUN2 Documentation](https://doc.photonengine.com/pun2/current/getting-started/pun-intro)
- [ARCore Developer Guide](https://developers.google.com/ar)
- [ARKit Developer Documentation](https://developer.apple.com/documentation/arkit)

### Tools
- [Unity Profiler](https://docs.unity3d.com/Manual/ProfilerWindow.html)
- [Android Logcat](https://docs.unity3d.com/Manual/android-logcat.html)
- [Xcode Instruments](https://developer.apple.com/xcode/features/)

### Community
- [Unity AR Foundation Forum](https://forum.unity.com/forums/ar-vr.80/)
- [Photon Community](https://forum.photonengine.com/)
- [Unity Discord](https://discord.gg/unity)

---

## 📞 Support

Untuk pertanyaan development atau issues:

1. Check dokumentasi di atas
2. Search di Unity Forums
3. Create issue di GitHub repository
4. Contact development team

**Happy coding! 🎯**