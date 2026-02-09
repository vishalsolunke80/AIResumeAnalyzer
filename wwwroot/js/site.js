// AI Resume Analyzer - Interactive JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Initialize all components
    initFileUpload();
    initFormSubmission();
    initAnimations();
    initScoreAnimation();
});

// ============================================
// File Upload Handler
// ============================================
function initFileUpload() {
    const fileInput = document.getElementById('resumeFile');
    const uploadWrapper = document.querySelector('.file-upload-wrapper');
    
    if (!fileInput || !uploadWrapper) return;
    
    // Click to upload
    uploadWrapper.addEventListener('click', () => fileInput.click());
    
    // Drag and drop
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        uploadWrapper.addEventListener(eventName, preventDefaults, false);
    });
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    ['dragenter', 'dragover'].forEach(eventName => {
        uploadWrapper.addEventListener(eventName, () => {
            uploadWrapper.classList.add('dragover');
        });
    });
    
    ['dragleave', 'drop'].forEach(eventName => {
        uploadWrapper.addEventListener(eventName, () => {
            uploadWrapper.classList.remove('dragover');
        });
    });
    
    uploadWrapper.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        const files = dt.files;
        if (files.length > 0 && files[0].type === 'application/pdf') {
            fileInput.files = files;
            handleFileSelect(files[0]);
        }
    });
    
    // File input change
    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
            handleFileSelect(e.target.files[0]);
        }
    });
    
    function handleFileSelect(file) {
        const fileNameDisplay = document.getElementById('fileName');
        const uploadIcon = uploadWrapper.querySelector('.file-upload-icon');
        const uploadText = uploadWrapper.querySelector('.file-upload-text');
        
        uploadWrapper.classList.add('has-file');
        
        if (uploadIcon) {
            uploadIcon.textContent = '✅';
        }
        
        if (uploadText) {
            uploadText.innerHTML = '<strong>File selected:</strong>';
        }
        
        if (fileNameDisplay) {
            fileNameDisplay.textContent = file.name;
            fileNameDisplay.style.display = 'block';
        } else {
            const nameSpan = document.createElement('div');
            nameSpan.id = 'fileName';
            nameSpan.className = 'file-name-display';
            nameSpan.textContent = file.name;
            uploadWrapper.appendChild(nameSpan);
        }
    }
}

// ============================================
// Form Submission with Loading
// ============================================
function initFormSubmission() {
    const form = document.querySelector('form[enctype="multipart/form-data"]');
    
    if (!form) return;
    
    form.addEventListener('submit', function(e) {
        const fileInput = document.getElementById('resumeFile');
        const jobDesc = document.getElementById('jobDescription');
        
        // Validation
        if (!fileInput.files.length) {
            e.preventDefault();
            showNotification('Please select a PDF file to upload.', 'error');
            return;
        }
        
        if (!jobDesc.value.trim()) {
            e.preventDefault();
            showNotification('Please enter a job description.', 'error');
            return;
        }
        
        // Show loading
        showLoading('Analyzing your resume with AI...');
    });
}

// ============================================
// Loading Overlay
// ============================================
function showLoading(message) {
    let overlay = document.getElementById('loadingOverlay');
    
    if (!overlay) {
        overlay = document.createElement('div');
        overlay.id = 'loadingOverlay';
        overlay.className = 'loading-overlay';
        overlay.innerHTML = `
            <div class="loading-spinner"></div>
            <div class="loading-text">${message || 'Processing...'}</div>
            <div class="loading-steps" style="margin-top: 1rem; color: rgba(255,255,255,0.5); font-size: 0.9rem;">
                <div id="step1" class="loading-step">📄 Extracting text from PDF...</div>
                <div id="step2" class="loading-step" style="opacity: 0.3;">🤖 Analyzing with AI...</div>
                <div id="step3" class="loading-step" style="opacity: 0.3;">📊 Generating results...</div>
            </div>
        `;
        document.body.appendChild(overlay);
    }
    
    overlay.classList.add('active');
    
    // Animate steps
    setTimeout(() => {
        document.getElementById('step1').style.opacity = '0.5';
        document.getElementById('step2').style.opacity = '1';
    }, 2000);
    
    setTimeout(() => {
        document.getElementById('step2').style.opacity = '0.5';
        document.getElementById('step3').style.opacity = '1';
    }, 4000);
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.remove('active');
    }
}

// ============================================
// Notifications
// ============================================
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 1rem 1.5rem;
        background: ${type === 'error' ? 'rgba(255, 75, 43, 0.9)' : 'rgba(102, 126, 234, 0.9)'};
        color: white;
        border-radius: 12px;
        font-weight: 500;
        z-index: 10000;
        animation: slideIn 0.3s ease;
        backdrop-filter: blur(10px);
        box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    `;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease forwards';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// ============================================
// Animations
// ============================================
function initAnimations() {
    // Add animation styles
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        @keyframes slideOut {
            from { transform: translateX(0); opacity: 1; }
            to { transform: translateX(100%); opacity: 0; }
        }
    `;
    document.head.appendChild(style);
    
    // Animate elements on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    document.querySelectorAll('.glass-card, .result-section').forEach(el => {
        el.style.opacity = '0';
        observer.observe(el);
    });
}

// ============================================
// Score Animation
// ============================================
function initScoreAnimation() {
    const scoreCircle = document.querySelector('.score-circle');
    const scoreValue = document.querySelector('.score-value');
    
    if (!scoreCircle || !scoreValue) return;
    
    const targetScore = parseInt(scoreValue.dataset.score || scoreValue.textContent);
    
    if (isNaN(targetScore)) return;
    
    // Set CSS variable for conic gradient
    scoreCircle.style.setProperty('--score', '0');
    scoreValue.textContent = '0%';
    
    // Animate score
    let currentScore = 0;
    const duration = 1500;
    const startTime = performance.now();
    
    function animateScore(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        // Easing function
        const easeOutQuart = 1 - Math.pow(1 - progress, 4);
        currentScore = Math.round(targetScore * easeOutQuart);
        
        scoreCircle.style.setProperty('--score', currentScore);
        scoreValue.textContent = currentScore + '%';
        
        // Update color class
        scoreValue.className = 'score-value';
        if (currentScore >= 70) {
            scoreValue.classList.add('high');
        } else if (currentScore >= 40) {
            scoreValue.classList.add('medium');
        } else {
            scoreValue.classList.add('low');
        }
        
        if (progress < 1) {
            requestAnimationFrame(animateScore);
        }
    }
    
    // Start animation after a short delay
    setTimeout(() => {
        requestAnimationFrame(animateScore);
    }, 500);
}

// ============================================
// Textarea Auto-resize
// ============================================
document.addEventListener('input', function(e) {
    if (e.target.tagName === 'TEXTAREA') {
        e.target.style.height = 'auto';
        e.target.style.height = Math.min(e.target.scrollHeight, 400) + 'px';
    }
});
