<<<<<<< HEAD
# 🚀 Airsoft AR Battle - Backend Server

## 📋 Overview
Advanced PvP backend server untuk game Airsoft AR Battle dengan fitur real-time multiplayer, GPS tracking, human detection, shooting mechanics, dan anti-cheat system.

**Live Server:** https://shaky-meeting-production.up.railway.app  
**Frontend:** https://airsoft-ar-battle.netlify.app

---

## ✨ Features

### 🔌 Real-Time Multiplayer
- WebSocket connections dengan Socket.io
- Real-time player synchronization
- Team-based gameplay (Red vs Blue)
- Live chat system
- Player lobby management

### 📍 GPS Tracking System
- Real-time location tracking
- Anti-cheat GPS validation
- Movement speed monitoring
- Nearby player detection
- GPS history logging
- Heatmap generation

### 👥 Human Detection API
- TensorFlow.js integration
- Real-time image analysis
- Anti-cheat detection validation
- Detection history tracking
- Confidence scoring
- Bounding box detection

### 🔫 Shooting Mechanics
- Multiple weapon types (Rifle, Sniper, SMG, Pistol)
- Damage calculation based on distance
- Accuracy system with weapon stats
- Reload mechanics
- Weapon switching
- Anti-cheat rate limiting

### 🛡️ Anti-Cheat System
- Rate limiting untuk semua actions
- GPS movement validation
- Shooting accuracy validation
- Human detection frequency monitoring
- Weapon switching validation
- Damage consistency checking
- Suspicious activity logging

### 📊 Database Management
- File-based JSON storage
- Automatic data cleanup
- Player statistics tracking
- GPS history management
- Shooting logs
- Anti-cheat logs
- Human detection logs

---

## 🚀 Quick Start

### Prerequisites
- Node.js >= 18.0.0
- npm atau yarn

### Installation

1. **Clone repository**
```bash
git clone https://github.com/airsoft-ar/backend.git
cd backend
```

2. **Install dependencies**
```bash
npm install
```

3. **Setup environment variables**
```bash
cp env.example .env
# Edit .env file sesuai kebutuhan
```

4. **Start development server**
```bash
npm run dev
```

5. **Start production server**
```bash
npm start
=======
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
>>>>>>> 36f38c8150d681b367aadb887c3491af99015301
```

---

<<<<<<< HEAD
## 📁 Project Structure

```
backend-server/
├── config/
│   └── database.js          # Database management
├── middleware/
│   └── antiCheat.js         # Anti-cheat system
├── routes/
│   ├── gpsTracking.js       # GPS tracking endpoints
│   ├── humanDetection.js    # Human detection API
│   └── shootingMechanics.js # Shooting mechanics
├── websocket/
│   └── gameSocket.js        # WebSocket manager
├── data/                    # JSON database files
├── logs/                    # Application logs
├── server-deploy.js         # Main server file
├── package.json
├── API_DOCUMENTATION.md     # Complete API docs
└── README.md               # This file
```

---

## 🔧 Configuration

### Environment Variables
```env
NODE_ENV=production
PORT=3000
CORS_ORIGIN=*
RATE_LIMIT_WINDOW=900000
RATE_LIMIT_MAX=100
```

### Database Configuration
- **Storage:** File-based JSON
- **Auto-cleanup:** Every 6 hours
- **Data retention:** 24 hours for GPS, 7 days for logs
- **Max file sizes:** 10MB per request

### Anti-Cheat Settings
- **Max shots per second:** 10
- **Max GPS speed:** 20 m/s (72 km/h)
- **Max accuracy:** 95%
- **Detection frequency limit:** 30 per minute
- **Weapon switch limit:** 5 per 10 seconds

---

## 📡 API Endpoints

### WebSocket Events
- `joinGame` - Join game session
- `gpsUpdate` - Update GPS position
- `fireWeapon` - Fire weapon
- `reloadWeapon` - Reload weapon
- `switchWeapon` - Switch weapon
- `humanDetection` - Send detection data
- `chatMessage` - Send chat message
- `joinTeam` - Join team

### REST API Endpoints

#### GPS Tracking
- `POST /api/gps/update` - Update GPS position
- `GET /api/gps/history/:playerId` - Get GPS history
- `GET /api/gps/nearby/:playerId` - Get nearby players
- `POST /api/gps/validate` - Validate GPS data
- `GET /api/gps/heatmap` - Get heatmap data

#### Human Detection
- `POST /api/detection/analyze` - Analyze image
- `POST /api/detection/stream` - Real-time detection
- `GET /api/detection/history/:playerId` - Get detection history
- `GET /api/detection/stats/:playerId` - Get detection stats

#### Shooting Mechanics
- `POST /api/shooting/fire` - Fire weapon
- `POST /api/shooting/reload` - Reload weapon
- `POST /api/shooting/switch-weapon` - Switch weapon
- `GET /api/shooting/stats/:playerId` - Get shooting stats
- `GET /api/shooting/weapons` - Get weapon configs

#### Anti-Cheat
- `GET /api/anti-cheat/status` - Get anti-cheat status

#### General
- `GET /` - Server status
- `GET /api/players` - Get all players
- `GET /api/status` - Get server status
- `POST /api/admin/cleanup` - Database cleanup

---

## 🛠️ Development

### Running Tests
```bash
# Run linting
npm run lint

# Run tests (if available)
npm test
```

