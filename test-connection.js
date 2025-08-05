// 🧪 Testing Script untuk Airsoft AR Battle Backend
// Memverifikasi koneksi API dan WebSocket

const https = require('https');
const WebSocket = require('ws');

const BACKEND_URL = 'https://shaky-meeting-production.up.railway.app';
const WS_URL = 'wss://shaky-meeting-production.up.railway.app';

console.log('🧪 Testing Airsoft AR Battle Backend Connection...\n');

// Test 1: Health Check
async function testHealthCheck() {
  console.log('1️⃣ Testing Health Check...');
  
  return new Promise((resolve) => {
    const req = https.get(BACKEND_URL, (res) => {
      let data = '';
      
      res.on('data', (chunk) => {
        data += chunk;
      });
      
      res.on('end', () => {
        try {
          const response = JSON.parse(data);
          console.log('✅ Health Check SUCCESS');
          console.log('   Status:', response.status);
          console.log('   Version:', response.version);
          console.log('   Players:', response.players);
          console.log('   Mode:', response.mode);
          resolve(true);
        } catch (error) {
          console.log('❌ Health Check FAILED - Invalid JSON response');
          console.log('   Response:', data);
          resolve(false);
        }
      });
    });
    
    req.on('error', (error) => {
      console.log('❌ Health Check FAILED - Connection error');
      console.log('   Error:', error.message);
      resolve(false);
    });
    
    req.setTimeout(10000, () => {
      console.log('❌ Health Check FAILED - Timeout');
      req.destroy();
      resolve(false);
    });
  });
}

// Test 2: API Endpoints
async function testAPIEndpoints() {
  console.log('\n2️⃣ Testing API Endpoints...');
  
  const endpoints = [
    '/api/players',
    '/api/shooting/weapons',
    '/api/anti-cheat/status'
  ];
  
  let successCount = 0;
  
  for (const endpoint of endpoints) {
    try {
      const response = await fetch(`${BACKEND_URL}${endpoint}`);
      if (response.ok) {
        console.log(`✅ ${endpoint} - SUCCESS`);
        successCount++;
      } else {
        console.log(`❌ ${endpoint} - FAILED (${response.status})`);
      }
    } catch (error) {
      console.log(`❌ ${endpoint} - FAILED (${error.message})`);
    }
  }
  
  console.log(`\n📊 API Endpoints: ${successCount}/${endpoints.length} working`);
  return successCount === endpoints.length;
}

// Test 3: WebSocket Connection
function testWebSocket() {
  console.log('\n3️⃣ Testing WebSocket Connection...');
  
  return new Promise((resolve) => {
    const ws = new WebSocket(WS_URL);
    
    ws.on('open', () => {
      console.log('✅ WebSocket Connection SUCCESS');
      
      // Send test message
      ws.send(JSON.stringify({
        type: 'test',
        message: 'Testing WebSocket connection'
      }));
      
      setTimeout(() => {
        ws.close();
        resolve(true);
      }, 2000);
    });
    
    ws.on('message', (data) => {
      try {
        const message = JSON.parse(data);
        console.log('📨 WebSocket Message Received:', message);
      } catch (error) {
        console.log('📨 WebSocket Raw Message:', data.toString());
      }
    });
    
    ws.on('error', (error) => {
      console.log('❌ WebSocket Connection FAILED');
      console.log('   Error:', error.message);
      resolve(false);
    });
    
    ws.on('close', () => {
      console.log('🔌 WebSocket Connection Closed');
    });
    
    // Timeout after 10 seconds
    setTimeout(() => {
      if (ws.readyState === WebSocket.CONNECTING) {
        console.log('❌ WebSocket Connection FAILED - Timeout');
        ws.terminate();
        resolve(false);
      }
    }, 10000);
  });
}

// Test 4: Performance Test
async function testPerformance() {
  console.log('\n4️⃣ Testing Performance...');
  
  const startTime = Date.now();
  
  try {
    const response = await fetch(`${BACKEND_URL}/api/players`);
    const endTime = Date.now();
    const responseTime = endTime - startTime;
    
    console.log(`⏱️ Response Time: ${responseTime}ms`);
    
    if (responseTime < 1000) {
      console.log('✅ Performance GOOD (< 1 second)');
      return true;
    } else if (responseTime < 3000) {
      console.log('⚠️ Performance ACCEPTABLE (< 3 seconds)');
      return true;
    } else {
      console.log('❌ Performance POOR (> 3 seconds)');
      return false;
    }
  } catch (error) {
    console.log('❌ Performance Test FAILED');
    console.log('   Error:', error.message);
    return false;
  }
}

// Main test function
async function runAllTests() {
  console.log('🚀 Starting Airsoft AR Battle Backend Tests...\n');
  
  const results = {
    healthCheck: false,
    apiEndpoints: false,
    webSocket: false,
    performance: false
  };
  
  // Run all tests
  results.healthCheck = await testHealthCheck();
  results.apiEndpoints = await testAPIEndpoints();
  results.webSocket = await testWebSocket();
  results.performance = await testPerformance();
  
  // Summary
  console.log('\n📊 TEST RESULTS SUMMARY');
  console.log('========================');
  console.log(`Health Check:     ${results.healthCheck ? '✅ PASS' : '❌ FAIL'}`);
  console.log(`API Endpoints:    ${results.apiEndpoints ? '✅ PASS' : '❌ FAIL'}`);
  console.log(`WebSocket:        ${results.webSocket ? '✅ PASS' : '❌ FAIL'}`);
  console.log(`Performance:      ${results.performance ? '✅ PASS' : '❌ FAIL'}`);
  
  const passedTests = Object.values(results).filter(Boolean).length;
  const totalTests = Object.keys(results).length;
  
  console.log(`\n🎯 Overall: ${passedTests}/${totalTests} tests passed`);
  
  if (passedTests === totalTests) {
    console.log('🎉 ALL TESTS PASSED! Backend is ready for production!');
    console.log('💕 Your Airsoft AR Battle server is working perfectly!');
  } else {
    console.log('⚠️ Some tests failed. Please check the issues above.');
  }
  
  return results;
}

// Run tests if this file is executed directly
if (require.main === module) {
  runAllTests().catch(console.error);
}

module.exports = { runAllTests }; 