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
}


);
window.onload = function () {
    // Get all date elements
    var dateElements = document.getElementsByClassName('date');

    // exammple date
    // 2024-06-02T22:02:01.8149071 if this we add 'Z' to the end to indicate UTC time
    // 2024-06-02T22:02:01.8149071Z this is UTC time


    // Counter for conversions
    var conversionCount = 0;

    // Convert the dates to the user's local timezone
    for (var i = 0; i < dateElements.length; i++) {
        // trim() removes whitespace from both ends of a string
        var dateElement =  dateElements[i];
        dateElement.textContent = dateElement.textContent.trim();
        if (!dateElement.textContent || dateElement.textContent === '' || dateElement.textContent === 'Unknown') {
            continue;
        }
        if (!dateElement.textContent.endsWith('Z')) {
            // The date is already in UTC format
            var originalDate = dateElement.textContent + 'Z'; // Add 'Z' to indicate UTC time
        }
        else {
            // The date is in local time
            var originalDate = dateElement.textContent;
        }
        var date = new Date(originalDate);

        // Format the date
        var options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false };
        var convertedDate = date.toLocaleString(navigator.language, options);

        dateElement.textContent = convertedDate;

        // Increment the counter and log if it reaches 3
        conversionCount++;
        if (conversionCount === 3) {
            console.log('Converted 3 dates. Most recent conversion: ' + originalDate + ' -> ' + convertedDate);
            conversionCount = 0;
        }
    }


    var dateOnlyElements = document.getElementsByClassName('dateOnly');
    // example dateOnly text (UTC by def): 11/24/2024
    for (var i = 0; i < dateOnlyElements.length; i++) {
        var dateElement = dateOnlyElements[i];
        if (!dateElement.textContent || dateElement.textContent === '' || dateElement.textContent === 'Unknown') {
            continue;
        }
        var originalDate = dateElement.textContent;
        var dateParts = originalDate.split('/');
        var date = new Date(Date.UTC(dateParts[2], dateParts[0] - 1, dateParts[1]));

        // Format the date
        var options = { year: 'numeric', month: '2-digit', day: '2-digit' };
        var convertedDate = date.toLocaleString(navigator.language, options);

        dateElement.textContent = convertedDate;
    }

}
