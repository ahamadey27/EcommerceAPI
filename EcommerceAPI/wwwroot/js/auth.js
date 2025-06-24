// API Configuration
const API_BASE_URL = '/api'; // Since we're serving from the same domain now

// API utility functions
const API = {
    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        const token = getToken();
        
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` })
            }
        };
        
        const response = await fetch(url, { ...defaultOptions, ...options });
        
        if (!response.ok) {
            if (response.status === 401) {
                clearToken();
                window.location.href = '/Login';
            }
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        return response.json();
    },

    // Auth endpoints
    async register(firstName, lastName, email, password, confirmPassword) {
        return this.request('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ firstName, lastName, email, password, confirmPassword })
        });
    },

    async login(email, password) {
        return this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
    },

    // Product endpoints
    async getProducts() {
        return this.request('/products');
    },

    async addProduct(product) {
        return this.request('/products', {
            method: 'POST',
            body: JSON.stringify(product)
        });
    },

    // Cart endpoints
    async getCart() {
        return this.request('/shopping-cart');
    },

    async addToCart(productId, quantity) {
        return this.request('/shopping-cart/add', {
            method: 'POST',
            body: JSON.stringify({ productId, quantity })
        });
    },

    async updateCartItem(productId, quantity) {
        return this.request(`/shopping-cart/update/${productId}`, {
            method: 'PUT',
            body: JSON.stringify({ quantity })
        });
    },

    async removeFromCart(productId) {
        return this.request(`/shopping-cart/remove/${productId}`, {
            method: 'DELETE'
        });
    },

    // Checkout endpoints
    async createOrder() {
        return this.request('/checkout/create-order', {
            method: 'POST'
        });
    }
};

// Token management
function getToken() {
    return localStorage.getItem('jwtToken');
}

function setToken(token) {
    localStorage.setItem('jwtToken', token);
}

function clearToken() {
    localStorage.removeItem('jwtToken');
}

function isLoggedIn() {
    return !!getToken();
}

// Auth functions
async function register(firstName, lastName, email, password, confirmPassword) {
    try {
        const response = await API.register(firstName, lastName, email, password, confirmPassword);
        if (response.token) {
            setToken(response.token);
            updateAuthUI();
            return { success: true };
        }
        return { success: false, error: response.message || 'Registration failed' };
    } catch (error) {
        return { success: false, error: error.message };
    }
}

async function login(email, password) {
    try {
        const response = await API.login(email, password);
        if (response.token) {
            setToken(response.token);
            updateAuthUI();
            return { success: true };
        }
        return { success: false, error: response.message || 'Login failed' };
    } catch (error) {
        return { success: false, error: error.message };
    }
}

function logout() {
    clearToken();
    updateAuthUI();
    window.location.href = '/';
}

// UI management
function updateAuthUI() {
    const authNav = document.getElementById('auth-nav');
    if (!authNav) return;

    if (isLoggedIn()) {
        authNav.innerHTML = `
            <li class="nav-item">
                <button class="btn btn-outline-danger" onclick="logout()">Logout</button>
            </li>
        `;
    } else {
        authNav.innerHTML = `
            <li class="nav-item">
                <a class="nav-link" href="/Login">Login</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="/Register">Register</a>
            </li>
        `;
    }
}

function showForAuth(selector) {
    document.querySelectorAll(selector).forEach(el => {
        el.style.display = isLoggedIn() ? 'block' : 'none';
    });
}

function showForGuest(selector) {
    document.querySelectorAll(selector).forEach(el => {
        el.style.display = isLoggedIn() ? 'none' : 'block';
    });
}

function showForAdmin(selector) {
    // For now, show for all authenticated users
    // In a real app, you'd check the user's role from the JWT
    showForAuth(selector);
}

async function updateCartCount() {
    if (!isLoggedIn()) {
        document.getElementById('cart-count').textContent = '0';
        return;
    }

    try {
        const cart = await API.getCart();
        const count = cart.items ? cart.items.reduce((sum, item) => sum + item.quantity, 0) : 0;
        document.getElementById('cart-count').textContent = count;
    } catch (error) {
        console.error('Error updating cart count:', error);
        document.getElementById('cart-count').textContent = '0';
    }
}

// Initialize auth UI on page load
document.addEventListener('DOMContentLoaded', function() {
    updateAuthUI();
    if (isLoggedIn()) {
        updateCartCount();
    }
});
