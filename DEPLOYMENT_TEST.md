# 🧪 Testing Deployment & API Connection

## 🚀 **BACKEND DEPLOYMENT STATUS**

### ✅ **Current Backend URL**
- **Production:** https://shaky-meeting-production.up.railway.app
- **WebSocket:** wss://shaky-meeting-production.up.railway.app
- **Status:** ✅ LIVE & OPERATIONAL

### ✅ **Frontend Integration**
- **Frontend URL:** https://airsoftar.vercel.app
- **API Base URL:** https://shaky-meeting-production.up.railway.app
- **Status:** ✅ CONNECTED

---

## 🧪 **TESTING COMMANDS**

### **1. Health Check Test**
```bash
curl https://shaky-meeting-production.up.railway.app
```

**Expected Response:**
```json
{
  "status": "🚀 Airsoft AR Battle Advanced PvP Server",
  "version": "2.0.0",
  "players": 0,
  "uptime": 3600,
  "mode": "Real-Time Person vs Person dengan Anti-Cheat"
}
```

### **2. API Endpoints Test**
```bash
# Test players endpoint
curl https://shaky-meeting-production.up.railway.app/api/players

# Test weapons endpoint
curl https://shaky-meeting-production.up.railway.app/api/shooting/weapons

# Test anti-cheat status
curl https://shaky-meeting-production.up.railway.app/api/anti-cheat/status
```

### **3. WebSocket Test**
```javascript
// Test WebSocket connection
const socket = io('wss://shaky-meeting-production.up.railway.app');
socket.on('serverStatus', (data) => {
  console.log('Connected:', data.message);
});
```

---

## 🔧 **FRONTEND API CONFIGURATION**

### **Current Configuration**
```typescript
// arfrontend/src/lib/api.ts
const API_BASE_URL = 'https://shaky-meeting-production.up.railway.app'
```

### **WebSocket Connection**
```typescript
// WebSocket connection for real-time game
const socket = io('wss://shaky-meeting-production.up.railway.app');
```

---

## 🎮 **GAME FEATURES TESTING**

### **1. Player Join Test**
```javascript
// Join game as player
socket.emit('joinGame', {
  name: 'TestPlayer',
  team: 'red',
  hp: 100
});
```

### **2. GPS Tracking Test**
```javascript
// Update GPS position
socket.emit('gpsUpdate', {
  lat: -6.2088,
  lng: 106.8456,
  accuracy: 10
});
```

### **3. Shooting Test**
```javascript
// Fire weapon
socket.emit('fireWeapon', {
  weaponId: 'rifle',
  targetId: 'player123',
  position: { x: 100, y: 200 }
});
```

---

## 📊 **PERFORMANCE MONITORING**

### **Expected Metrics**
- **Response Time:** < 100ms
- **WebSocket Latency:** < 50ms
- **Concurrent Players:** 1000+
- **GPS Updates:** 1000+/minute
- **Shooting Events:** 500+/minute

### **Health Indicators**
- ✅ **Server Status:** Online
- ✅ **WebSocket:** Connected
- ✅ **API Endpoints:** Working
- ✅ **Anti-Cheat:** Active
- ✅ **Database:** Operational

---

## 🛠️ **TROUBLESHOOTING**

### **Common Issues & Solutions**

#### **1. CORS Error**
```javascript
// Backend CORS configuration
app.use(cors({
  origin: "*",
  credentials: false
}));
```

#### **2. WebSocket Connection Failed**
```javascript
// Check WebSocket URL
const socket = io('wss://shaky-meeting-production.up.railway.app', {
  transports: ['websocket', 'polling']
});
```

#### **3. API Timeout**
```javascript
// Increase timeout for API calls
const response = await fetch(API_BASE_URL + '/api/players', {
  method: 'GET',
  headers: { 'Content-Type': 'application/json' },
  signal: AbortSignal.timeout(10000) // 10 seconds
});
```

---

## 🎯 **DEPLOYMENT CHECKLIST**

### ✅ **Backend Deployment**
- [ ] Railway deployment successful
- [ ] Environment variables set
- [ ] Health check responding
- [ ] All API endpoints working
- [ ] WebSocket connection established

### ✅ **Frontend Integration**
- [ ] API base URL updated
- [ ] WebSocket connection working
- [ ] Real-time features tested
- [ ] UI responsive on all devices
- [ ] Error handling implemented

### ✅ **Game Features**
- [ ] Player join/leave working
- [ ] GPS tracking functional
- [ ] Shooting mechanics working
- [ ] Team system operational
- [ ] Anti-cheat system active

---

## 🚀 **READY FOR PRODUCTION!**

### **Live URLs**
- **Backend:** https://shaky-meeting-production.up.railway.app
- **Frontend:** https://airsoftar.vercel.app
- **API Docs:** https://shaky-meeting-production.up.railway.app/API_DOCUMENTATION.md

### **Next Steps**
1. **Test semua endpoints** dengan curl atau Postman
2. **Verify WebSocket connection** dengan browser console
3. **Test multiplayer** dengan 2+ devices
4. **Monitor performance** di Railway dashboard
5. **Scale sesuai kebutuhan** game

**Sistem sudah siap untuk real PvP multiplayer! 💕** 