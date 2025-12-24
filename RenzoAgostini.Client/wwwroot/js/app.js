// ===== JAVASCRIPT MODERNO PER GALLERIA AGOSTINI =====

// Toast Notification System
class ToastManager {
    constructor() {
        this.container = this.createContainer();
        this.toasts = new Map();
        this.toastId = 0;
    }

    createContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }
        return container;
    }

    show(message, type = 'info', title = '', duration = 5000) {
        const id = ++this.toastId;
        const toast = this.createToast(id, message, type, title);

        this.container.appendChild(toast);
        this.toasts.set(id, toast);

        // Trigger entrance animation
        setTimeout(() => toast.classList.add('show'), 10);

        // Auto remove
        if (duration > 0) {
            setTimeout(() => this.remove(id), duration);
        }

        return id;
    }

    createToast(id, message, type, title) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.dataset.toastId = id;

        const icon = this.getIcon(type);

        toast.innerHTML = `
            <div class="toast-icon">${icon}</div>
            <div class="toast-content">
                ${title ? `<div class="toast-title">${title}</div>` : ''}
                <div class="toast-message">${message}</div>
            </div>
            <button class="toast-close" onclick="toastManager.remove(${id})">×</button>
        `;

        // Progress bar animation
        const progressBar = document.createElement('div');
        progressBar.className = 'toast-progress';
        toast.appendChild(progressBar);

        return toast;
    }

    getIcon(type) {
        // Removed emojis as per user request
        return '';
    }

    remove(id) {
        const toast = this.toasts.get(id);
        if (toast) {
            toast.classList.add('removing');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
                this.toasts.delete(id);
            }, 300);
        }
    }

    clear() {
        this.toasts.forEach((toast, id) => this.remove(id));
    }
}

// Initialize Toast Manager
const toastManager = new ToastManager();

// Global Functions for Blazor Integration
window.showToast = (message, type, title) => {
    return toastManager.show(message, type, title);
};

window.clearToasts = () => {
    toastManager.clear();
};

// Smooth Scroll Functionality
window.initializeSmoothScroll = () => {
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
};

// Counter Animation
window.animateCounters = () => {
    const observerOptions = {
        threshold: 0.5,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counter = entry.target;
                const target = parseInt(counter.dataset.target || counter.textContent);
                animateCounter(counter, target);
                observer.unobserve(counter);
            }
        });
    }, observerOptions);

    document.querySelectorAll('.stat-number').forEach(counter => {
        observer.observe(counter);
    });
};

function animateCounter(element, target) {
    let current = 0;
    const increment = target / 50; // 50 steps
    const timer = setInterval(() => {
        current += increment;
        if (current >= target) {
            element.textContent = target;
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current);
        }
    }, 30);
}

// Scroll Animations
window.initializeScrollAnimations = () => {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-fade-in-up');
            }
        });
    }, observerOptions);

    // Observe elements for animation
    document.querySelectorAll('.feature-card, .painting-card, .cta-container').forEach(el => {
        observer.observe(el);
    });
};

// Copy to Clipboard
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        const successful = document.execCommand('copy');
        document.body.removeChild(textArea);
        return successful;
    }
};

// Web Share API
window.shareContent = async (shareData) => {
    if (navigator.share) {
        try {
            await navigator.share(shareData);
            return true;
        } catch (err) {
            console.error('Error sharing:', err);
            return false;
        }
    }
    return false;
};

// Layout Initialization
window.initializeLayout = () => {
    // Add mobile menu event listeners
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            // Close mobile menu if open
            const mobileMenu = document.querySelector('.nav-mobile.active');
            if (mobileMenu) {
                mobileMenu.classList.remove('active');
            }
        }
    });

    // Handle mobile menu backdrop clicks
    document.addEventListener('click', (e) => {
        const mobileMenu = document.querySelector('.nav-mobile');
        const mobileToggle = document.querySelector('.nav-mobile-toggle');

        if (mobileMenu && !mobileMenu.contains(e.target) && !mobileToggle.contains(e.target)) {
            mobileMenu.classList.remove('active');
        }
    });

    // Initialize other layout features
    initializeNavbarScroll();
    initializeLazyLoading();
};

// Navbar Scroll Effect
function initializeNavbarScroll() {
    let navbar = document.querySelector('.nav');
    let lastScrollY = window.scrollY;

    window.addEventListener('scroll', () => {
        // Re-attempt to find navbar if not found (Blazor hydration timing)
        if (!navbar) {
            navbar = document.querySelector('.nav');
        }

        if (!navbar) return;

        const currentScrollY = window.scrollY;

        if (currentScrollY > 100) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }

        // Hide/show navbar on scroll
        if (currentScrollY > lastScrollY && currentScrollY > 200) {
            navbar.style.transform = 'translateY(-100%)';
        } else {
            navbar.style.transform = 'translateY(0)';
        }

        lastScrollY = currentScrollY;
    });
}

