// Debug script for login page
console.log('=== LOGIN DEBUG SCRIPT LOADED ===');

// Override console.log to also show in page
const originalConsoleLog = console.log;
console.log = function(...args) {
    originalConsoleLog(...args);
    
    // Try to show logs in page if debug container exists
    const debugContainer = document.getElementById('debug-container');
    if (debugContainer) {
        const logEntry = document.createElement('div');
        logEntry.className = 'alert alert-info alert-sm';
        logEntry.textContent = `[${new Date().toLocaleTimeString()}] ${args.join(' ')}`;
        debugContainer.appendChild(logEntry);
        debugContainer.scrollTop = debugContainer.scrollHeight;
    }
};

// Check what's available immediately
console.log('Window location:', window.location.href);
console.log('API_BASE_URL defined?', typeof API_BASE_URL !== 'undefined');
console.log('login function defined?', typeof login === 'function');

// Monitor script loading
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.type === 'childList') {
            mutation.addedNodes.forEach(function(node) {
                if (node.tagName === 'SCRIPT') {
                    console.log('Script tag added:', node.src || 'inline script');
                }
            });
        }
    });
});

if (document.head) {
    observer.observe(document.head, { childList: true, subtree: true });
}

// Wait for everything to load
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded');
    
    setTimeout(function() {
        console.log('After timeout - API_BASE_URL:', typeof API_BASE_URL !== 'undefined' ? API_BASE_URL : 'UNDEFINED');
        console.log('After timeout - login function:', typeof login);
        
        // Try to find and enhance the login form
        const loginForm = document.getElementById('login-form');
        if (loginForm) {
            console.log('Login form found');
            
            // Add debug container to show logs in page
            const debugContainer = document.createElement('div');
            debugContainer.id = 'debug-container';
            debugContainer.style.cssText = 'max-height: 200px; overflow-y: auto; margin: 10px 0; padding: 10px; border: 1px solid #ccc; background: #f9f9f9; font-family: monospace; font-size: 12px;';
            loginForm.parentNode.insertBefore(debugContainer, loginForm);
            
            // Add quick test button
            const testButton = document.createElement('button');
            testButton.type = 'button';
            testButton.className = 'btn btn-warning btn-sm mb-3';
            testButton.textContent = 'Fill Admin Credentials';
            testButton.onclick = function() {
                document.getElementById('email').value = 'admin@ecommerce.com';
                document.getElementById('password').value = 'Admin123!';
                console.log('Filled admin credentials');
            };
            loginForm.parentNode.insertBefore(testButton, loginForm);
            
        } else {
            console.log('Login form NOT found');
        }
    }, 500);
});

// Monitor for login attempts
if (window.fetch) {
    const originalFetch = window.fetch;
    window.fetch = function(...args) {
        console.log('Fetch call:', args[0]);
        return originalFetch.apply(this, args).then(response => {
            console.log('Fetch response:', response.status, response.statusText);
            return response;
        }).catch(error => {
            console.log('Fetch error:', error);
            throw error;
        });
    };
}