### Code Structure
- **Modular design** dengan separation of concerns
- **Middleware pattern** untuk anti-cheat
- **Event-driven architecture** untuk WebSocket
- **RESTful API** design
- **Error handling** yang comprehensive

### Logging
- **Winston logger** untuk structured logging
- **Error tracking** dengan stack traces
- **Performance monitoring** dengan uptime tracking
- **Anti-cheat logging** untuk security

---

## 🚀 Deployment

### Railway (Recommended)
1. Connect GitHub repository ke Railway
2. Set environment variables
3. Deploy automatically

### Heroku
```bash
# Create Heroku app
heroku create airsoft-ar-backend

# Set environment variables
heroku config:set NODE_ENV=production

# Deploy
git push heroku main
```

### Docker
```dockerfile
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
EXPOSE 3000
CMD ["npm", "start"]
```

### Environment Variables for Production
```env
NODE_ENV=production
PORT=3000
CORS_ORIGIN=https://airsoft-ar-battle.netlify.app
RATE_LIMIT_WINDOW=900000
RATE_LIMIT_MAX=100
```

---

## 📊 Monitoring

### Health Checks
- **Server status:** `GET /`
- **Player count:** `GET /api/players`
- **Anti-cheat status:** `GET /api/anti-cheat/status`

### Metrics
- Active player count
- WebSocket connections
- API request rates
- Anti-cheat violations
- GPS tracking accuracy
- Shooting statistics

### Logs
- **Error logs:** `logs/error.log`
- **Combined logs:** `logs/combined.log`
- **Anti-cheat logs:** Database storage
- **GPS logs:** Database storage

---

## 🔒 Security Features

### Anti-Cheat Protection
- **Rate limiting** untuk semua actions
- **GPS validation** untuk realistic movement
- **Shooting validation** untuk accuracy
- **Detection validation** untuk frequency
- **Weapon validation** untuk switching
- **Damage validation** untuk consistency

### API Security
- **Helmet.js** untuk security headers
- **CORS protection** dengan whitelist
- **Rate limiting** per IP address
- **Input validation** untuk semua endpoints
- **Error handling** tanpa information leakage

### Data Protection
- **Automatic cleanup** untuk old data
- **Encrypted storage** untuk sensitive data
- **Access control** untuk admin endpoints
- **Audit logging** untuk all activities

---

## 🐛 Troubleshooting

### Common Issues

#### WebSocket Connection Failed
```bash
# Check server status
curl https://shaky-meeting-production.up.railway.app

# Check WebSocket endpoint
wscat -c wss://shaky-meeting-production.up.railway.app
```

#### GPS Tracking Issues
```bash
# Validate GPS data
curl -X POST /api/gps/validate \
  -H "Content-Type: application/json" \
  -d '{"playerId":"test","gpsData":{"lat":-6.2088,"lng":106.8456}}'
```

#### Anti-Cheat Warnings
```bash
# Check anti-cheat status
curl https://shaky-meeting-production.up.railway.app/api/anti-cheat/status
```

### Performance Optimization
- **Database cleanup** setiap 6 jam
- **Rate limiting** untuk prevent abuse
- **Compression** untuk API responses
- **Caching** untuk static data
- **Connection pooling** untuk WebSocket

---

## 📈 Performance

### Benchmarks
- **WebSocket connections:** 1000+ concurrent
- **GPS updates:** 1000+ per minute
- **Shooting events:** 500+ per minute
- **Detection requests:** 200+ per minute
- **API responses:** < 100ms average

### Optimization Tips
1. **Use WebSocket** untuk real-time events
2. **Batch GPS updates** untuk reduce requests
3. **Implement caching** untuk weapon configs
4. **Optimize images** sebelum upload
5. **Use compression** untuk large responses

---

## 🤝 Contributing

### Development Workflow
1. Fork repository
2. Create feature branch
3. Make changes
4. Add tests
5. Submit pull request

### Code Standards
- **ESLint** untuk code quality
- **Prettier** untuk formatting
- **JSDoc** untuk documentation
- **TypeScript** (optional)

---

## 📞 Support

### Documentation
- **API Documentation:** `API_DOCUMENTATION.md`
- **WebSocket Events:** See API docs
- **Error Codes:** See API docs

### Contact
- **GitHub Issues:** https://github.com/airsoft-ar/backend/issues
- **Email:** support@airsoft-ar.com
- **Discord:** https://discord.gg/airsoft-ar

---

## 📄 License

MIT License - see LICENSE file for details.

---

## 🎮 Game Features

### Real PvP Only
- ✅ **No AI/Bots** - Real humans only
- ✅ **Real-time multiplayer** - Instant battles
- ✅ **GPS-based gameplay** - Real location tracking
- ✅ **AR integration** - Augmented reality support
- ✅ **Mobile optimized** - Smartphone gameplay
- ✅ **Global accessibility** - Worldwide players

### Advanced Mechanics
- ✅ **Multiple weapons** - Rifle, Sniper, SMG, Pistol
- ✅ **Damage calculation** - Distance-based damage
- ✅ **Accuracy system** - Weapon-specific accuracy
- ✅ **Team battles** - Red vs Blue teams
- ✅ **Live chat** - Team communication
- ✅ **Leaderboards** - Player statistics

**Happy gaming! 💕** 
=======
**Dibuat dengan ❤️ untuk para airsoft enthusiast!**

*"Real airsoft, virtual battle, unlimited fun!"* 
>>>>>>> 36f38c8150d681b367aadb887c3491af99015301
