// Prevents links from being opened when dragging in the post content
document.querySelectorAll('.post-content-link').forEach(link => {
    let isDragging = false;
    link.addEventListener('mousedown', () => isDragging = false);
    link.addEventListener('mousemove', () => isDragging = true);
    link.addEventListener('click', function(e) {
        if (isDragging) {
            e.preventDefault();
            isDragging = false;
        }
    });
});