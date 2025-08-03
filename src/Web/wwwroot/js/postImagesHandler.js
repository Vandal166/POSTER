const imageBtn = document.getElementById('uploadImageBtn');
const imageInput = document.getElementById('imageInput');
const imagePreviewList = document.getElementById('imagePreviewList');

window.imageIDs = [];

imageBtn.addEventListener('click', () => {
    imageInput.click();
});

function updateImageFileInputs() {
    // Remove existing
    document.querySelectorAll('input[name="ImageFileIDs"]').forEach(e => e.remove());
    // Add one input per image ID
    const form = document.getElementById('createPostForm');
    imageIDs.forEach(id => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'ImageFileIDs';
        input.value = id;
        form.appendChild(input);
    });
}

imageInput.addEventListener('change', async () => {
    const files = Array.from(imageInput.files);
    if (!files.length) return;

    for (const file of files) {
        const formData = new FormData();
        formData.append('file', file);

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const res = await fetch('/images', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            });

            if (!res.ok) {
                const errorText = await res.text();
                alert("Image upload failed: " + errorText);
                continue;
            }

            const { fileID } = await res.json();
            imageIDs.push(fileID);

            // Create preview element
            const wrapper = document.createElement('div');
            wrapper.className = 'd-inline-block position-relative m-1';

            const img = document.createElement('img');
            img.src = `/images/${fileID}`;
            img.className = 'rounded shadow-sm';
            img.style.width = '120px';
            img.style.height = '120px';
            img.style.objectFit = 'cover';

            const removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'btn btn-sm btn-outline-danger position-absolute';
            removeBtn.style.top = '4px';
            removeBtn.style.right = '4px';
            removeBtn.innerHTML = '<i class="bi bi-x"></i>';
            removeBtn.onclick = async () => {
                await fetch(`/images/${fileID}`, { method: 'DELETE' });
                wrapper.remove();
                imageIDs = imageIDs.filter(id => id !== fileID);
                updateImageFileInputs();
                if (imageIDs.length === 0) imagePreviewList.classList.add('d-none');
            };

            wrapper.appendChild(img);
            wrapper.appendChild(removeBtn);
            imagePreviewList.appendChild(wrapper);

        } catch (err) {
            console.error(err);
        }
    }

    updateImageFileInputs();
    if (imageIDs.length > 0) imagePreviewList.classList.remove('d-none');
    imageInput.value = '';
});