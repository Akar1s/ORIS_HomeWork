
document.addEventListener("DOMContentLoaded", function () {
        const bookingForm = document.getElementById("booking-form");
    if (bookingForm) {
        initBookingForm(bookingForm);
    }

        const regForm = document.getElementById("register-form");
    if (regForm) {
        initRegistrationForm(regForm);
    }

        const loginForm = document.getElementById("login-form");
    if (loginForm) {
        initLoginForm(loginForm);
    }

        const styleSelect = document.getElementById("style-filter");
    const tourList = document.getElementById("tour-list");
    if (styleSelect && tourList) {
        initTourFilter(styleSelect, tourList);
    }

        initLoadMore();

        initSearchAutocomplete();
});

function initBookingForm(form) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const nameRegex = /^[\p{L}\s'-]{2,100}$/u;

    form.addEventListener("submit", function (e) {
        let valid = true;
        clearErrors(form);

        const nameInput = form.querySelector("#name");
        const emailInput = form.querySelector("#email");
        const personsInput = form.querySelector("#persons");
        const status = document.getElementById("booking-status");

                if (!nameRegex.test(nameInput.value.trim())) {
            showError(form, "name", "Please enter a valid name (2-100 characters)");
            valid = false;
        }

                if (!emailRegex.test(emailInput.value.trim())) {
            showError(form, "email", "Please enter a valid email address");
            valid = false;
        }

                const persons = Number(personsInput.value);
        if (!Number.isInteger(persons) || persons < 1 || persons > 20) {
            showError(form, "persons", "Number of travelers must be between 1 and 20");
            valid = false;
        }

        if (!valid) {
            e.preventDefault();
            if (status) {
                status.textContent = "Please fix the errors above";
                status.style.color = "#c62828";
            }
        }
    });
}

function initRegistrationForm(form) {
    const emailRegex = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$/i;
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

        const emailInput = form.querySelector("#email");
    const emailCounter = document.getElementById("email-count");
    if (emailInput && emailCounter) {
        emailInput.addEventListener("input", function() {
            emailCounter.textContent = this.value.length;
        });
    }

        const passwordInput = form.querySelector("#password");
    const strengthBar = document.getElementById("strength-bar");
    if (passwordInput && strengthBar) {
        passwordInput.addEventListener("input", function() {
            updatePasswordStrength(this.value, strengthBar);
        });
    }

    form.addEventListener("submit", function (e) {
        let valid = true;
        clearErrors(form);

        const email = form.querySelector("#email");
        const password = form.querySelector("#password");
        const confirm = form.querySelector("#confirm");

        if (!emailRegex.test(email.value.trim()) || email.value.length > 150) {
            showError(form, "email", "Please enter a valid email address");
            valid = false;
        }

        if (!passwordRegex.test(password.value)) {
            showError(form, "password", "Password must be at least 8 characters with uppercase, lowercase and number");
            valid = false;
        }

        if (password.value !== confirm.value) {
            showError(form, "confirm", "Passwords do not match");
            valid = false;
        }

        if (!valid) e.preventDefault();
    });
}

function initLoginForm(form) {
    const emailRegex = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$/i;

    form.addEventListener("submit", function (e) {
        let valid = true;
        clearErrors(form);

        const email = form.querySelector("#email");
        const password = form.querySelector("#password");

        if (!emailRegex.test(email.value.trim())) {
            showError(form, "email", "Please enter a valid email address");
            valid = false;
        }

        if (password.value.length < 1) {
            showError(form, "password", "Please enter your password");
            valid = false;
        }

        if (!valid) e.preventDefault();
    });
}

function updatePasswordStrength(password, strengthBar) {
    strengthBar.className = "password-strength-bar";
    
    if (password.length === 0) {
        strengthBar.style.width = "0";
        return;
    }
    
    let score = 0;
    if (password.length >= 8) score++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
    if (/\d/.test(password)) score++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score++;
    
    if (score <= 1) {
        strengthBar.classList.add("strength-weak");
    } else if (score <= 2) {
        strengthBar.classList.add("strength-medium");
    } else {
        strengthBar.classList.add("strength-strong");
    }
}

function initTourFilter(select, container) {
    select.addEventListener("change", function () {
        const styleId = select.value;
        let url = "/api/tours";

        if (styleId) {
            url += "?styleId=" + encodeURIComponent(styleId);
        }

                container.innerHTML = '<div class="loading">Loading tours...</div>';

        fetch(url, {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error("HTTP error " + response.status);
            }
            return response.json();
        })
        .then(data => {
            renderTours(container, data);
        })
        .catch(err => {
            console.error(err);
            container.innerHTML = '<div class="error-state">Error loading tours. Please try again.</div>';
        });
    });
}

