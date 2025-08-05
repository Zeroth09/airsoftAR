// 🧪 Local Testing Script untuk Airsoft AR Battle Backend
// Test server local sebelum deployment ke Railway

const express = require('express');
const http = require('http');
const cors = require('cors');
const helmet = require('helmet');
const compression = require('compression');
const rateLimit = require('express-rate-limit');
const winston = require('winston');

// Import custom modules
const database = require('./config/database');
const antiCheat = require('./middleware/antiCheat');
const GameSocketManager = require('./websocket/gameSocket');

// Import routes
const gpsTrackingRoutes = require('./routes/gpsTracking');
const humanDetectionRoutes = require('./routes/humanDetection');
const shootingMechanicsRoutes = require('./routes/shootingMechanics');

const app = express();
const server = http.createServer(app);

// Initialize WebSocket manager
const gameSocket = new GameSocketManager(server);

// Security middleware
app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'", "'unsafe-inline'"],
      imgSrc: ["'self'", "data:", "https:"],
      connectSrc: ["'self'", "wss:", "ws:"]
    }
  }
}));

// Compression middleware
app.use(compression());

// Rate limiting
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limit each IP to 100 requests per windowMs
  message: {
    success: false,
    message: 'Terlalu banyak request dari IP ini, coba lagi nanti'
  }
});
app.use('/api/', limiter);

// CORS middleware
app.use(cors({
  origin: "*",
  credentials: false
}));
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Logger configuration
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  defaultMeta: { service: 'airsoft-ar-backend' },
  transports: [
    new winston.transports.Console({
      format: winston.format.simple()
    })
  ]
});

// Health check endpoint
app.get("/", (req, res) => {
  res.json({
    status: "🚀 Airsoft AR Battle Advanced PvP Server",
    version: "2.0.0",
    players: gameSocket.getActiveConnections().size,
    uptime: process.uptime(),
    mode: "Real-Time Person vs Person dengan Anti-Cheat",
    features: [
      "GPS Tracking dengan Validasi",
      "Human Detection API",
      "Shooting Mechanics",
      "Anti-Cheat System",
      "Real-Time WebSocket",
      "No AI/Bots - Real Humans Only"
    ],
    endpoints: {
      gps: "/api/gps/*",
      detection: "/api/detection/*",
      shooting: "/api/shooting/*",
      websocket: "ws://localhost:3001"
    }
  });
});

// API endpoints
app.get("/api/players", (req, res) => {
  const activeConnections = gameSocket.getActiveConnections();
  const players = Array.from(activeConnections.values());
  
  res.json({
    success: true,
    total: players.length,
    red: players.filter(p => p.team === 'red').length,
    blue: players.filter(p => p.team === 'blue').length,
    players: players.map(p => ({
      name: p.name,
      team: p.team,
      isAlive: p.isAlive,
      kills: p.kills,
      deaths: p.deaths,
      hp: p.hp,
      weapon: p.weapon
    }))
  });
});

app.get("/api/status", (req, res) => {
  const activeConnections = gameSocket.getActiveConnections();
  
  res.json({
    success: true,
    server: "online",
    players: activeConnections.size,
    teams: {
      red: Array.from(activeConnections.values()).filter(p => p.team === 'red').length,
      blue: Array.from(activeConnections.values()).filter(p => p.team === 'blue').length
    },
    uptime: Math.floor(process.uptime()),
    antiCheat: {
      active: true,
      suspiciousActivities: antiCheat.suspiciousActivities.size
    }
  });
});

// Mount route modules
app.use('/api/gps', gpsTrackingRoutes);
app.use('/api/detection', humanDetectionRoutes);
app.use('/api/shooting', shootingMechanicsRoutes);

// Anti-cheat monitoring endpoint
app.get("/api/anti-cheat/status", (req, res) => {
  try {
    const suspiciousCount = antiCheat.suspiciousActivities.size;
    const rateLimitCount = antiCheat.rateLimits.size;
    
    res.json({
      success: true,
      data: {
        suspiciousActivities: suspiciousCount,
        rateLimits: rateLimitCount,
        system: "active",
        lastCleanup: new Date().toISOString()
      }
    });
  } catch (error) {
    logger.error('Anti-cheat status error:', error);
    res.status(500).json({
      success: false,
      message: 'Error retrieving anti-cheat status'
    });
  }
});

// Weapons configuration endpoint
app.get("/api/shooting/weapons", (req, res) => {
  try {
    const weapons = {
      rifle: {
        name: "Rifle",
        damage: 25,
        accuracy: 85,
        range: 100,
        fireRate: 600,
        reloadTime: 3000,
        ammo: 30,
        maxAmmo: 30
      },
      sniper: {
        name: "Sniper",
        damage: 100,
        accuracy: 95,
        range: 200,
        fireRate: 1200,
        reloadTime: 4000,
        ammo: 5,
        maxAmmo: 5
      },
      smg: {
        name: "SMG",
        damage: 15,
        accuracy: 75,
        range: 50,
        fireRate: 900,
        reloadTime: 2000,
        ammo: 25,
        maxAmmo: 25
      },
      pistol: {
        name: "Pistol",
        damage: 20,
        accuracy: 80,
        range: 30,
        fireRate: 500,
        reloadTime: 1500,
        ammo: 12,
        maxAmmo: 12
      }
    };
    
    res.json({
      success: true,
      weapons: weapons
    });
  } catch (error) {
    logger.error('Weapons endpoint error:', error);
    res.status(500).json({
      success: false,
      message: 'Error retrieving weapons configuration'
    });
  }
});

// Error handling middleware
app.use((err, req, res, next) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({
    success: false,
    message: 'Internal server error',
    error: process.env.NODE_ENV === 'development' ? err.message : 'Something went wrong'
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    success: false,
    message: 'Endpoint tidak ditemukan',
    availableEndpoints: [
      'GET /',
      'GET /api/players',
      'GET /api/status',
      'GET /api/anti-cheat/status',
      'GET /api/shooting/weapons',
      'WebSocket: ws://localhost:3001'
    ]
  });
});

// Start server
const PORT = 3001;
server.listen(PORT, () => {
  console.log(`🚀 Airsoft AR Battle Local Test Server running on port ${PORT}`);
  console.log(`🌐 Server URL: http://localhost:${PORT}`);
  console.log(`🔌 WebSocket URL: ws://localhost:${PORT}`);
  console.log(`🛡️ Anti-Cheat System: ACTIVE`);
  console.log(`📍 GPS Tracking: ENABLED`);
  console.log(`👥 Human Detection: ENABLED`);
  console.log(`🔫 Shooting Mechanics: ENABLED`);
  console.log(`💕 Real PvP Mode: NO AI/BOTS - HUMANS ONLY`);
  console.log(`\n🧪 Testing endpoints:`);
  console.log(`   Health: http://localhost:${PORT}/`);
  console.log(`   Players: http://localhost:${PORT}/api/players`);
  console.log(`   Weapons: http://localhost:${PORT}/api/shooting/weapons`);
  console.log(`   Anti-Cheat: http://localhost:${PORT}/api/anti-cheat/status`);
});

module.exports = { app, server, gameSocket }; 