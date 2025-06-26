// Site-wide JavaScript functionality

// Smooth scrolling function
window.scrollToElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

// File download function
window.downloadFile = (fileName, mimeType, fileContent) => {
    const blob = new Blob([fileContent], { type: mimeType });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
};

// Initialize tooltips when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});

// Auto-hide alerts after 5 seconds
window.autoHideAlert = (alertId) => {
    setTimeout(() => {
        const alert = document.getElementById(alertId);
        if (alert) {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }
    }, 5000);
};

// Form validation helpers
window.validateForm = (formId) => {
    const form = document.getElementById(formId);
    if (form) {
        form.classList.add('was-validated');
        return form.checkValidity();
    }
    return false;
};

// Local storage helpers
window.localStorageHelper = {
    setItem: (key, value) => {
        try {
            localStorage.setItem(key, JSON.stringify(value));
            return true;
        } catch (e) {
            console.error('Error saving to localStorage:', e);
            return false;
        }
    },
    
    getItem: (key) => {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : null;
        } catch (e) {
            console.error('Error reading from localStorage:', e);
            return null;
        }
    },
    
    removeItem: (key) => {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            console.error('Error removing from localStorage:', e);
            return false;
        }
    }
};

// Session storage helpers
window.sessionStorageHelper = {
    setItem: (key, value) => {
        try {
            sessionStorage.setItem(key, JSON.stringify(value));
            return true;
        } catch (e) {
            console.error('Error saving to sessionStorage:', e);
            return false;
        }
    },
    
    getItem: (key) => {
        try {
            const item = sessionStorage.getItem(key);
            return item ? JSON.parse(item) : null;
        } catch (e) {
            console.error('Error reading from sessionStorage:', e);
            return null;
        }
    },
    
    removeItem: (key) => {
        try {
            sessionStorage.removeItem(key);
            return true;
        } catch (e) {
            console.error('Error removing from sessionStorage:', e);
            return false;
        }
    }
};

// Copy to clipboard function
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy text: ', err);
        return false;
    }
};

// Loading state management
window.showLoading = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>';
    }
};

window.hideLoading = (elementId, content) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = content || '';
    }
};

// Price formatting helper
window.formatPrice = (amount, currency = 'USD') => {
    try {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency
        }).format(amount);
    } catch (e) {
        return `${currency} ${amount.toFixed(2)}`;
    }
};

// Debounce function for search inputs
window.debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Error handling
window.addEventListener('error', function(e) {
    console.error('Global error:', e.error);
});

window.addEventListener('unhandledrejection', function(e) {
    console.error('Unhandled promise rejection:', e.reason);
});

// Page visibility API for pausing/resuming activities
document.addEventListener('visibilitychange', function() {
    if (document.hidden) {
        // Page is hidden - pause activities
        console.log('Page hidden');
    } else {
        // Page is visible - resume activities
        console.log('Page visible');
    }
});

// Print functionality
window.printPage = () => {
    window.print();
};

// Share functionality (if supported)
window.shareContent = async (title, text, url) => {
    if (navigator.share) {
        try {
            await navigator.share({
                title: title,
                text: text,
                url: url
            });
            return true;
        } catch (err) {
            console.error('Error sharing:', err);
            return false;
        }
    } else {
        // Fallback - copy to clipboard
        const shareText = `${title}\n${text}\n${url}`;
        return await window.copyToClipboard(shareText);
    }
};
