
document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('login-form');
    const alertContainer = document.getElementById('alert-container');

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

            try {
                const result = await login(email, password);

                if (result.success) {
                    // Redirect to home page on successful login
                    window.location.href = '/';
                } else {
                    // Show error message
                    if (alertContainer) {
                        const alert = document.createElement('div');
                        alert.className = 'alert alert-danger';
                        alert.textContent = result.error || 'Login failed. Please check your credentials.';
                        alertContainer.appendChild(alert);
                    }
                }
            } catch (error) {
                // Show generic error message
                if (alertContainer) {
                    const alert = document.createElement('div');
                    alert.className = 'alert alert-danger';
                    alert.textContent = 'An unexpected error occurred. Please try again later.';
                    alertContainer.appendChild(alert);
                }
                console.error('Login error:', error);
            } finally {
                // Hide spinner and re-enable button
                spinner.classList.add('d-none');
                btn.disabled = false;
            }
        });
    }
});
