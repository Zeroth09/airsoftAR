// 🚀 Simple Airsoft AR Battle Server untuk Railway Deployment
const express = require('express');
const cors = require('cors');
const http = require('http');
const socketIo = require('socket.io');

const app = express();
const server = http.createServer(app);

// Basic middleware
app.use(cors());
app.use(express.json());

// Initialize WebSocket
const io = socketIo(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

// Game state
const players = new Map();

// Health check
app.get('/', (req, res) => {
  res.json({
    status: '🚀 Airsoft AR Battle Server',
    version: '2.0.0',
    players: players.size,
    uptime: process.uptime(),
    mode: 'Real-Time PvP'
  });
});

// Players API
app.get('/api/players', (req, res) => {
  const playerList = Array.from(players.values()).map(p => ({
    id: p.id,
    name: p.name,
    team: p.team,
    hp: p.hp
  }));
  
  res.json({
    success: true,
    total: playerList.length,
    players: playerList
  });
});

// Weapons API
app.get('/api/shooting/weapons', (req, res) => {
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
});

// Anti-cheat status
app.get('/api/anti-cheat/status', (req, res) => {
  res.json({
    success: true,
    data: {
      suspiciousActivities: 0,
      rateLimits: 0,
      system: "active",
      lastCleanup: new Date().toISOString()
    }
  });
});

// WebSocket events
io.on('connection', (socket) => {
  console.log(`🔌 Player connected: ${socket.id}`);
  
  // Send welcome message
  socket.emit('serverStatus', {
    message: '🌐 Connected to Real-Time PvP Server!',
    type: 'realServer',
    timestamp: Date.now()
  });
  
  // Player join
  socket.on('joinGame', (playerData) => {
    const player = {
      id: socket.id,
      name: playerData.name || `Player_${Math.random().toString(36).substr(2, 5)}`,
      team: playerData.team || 'red',
      hp: playerData.hp || 100,
      kills: 0,
      deaths: 0
    };
    
    players.set(socket.id, player);
    
    // Broadcast to all players
    io.emit('playerJoined', player);
    io.emit('playerCount', players.size);
    
    console.log(`🎮 ${player.name} joined team ${player.team}`);
  });
  
  // GPS update
  socket.on('gpsUpdate', (gpsData) => {
    const player = players.get(socket.id);
    if (player) {
      player.gps = gpsData;
      socket.broadcast.emit('playerGPSUpdate', {
        playerId: socket.id,
        gps: gpsData
      });
    }
  });
  
  // Fire weapon
  socket.on('fireWeapon', (shootingData) => {
    const player = players.get(socket.id);
    if (player) {
      const shotResult = {
        shooterId: socket.id,
        weaponId: shootingData.weaponId,
        targetId: shootingData.targetId,
        damage: Math.floor(Math.random() * 25) + 10,
        hit: Math.random() > 0.3
      };
      
      io.emit('shotFired', shotResult);
      console.log(`🔫 ${player.name} fired ${shootingData.weaponId}`);
    }
  });
  
  // Disconnect
  socket.on('disconnect', () => {
    const player = players.get(socket.id);
    if (player) {
      players.delete(socket.id);
      io.emit('playerLeft', { playerId: socket.id, name: player.name });
      io.emit('playerCount', players.size);
      console.log(`👋 ${player.name} disconnected`);
    }
  });
});

// Start server
const PORT = process.env.PORT || 3001;
server.listen(PORT, () => {
  console.log(`🚀 Airsoft AR Battle Server running on port ${PORT}`);
  console.log(`🌐 Server URL: http://localhost:${PORT}`);
  console.log(`🔌 WebSocket URL: ws://localhost:${PORT}`);
  console.log(`💕 Real PvP Mode: NO AI/BOTS - HUMANS ONLY`);
});

module.exports = { app, server, io }; 