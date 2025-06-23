# Debugging Guide: Frontend-API Connection

## Current Setup ✅
- **API**: `https://localhost:7155` (running)
- **Frontend**: `https://localhost:7045` (running)

## Testing Steps

### 1. Basic API Test
First, verify the API is accessible:
- Open browser to: `https://localhost:7155/swagger`
- You should see the Swagger UI with all endpoints

### 2. Test API Registration Endpoint Directly
Open browser console (F12) and run:
```javascript
fetch('https://localhost:7155/api/auth/register', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        email: 'test@example.com',
        password: 'TestPassword123!'
    })
})
.then(response => {
    console.log('Status:', response.status);
    return response.text();
})
.then(data => console.log('Response:', data))
.catch(error => console.error('Error:', error));
```

### 3. Test Frontend Registration
1. Open browser to: `https://localhost:7045`
2. Click "Register" 
3. Fill out the form
4. Open browser console (F12) to see any JavaScript errors
5. Check Network tab for failed requests

## Common Issues & Solutions

### CORS Issues
If you see CORS errors, the API needs to allow requests from the frontend origin.

### SSL Certificate Issues
Both services use self-signed certificates. You may need to:
1. Visit `https://localhost:7155` first and accept the certificate
2. Visit `https://localhost:7045` and accept the certificate

### Network Errors
Check if:
- Both services are running
- Firewall isn't blocking the ports
- Antivirus isn't interfering

## API Endpoints for Testing
- **Register**: `POST https://localhost:7155/api/auth/register`
- **Login**: `POST https://localhost:7155/api/auth/login`
- **Products**: `GET https://localhost:7155/api/products`

## Debug JavaScript Issues
In the frontend, open browser console and check:
1. Any JavaScript errors
2. Network tab for failed requests
3. Application tab → Local Storage for JWT token storage

## Next Steps
Once you identify the specific error:
1. Share the exact error message from browser console
2. Share any network request details from the Network tab
3. Let me know which step fails in the testing process
