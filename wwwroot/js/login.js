document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('login-form');
    const alertContainer = document.getElementById('alert-container');
    const debugPanel = document.getElementById('debug-panel');

    // Bulletproof API base URL
    let apiBase = window.location.origin;
    if (apiBase.endsWith('/')) apiBase = apiBase.slice(0, -1);
    apiBase += '/api/auth/login';

    function showDebug(msg) {
        if (debugPanel) {
            debugPanel.style.display = 'block';
            debugPanel.innerHTML += `<div>${new Date().toLocaleTimeString()} - ${msg}</div>`;
        }
        console.log(msg);
    }

    if (loginForm) {
        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const btn = document.getElementById('login-btn');
            const spinner = btn.querySelector('.spinner-border');

            // Show spinner and disable button
            spinner.classList.remove('d-none');
            btn.disabled = true;

            // Clear previous alerts
            if (alertContainer) {
                alertContainer.innerHTML = '';
            }

            showDebug(`POST ${apiBase} with email=${email}`);
            try {
                // Direct API call for reliability
                const response = await fetch(apiBase, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password })
                });
                let result;
                try { result = await response.json(); } catch { result = {}; }
                showDebug('API response: ' + JSON.stringify(result));

                if (response.ok && result.token) {
                    localStorage.setItem('jwt_token', result.token);
                    // Optionally show success
                    alertContainer.innerHTML = '<div class="alert alert-success">Login successful! Redirecting...</div>';
                    showDebug('Login success, token stored.');
                    setTimeout(() => { window.location.href = '/'; }, 1000);
                } else {
                    let errorMsg = result.error || result.title || 'Login failed. Please check your credentials.';
                    if (result.errors) errorMsg += '<br>' + Object.values(result.errors).flat().join('<br>');
                    alertContainer.innerHTML = `<div class="alert alert-danger">${errorMsg}</div>`;
                    showDebug('Login failed: ' + errorMsg);
                }
            } catch (error) {
                alertContainer.innerHTML = `<div class="alert alert-danger">Unexpected error: ${error.message}</div>`;
                showDebug('JS error: ' + error.message);
            } finally {
                spinner.classList.add('d-none');
                btn.disabled = false;
            }
        });
    }
});
