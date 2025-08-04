// setting default src for images and videos on 404
function setFallbackOnError(selector, attr = 'src') {
    document.querySelectorAll(selector).forEach(el => {
        el.addEventListener('error', function handler() {
            if (el[attr] !== el.dataset.fallback) {
                el[attr] = el.dataset.fallback;
            }
            el.removeEventListener('error', handler);
        });
    });
}
document.querySelectorAll('video[data-fallback] source').forEach(source => {
    source.addEventListener('error', function handler() {
        const video = source.parentElement;
        if (video && video.poster !== video.dataset.fallback) {
            video.poster = video.dataset.fallback;
        }
        source.removeEventListener('error', handler);
    });
});

setFallbackOnError('img[data-fallback]', 'src');