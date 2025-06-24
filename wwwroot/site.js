// Enhanced JavaScript for Ecommerce Store

// Initialize page animations and interactions
document.addEventListener('DOMContentLoaded', function() {
    // Add smooth scrolling to all anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Add loading animations to buttons
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function() {
            if (this.type === 'submit') {
                const spinner = this.querySelector('.spinner-border');
                if (spinner) {
                    spinner.classList.remove('d-none');
                    this.disabled = true;
                    
                    // Re-enable button after 3 seconds as fallback
                    setTimeout(() => {
                        spinner.classList.add('d-none');
                        this.disabled = false;
                    }, 3000);
                }
            }
        });
    });

    // Enhanced alert system
    window.showAlert = function(message, type = 'info', container = 'alert-container') {
        const alertContainer = document.getElementById(container);
        if (!alertContainer) return;

        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show slide-up`;
        alertDiv.innerHTML = `
            <i class="fas fa-${getAlertIcon(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        alertContainer.appendChild(alertDiv);
        
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            if (alertDiv.parentNode) {
                alertDiv.classList.remove('show');
                setTimeout(() => {
                    if (alertDiv.parentNode) {
                        alertDiv.remove();
                    }
                }, 150);
            }
        }, 5000);
    };

    // Helper function to get alert icons
    function getAlertIcon(type) {
        const icons = {
            'success': 'check-circle',
            'danger': 'exclamation-triangle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle',
            'primary': 'info-circle'
        };
        return icons[type] || 'info-circle';
    }

    // Enhanced form validation
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const requiredFields = this.querySelectorAll('[required]');
            let isValid = true;

            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    field.classList.add('is-invalid');
                    isValid = false;
                } else {
                    field.classList.remove('is-invalid');
                    field.classList.add('is-valid');
                }
            });

            // Validate email fields
            const emailFields = this.querySelectorAll('input[type="email"]');
            emailFields.forEach(field => {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (field.value && !emailRegex.test(field.value)) {
                    field.classList.add('is-invalid');
                    isValid = false;
                }
            });

            // Validate password confirmation
            const password = this.querySelector('input[type="password"]');
            const confirmPassword = this.querySelector('input[id*="confirm"]');
            if (password && confirmPassword && password.value !== confirmPassword.value) {
                confirmPassword.classList.add('is-invalid');
                isValid = false;
            }

            if (!isValid) {
                e.preventDefault();
                showAlert('Please fix the errors in the form.', 'danger');
            }
        });
    });

    // Add hover effects to cards
    document.querySelectorAll('.card').forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transition = 'transform 0.3s ease, box-shadow 0.3s ease';
        });
    });

    // Initialize tooltips (Bootstrap 5)
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            new bootstrap.Tooltip(el);
        });
    }

    // Initialize popovers (Bootstrap 5)
    if (typeof bootstrap !== 'undefined' && bootstrap.Popover) {
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
            new bootstrap.Popover(el);
        });
    }
});

// Utility functions
window.formatPrice = function(price) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(price);
};

window.showLoading = function(element, show = true) {
    if (show) {
        element.classList.add('loading');
    } else {
        element.classList.remove('loading');
    }
};

// Enhanced console log for debugging
console.log('🛒 Ecommerce Store - Enhanced JavaScript Loaded');