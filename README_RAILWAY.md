# 🚀 Railway Dashboard Deployment Guide

## 📋 **STEP-BY-STEP DEPLOYMENT**

### **1. Buka Railway Dashboard**
- Kunjungi: https://railway.app/dashboard
- Login dengan GitHub account

### **2. Create New Project**
- Klik "New Project"
- Pilih "Deploy from GitHub repo"
- Atau pilih "Deploy from template"

### **3. Upload Files Manual**
Jika tidak punya GitHub repo:
1. Klik "New Project"
2. Pilih "Deploy from template"
3. Pilih "Node.js"
4. Upload semua files dari folder `backend-server/`

### **4. Set Environment Variables**
Di Railway dashboard, set environment variables:
```env
NODE_ENV=production
PORT=3000
CORS_ORIGIN=*
```

### **5. Deploy**
- Railway akan otomatis detect `package.json`
- Build process akan berjalan
- Tunggu deployment selesai

### **6. Get Domain**
- Setelah deploy berhasil
- Railway akan kasih domain URL
- Catat URL tersebut untuk frontend

## 📁 **FILES YANG PERLU DIUPLOAD**

```
backend-server/
├── simple-server.js          # Main server file
├── package.json              # Dependencies
├── Procfile                  # Process configuration
├── railway.toml             # Railway config
└── README_RAILWAY.md        # This file
```

## 🧪 **TESTING SETELAH DEPLOY**

### **1. Health Check**
```bash
curl https://your-railway-domain.up.railway.app
```

### **2. API Endpoints**
```bash
# Players API
curl https://your-railway-domain.up.railway.app/api/players

# Weapons API
curl https://your-railway-domain.up.railway.app/api/shooting/weapons

# Anti-cheat API
curl https://your-railway-domain.up.railway.app/api/anti-cheat/status
```

### **3. WebSocket Test**
```javascript
// Test WebSocket connection
const socket = io('wss://your-railway-domain.up.railway.app');
socket.on('serverStatus', (data) => {
  console.log('Connected:', data.message);
});
```

## 🎯 **EXPECTED RESPONSE**

### **Health Check**
```json
{
  "status": "🚀 Airsoft AR Battle Server",
  "version": "2.0.0",
  "players": 0,
  "uptime": 123.45,
  "mode": "Real-Time PvP"
}
```

### **Players API**
```json
{
  "success": true,
  "total": 0,
  "players": []
}
```

### **Weapons API**
```json
{
  "success": true,
  "weapons": {
    "rifle": { "name": "Rifle", "damage": 25, ... },
    "sniper": { "name": "Sniper", "damage": 100, ... },
    "smg": { "name": "SMG", "damage": 15, ... },
    "pistol": { "name": "Pistol", "damage": 20, ... }
  }
}
```

## 🚀 **NEXT STEPS**

Setelah backend deploy berhasil:
1. **Update frontend** dengan URL backend yang baru
2. **Test integration** antara frontend dan backend
3. **Deploy frontend** ke Vercel
4. **Test multiplayer** dengan 2+ devices

**Happy deploying! 💕** 