const videoBtn = document.getElementById('uploadVideoBtn');
const videoInput = document.getElementById('videoInput');
const videoPreview = document.getElementById('videoPreview');
const videoElement = document.getElementById('videoElement');
const videoFileIDInput = document.getElementById('videoFileID');
const removeBtn = document.getElementById('removeVideoBtn');

videoBtn.addEventListener('click', () => {
    videoInput.click();
});

videoInput.addEventListener('change', async () => {
    const file = videoInput.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const res = await fetch('/videos', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token
            }
        });

        if (!res.ok){
            const errorText = await res.text();
            alert("Video upload failed" + errorText); //TODO display an float error modal 
            throw new Error("Upload failed");
        }

        const { fileID } = await res.json();

        // Set hidden input
        videoFileIDInput.value = fileID;

        // Show preview
        videoElement.src = `/videos/${fileID}`;
        videoPreview.classList.remove('d-none');
    } catch (err) {
        console.error(err);
    }
});

removeBtn.addEventListener('click', async () => {
    const fileID = videoFileIDInput.value;
    if (fileID) {
        try {
            await fetch(`/videos/${fileID}`, {
                method: 'DELETE'
            });
        } catch (err) {
            console.error('Failed to delete video:', err);
        }
    }

    videoElement.src = '';
    videoFileIDInput.value = '';
    videoPreview.classList.add('d-none');
    videoInput.value = '';
});