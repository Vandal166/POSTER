document.body.addEventListener("showToast", function (e) {
    let { message, type } = e.detail;
    type = type.toLowerCase(); // Normalize enum value
    showToast(message, type);
});

let toastIdCounter = 0;

function showToast(message, type = 'error') {
    const toastContainer = document.getElementById('global-toast-container');
    type = type.toLowerCase();
    
    const toastId = `toast-${toastIdCounter++}`;
    const toastEl = document.createElement('div');

    toastEl.className = `toast align-items-center text-white border-0 mb-2 bg-${type === 'success' ? 'success' : 'danger'}`;
    toastEl.id = toastId;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toastEl);

    const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}
