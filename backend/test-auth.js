// Simple Node.js test script to verify JWT authentication
const http = require('http');

// Test function to make HTTP requests
function makeRequest(options, data) {
    return new Promise((resolve, reject) => {
        const req = http.request(options, (res) => {
            let body = '';
            res.on('data', (chunk) => {
                body += chunk;
            });
            res.on('end', () => {
                resolve({
                    statusCode: res.statusCode,
                    headers: res.headers,
                    body: body
                });
            });
        });
        
        req.on('error', (err) => {
            reject(err);
        });
        
        if (data) {
            req.write(data);
        }
        req.end();
    });
}

async function testAuth() {
    console.log('🔐 Testing JWT Authentication...\n');
    
    try {
        // Step 1: Test login to get JWT token
        console.log('1️⃣ Testing login...');
        const loginData = JSON.stringify({
            username: 'testuser',
            password: 'testpass'
        });
        
        const loginOptions = {
            hostname: 'localhost',
            port: 8080,
            path: '/api/auth/login',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': Buffer.byteLength(loginData)
            }
        };
        
        const loginResponse = await makeRequest(loginOptions, loginData);
        console.log(`Status: ${loginResponse.statusCode}`);
        
        if (loginResponse.statusCode === 200) {
            const loginResult = JSON.parse(loginResponse.body);
            const token = loginResult.accessToken;
            console.log('✅ Login successful! Token received.');
            console.log(`Token: ${token.substring(0, 50)}...\n`);
            
            // Step 2: Test protected endpoint without token (should fail)
            console.log('2️⃣ Testing protected endpoint WITHOUT token...');
            const protectedOptions = {
                hostname: 'localhost',
                port: 8080,
                path: '/api/groups',
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(JSON.stringify({ name: 'Test Group', isPublic: true }))
                }
            };
            
            const protectedResponse = await makeRequest(protectedOptions);
            console.log(`Status: ${protectedResponse.statusCode}`);
            
            if (protectedResponse.statusCode === 401) {
                console.log('✅ Protected endpoint correctly rejects unauthenticated request.\n');
            } else {
                console.log('❌ Protected endpoint should reject unauthenticated request!\n');
            }
            
            // Step 3: Test protected endpoint with token (should work)
            console.log('3️⃣ Testing protected endpoint WITH token...');
            const authOptions = {
                hostname: 'localhost',
                port: 8080,
                path: '/api/groups',
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                    'Content-Length': Buffer.byteLength(JSON.stringify({ name: 'Test Group', isPublic: true }))
                }
            };
            
            const authResponse = await makeRequest(authOptions);
            console.log(`Status: ${authResponse.statusCode}`);
            
            if (authResponse.statusCode === 200) {
                console.log('✅ Protected endpoint accepts authenticated request!');
                console.log('Response:', JSON.parse(authResponse.body));
            } else {
                console.log('❌ Protected endpoint should accept authenticated request!');
                console.log('Response:', authResponse.body);
            }
            
        } else {
            console.log('❌ Login failed!');
            console.log('Response:', loginResponse.body);
        }
        
    } catch (error) {
        console.error('❌ Error:', error.message);
        console.log('\n💡 Make sure the API server is running on localhost:8080');
        console.log('   and you have a test user with username: testuser, password: testpass');
    }
}

testAuth();