// Lazy Loading for Images
function initializeLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

// Add Bounce Animation to Elements
window.addBounceAnimation = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.classList.add('animate-bounce');
        setTimeout(() => {
            element.classList.remove('animate-bounce');
        }, 600);
    }
};

// Home Page Specific Animations
window.initializeHomeAnimations = () => {
    // Parallax effect for hero background
    const hero = document.querySelector('.hero');
    if (hero) {
        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const rate = scrolled * -0.5;
            hero.style.transform = `translateY(${rate}px)`;
        });
    }

    // Stagger animation for feature cards
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.2}s`;
    });

    // Floating animation for hero elements
    const heroElements = document.querySelectorAll('.hero-badge, .hero-title, .hero-subtitle, .hero-actions');
    heroElements.forEach((element, index) => {
        element.style.animationDelay = `${index * 0.3}s`;
        element.classList.add('animate-fade-in-up');
    });
};

// Utility Functions
window.trackEvent = (eventName, eventData) => {
    // Google Analytics or other tracking service
    if (typeof gtag !== 'undefined') {
        gtag('event', eventName, eventData);
    }
    console.log('Event tracked:', eventName, eventData);
};

// Performance Monitoring
window.measurePerformance = (name, fn) => {
    const start = performance.now();
    const result = fn();
    const end = performance.now();
    console.log(`${name} took ${end - start} milliseconds`);
    return result;
};

// Accessibility Helpers
window.trapFocus = (element) => {
    const focusableElements = element.querySelectorAll(
        'a[href], button, textarea, input[type="text"], input[type="radio"], input[type="checkbox"], select'
    );
    const firstFocusableElement = focusableElements[0];
    const lastFocusableElement = focusableElements[focusableElements.length - 1];

    element.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            if (e.shiftKey) {
                if (document.activeElement === firstFocusableElement) {
                    lastFocusableElement.focus();
                    e.preventDefault();
                }
            } else {
                if (document.activeElement === lastFocusableElement) {
                    firstFocusableElement.focus();
                    e.preventDefault();
                }
            }
        }
    });
};

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.initializeSmoothScroll();
    window.initializeLayout();

    // Add CSS classes for enhanced animations
    const style = document.createElement('style');
    style.textContent = `
        .animate-bounce {
            animation: bounceScale 0.6s ease-out !important;
        }
        
        .toast {
            transform: translateX(100%);
            transition: transform 0.3s ease-out;
        }
        
        .toast.show {
            transform: translateX(0);
        }
        
        .toast.removing {
            transform: translateX(100%);
            opacity: 0;
        }
        
        .nav.scrolled {
            background: rgba(255, 255, 255, 0.98) !important;
            backdrop-filter: blur(20px);
            box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
        }
        
        .lazy {
            opacity: 0;
            transition: opacity 0.3s;
        }
        
        .lazy:not(.lazy) {
            opacity: 1;
        }
    `;
    document.head.appendChild(style);
});

// Export for global access
window.GalleriaUtils = {
    toastManager,
    initializeSmoothScroll: window.initializeSmoothScroll,
    animateCounters: window.animateCounters,
    initializeScrollAnimations: window.initializeScrollAnimations,
    copyToClipboard: window.copyToClipboard,
    shareContent: window.shareContent,
    initializeLayout: window.initializeLayout,
    addBounceAnimation: window.addBounceAnimation,
    initializeHomeAnimations: window.initializeHomeAnimations,
    trackEvent: window.trackEvent,
    measurePerformance: window.measurePerformance,
    trackEvent: window.trackEvent,
    measurePerformance: window.measurePerformance,
    trapFocus: window.trapFocus,
    initializeImageProtection: window.initializeImageProtection
};

// Image Protection (Right-click disable)
window.initializeImageProtection = () => {
    document.addEventListener('contextmenu', (e) => {
        if (e.target.tagName === 'IMG') {
            e.preventDefault();
            // Optional: Show a toast? No, silent is better to not annoy legitimate users too much.
            return false;
        }
    });

    // Disable drag start on images globally
    document.addEventListener('dragstart', (e) => {
        if (e.target.tagName === 'IMG') {
            e.preventDefault();
            return false;
        }
    });
};

// Auto-run on load
window.initializeImageProtection();