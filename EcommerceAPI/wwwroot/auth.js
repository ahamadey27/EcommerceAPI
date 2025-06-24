// Authentication and API utilities
const API_BASE_URL = 'http://localhost:5000/api';

// Token management
const getToken = () => localStorage.getItem('jwt_token');
const setToken = (token) => localStorage.setItem('jwt_token', token);
const removeToken = () => localStorage.removeItem('jwt_token');

// API call helper with JWT
async function apiCall(endpoint, options = {}) {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers
    });
    
    if (response.status === 401) {
        // Token expired or invalid
        removeToken();
        updateNavigation();
        throw new Error('Authentication required');
    }
    
    return response;
}

// Authentication functions
async function login(email, password) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });
        
        if (response.ok) {
            const data = await response.json();
            setToken(data.token);
            updateNavigation();
            return { success: true, data };
        } else {
            const error = await response.text();
            return { success: false, error };
        }
    } catch (error) {
        return { success: false, error: error.message };
    }
}

async function register(firstName, lastName, email, password, confirmPassword) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                firstName, 
                lastName, 
                email, 
                password, 
                confirmPassword 
            })
        });
        
        if (response.ok) {
            const data = await response.json();
            setToken(data.token);
            updateNavigation();
            return { success: true, data };
        } else {
            const error = await response.text();
            return { success: false, error };
        }
    } catch (error) {
        return { success: false, error: error.message };
    }
}

function logout() {
    removeToken();
    updateNavigation();
    window.location.href = '/';
}

// Check if user is logged in
function isLoggedIn() {
    return !!getToken();
}

// Update navigation based on auth status
function updateNavigation() {
    const authNav = document.getElementById('auth-nav');
    if (!authNav) return;
    
    if (isLoggedIn()) {
        authNav.innerHTML = `
            <li class="nav-item">
                <a class="nav-link text-dark" href="#" onclick="logout()">Logout</a>
            </li>
        `;
    } else {
        authNav.innerHTML = `
            <li class="nav-item">
                <a class="nav-link text-dark" href="/Login">Login</a>
            </li>
            <li class="nav-item">
                <a class="nav-link text-dark" href="/Register">Register</a>
            </li>
        `;
    }
}

// Update cart count
async function updateCartCount() {
    if (!isLoggedIn()) {
        document.getElementById('cart-count').textContent = '0';
        return;
    }    try {
        const response = await apiCall('/shoppingcart');
        if (response.ok) {
            const cart = await response.json();
            const totalItems = cart && cart.items && Array.isArray(cart.items) 
                ? cart.items.reduce((sum, item) => sum + item.quantity, 0) 
                : 0;
            document.getElementById('cart-count').textContent = totalItems;
        }
    } catch (error) {
        console.error('Error updating cart count:', error);
        // Set cart count to 0 on error
        const cartCountElement = document.getElementById('cart-count');
        if (cartCountElement) {
            cartCountElement.textContent = '0';
        }
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    updateNavigation();
    updateCartCount();
});

// Show/hide elements based on auth status
function showForAuth(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(el => {
        el.style.display = isLoggedIn() ? 'block' : 'none';
    });
}

function showForGuest(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(el => {
        el.style.display = isLoggedIn() ? 'none' : 'block';
    });
}

// Role-based visibility functions
function showForAdmin(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(el => {
        el.style.display = isAdmin() ? 'block' : 'none';
    });
}

function showForUser(selector) {
    const elements = document.querySelectorAll(selector);
    elements.forEach(el => {
        el.style.display = (isLoggedIn() && !isAdmin()) ? 'block' : 'none';
    });
}

// Check if current user is admin
function isAdmin() {
    const token = getToken();
    if (!token) return false;
    
    try {
        // Decode JWT token to check for admin role
        const payload = JSON.parse(atob(token.split('.')[1]));
        const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        return roles && (roles === 'Admin' || (Array.isArray(roles) && roles.includes('Admin')));
    } catch (error) {
        console.error('Error checking admin role:', error);
        return false;
    }
}

// Utility functions for API calls
const API = {
    // Products
    async getProducts() {
        const response = await fetch(`${API_BASE_URL}/products`);
        return response.json();
    },
    
    // Cart operations
    async getCart() {
        const response = await apiCall('/shoppingcart');
        return response.json();
    },
      async addToCart(productId, quantity = 1) {
        const response = await apiCall('/shoppingcart/items', {
            method: 'POST',
            body: JSON.stringify({ productId, quantity })
        });
        return response.json();
    },
      async updateCartItem(productId, quantity) {
        const response = await apiCall(`/shoppingcart/items/${productId}`, {
            method: 'PUT',
            body: JSON.stringify({ quantity })
        });
        return response.json();
    },
      async removeFromCart(productId) {
        const response = await apiCall(`/shoppingcart/items/${productId}`, {
            method: 'DELETE'
        });
        return response.ok;
    },
      // Checkout
    async createCheckoutSession() {
        console.log('API.createCheckoutSession called');
        console.log('API_BASE_URL:', API_BASE_URL);
        const response = await apiCall('/checkout/create-session', {
            method: 'POST'
        });
        console.log('Checkout response:', response);
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Checkout API error:', response.status, errorText);
            throw new Error(`Checkout failed: ${response.status} - ${errorText}`);
        }
        return response.json();
    }
};
