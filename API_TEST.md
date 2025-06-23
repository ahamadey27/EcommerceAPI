# Quick API Test

You can test the API registration endpoint directly in the browser console:

1. Open browser to: `https://localhost:7155/swagger`
2. Open browser console (F12)
3. Run this command:

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
    if (response.ok) {
        return response.json();
    } else {
        return response.text();
    }
})
.then(data => console.log('Response:', data))
.catch(error => console.error('Error:', error));
```

If this works, then the API is fine and the issue is with the frontend connection.

## Current Status:
- ✅ API running on `https://localhost:7155` with CORS enabled
- ✅ Frontend running on `https://localhost:7045`
- ✅ CORS configured to allow requests from frontend

Try the registration again from the frontend now!
