document.body.addEventListener('htmx:afterSwap', (e) => {
    if (e.target.id === 'posts-container' || e.target.id === 'loader') {
        document.querySelectorAll("[data-post-id]").forEach(postElement => {
            if (!postElement._watching) {
                startWatchingPost(postElement, postElement.dataset.postId);
                postElement._watching = true;
            }
        });
    }
});
function isCenterLineOverlapping(postElement) {
    const postRect = postElement.getBoundingClientRect();
    const lineY = window.innerHeight / 2;
    return postRect.top <= lineY && postRect.bottom >= lineY;
}

function startWatchingPost(postElement, postId) {
    let viewed = false;
    let timer = null;
    function checkOverlap() {
        if (viewed) return;

        if (isCenterLineOverlapping(postElement)) {
            if (!timer) {
                timer = setTimeout(() => {
                    sendView(postId);
                    viewed = true;
                }, 3000); // after 3 seconds of being in view
            }
        } else {
            clearTimeout(timer);
            timer = null;
        }
    }

    window.addEventListener("scroll", checkOverlap);
    window.addEventListener("resize", checkOverlap);
    checkOverlap(); // Initial check
}

function sendView(postId) {
    fetch('/Posts/ViewPost', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ postID: postId })
    })
        .then(response => {
            if (!response.ok) {
                console.error('Failed to send view for post:', postId);
            } else {
                console.log('View sent for post:', postId);
                htmx.ajax('GET', `/Posts/ViewPost?postId=${postId}`, {
                    target: `#post-stats-${postId}`,
                    swap: 'outerHTML'
                });
            }
        })
        .catch(error => {
            console.error('Error sending view for post:', postId, error);
        });
}