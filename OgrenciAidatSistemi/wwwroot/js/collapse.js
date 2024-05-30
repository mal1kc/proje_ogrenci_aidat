// collapse.js
document.addEventListener('DOMContentLoaded', (event) => {
    var collapsibleElements = document.querySelectorAll('[data-bs-toggle="collapse"]');

    collapsibleElements.forEach((element) => {
        var targetId = element.getAttribute('data-bs-target');
        var targetElement = document.querySelector(targetId);
        var triangle = element.querySelector('.triangle');

        if (targetElement && triangle) {
            targetElement.addEventListener('show.bs.collapse', function () {
                triangle.classList.add('flipped');
            });

            targetElement.addEventListener('hide.bs.collapse', function () {
                triangle.classList.remove('flipped');
            });
        }
    });
});
