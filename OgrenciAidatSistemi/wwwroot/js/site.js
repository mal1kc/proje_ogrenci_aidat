// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById('color-mode-toggle-btn').addEventListener('click', () => {
    const currentTheme = document.documentElement.getAttribute('data-bs-theme');
    const switchToTheme = currentTheme === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-bs-theme', switchToTheme);
    localStorage.setItem('theme', switchToTheme);
    updateButtonText();
});

// Set the theme on initial load

const storedTheme = localStorage.getItem('theme');
if (storedTheme) {
    document.documentElement.setAttribute('data-bs-theme', storedTheme);
}

function updateButtonText() {
    const storedTheme = localStorage.getItem('theme');
    var btnSwitch = document.getElementById('color-mode-toggle-btn');
    if (storedTheme === 'dark') {
        // Dark mode
        btnSwitch.textContent = '🐱‍👤';
    } else {
        // Light mode
        btnSwitch.textContent = '☁️';
    }
}

window.addEventListener('DOMContentLoaded', (event) => {
    const storedTheme = localStorage.getItem('theme');
    if (storedTheme) {
        document.documentElement.setAttribute('data-bs-theme', storedTheme);
    }

    updateButtonText();
});