function renderTours(container, tours) {
    if (!Array.isArray(tours) || tours.length === 0) {
        container.innerHTML = '<div class="empty-state">No tours found matching your criteria.</div>';
        return;
    }

    let html = "";
    for (const t of tours) {
        html += `
        <div class="tour-card" data-id="${t.id}">
            <div class="card-image">
                <img src="https://csspicker.dev/api/image/?q=${encodeURIComponent(t.destination)}&image_type=photo" alt="${escapeHtml(t.destination)}">
                <span class="badge">Top Seller</span>
                <button class="quick-view">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                        <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                    Quick View
                </button>
            </div>
            <div class="card-content">
                <p class="travel-style">${escapeHtml(t.travelStyle)}</p>
                <h3>${escapeHtml(t.name)}</h3>
                <div class="tour-details">
                    <span>
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                            <line x1="16" y1="2" x2="16" y2="6"></line>
                            <line x1="8" y1="2" x2="8" y2="6"></line>
                            <line x1="3" y1="10" x2="21" y2="10"></line>
                        </svg>
                        ${t.days} days
                    </span>
                    <span>
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                            <circle cx="12" cy="10" r="3"></circle>
                        </svg>
                        ${escapeHtml(t.destination)}
                    </span>
                </div>
                <div class="price-section">
                    <div class="price-info">
                        <p class="current-price">$${t.price} <span>USD</span></p>
                    </div>
                    <button class="view-tour" onclick="window.location.href='/tours/${t.id}'">View tour</button>
                </div>
            </div>
        </div>`;
    }
    container.innerHTML = html;
}

function initLoadMore() {
    const loadMoreBtn = document.querySelector('.load-more-btn');
    if (!loadMoreBtn) return;

    let currentPage = 1;
    
    loadMoreBtn.addEventListener('click', function() {
        currentPage++;
        loadMoreBtn.textContent = 'Loading...';
        loadMoreBtn.disabled = true;

        fetch(`/api/tours?page=${currentPage}`)
            .then(response => response.json())
            .then(data => {
                if (data.length > 0) {
                    const grid = document.querySelector('.tour-grid');
                    data.forEach(tour => {
                                            });
                    loadMoreBtn.textContent = 'Load more';
                    loadMoreBtn.disabled = false;
                } else {
                    loadMoreBtn.textContent = 'No more tours';
                    loadMoreBtn.disabled = true;
                }
            })
            .catch(err => {
                console.error(err);
                loadMoreBtn.textContent = 'Load more';
                loadMoreBtn.disabled = false;
            });
    });
}

function initSearchAutocomplete() {
    const searchInputs = document.querySelectorAll('.hero-input[name="destination"], #header-search');
    
    const destinations = [
        'Peru', 'Thailand', 'Japan', 'Italy', 'Morocco', 
        'Costa Rica', 'Greece', 'Vietnam', 'Egypt', 'Iceland',
        'Tanzania', 'India', 'Portugal', 'Mexico', 'New Zealand'
    ];

    searchInputs.forEach(input => {
        if (!input) return;

        const wrapper = document.createElement('div');
        wrapper.className = 'autocomplete-wrapper';
        wrapper.style.position = 'relative';
        
        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);

        const dropdown = document.createElement('div');
        dropdown.className = 'autocomplete-dropdown';
        dropdown.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            display: none;
            z-index: 1000;
            max-height: 200px;
            overflow-y: auto;
        `;
        wrapper.appendChild(dropdown);

        input.addEventListener('input', function() {
            const value = this.value.toLowerCase();
            if (value.length < 2) {
                dropdown.style.display = 'none';
                return;
            }

            const matches = destinations.filter(d => 
                d.toLowerCase().includes(value)
            );

            if (matches.length > 0) {
                dropdown.innerHTML = matches.map(m => `
                    <div class="autocomplete-item" style="padding: 10px 15px; cursor: pointer; border-bottom: 1px solid #f0f0f0;">
                        ${escapeHtml(m)}
                    </div>
                `).join('');
                dropdown.style.display = 'block';

                dropdown.querySelectorAll('.autocomplete-item').forEach(item => {
                    item.addEventListener('click', function() {
                        input.value = this.textContent.trim();
                        dropdown.style.display = 'none';
                    });
                    
                    item.addEventListener('mouseenter', function() {
                        this.style.background = '#f5f5f5';
                    });
                    
                    item.addEventListener('mouseleave', function() {
                        this.style.background = 'white';
                    });
                });
            } else {
                dropdown.style.display = 'none';
            }
        });

        document.addEventListener('click', function(e) {
            if (!wrapper.contains(e.target)) {
                dropdown.style.display = 'none';
            }
        });
    });
}

function showError(form, fieldName, message) {
    const errorElem = form.querySelector(`.error[data-for="${fieldName}"]`);
    const input = form.querySelector(`#${fieldName}`);
    if (errorElem) errorElem.textContent = message;
    if (input) input.classList.add('error-input');
}

function clearErrors(form) {
    form.querySelectorAll(".error").forEach(e => e.textContent = "");
    form.querySelectorAll("input").forEach(e => e.classList.remove('error-input'));
}

function escapeHtml(str) {
    if (!str) return "";
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}
