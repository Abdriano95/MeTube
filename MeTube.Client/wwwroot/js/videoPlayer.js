window.initializeVideoPlayer = async (dotNetHelper) => {
    const video = document.getElementById('videoPlayer');
    if (!video) return;

    // Record user viewing history
    video.addEventListener('play', async () => {
        console.log('Video playback started');
        await dotNetHelper.invokeMethodAsync('HandleVideoPlay');
    });

    console.log('Simple video player initialized');
};

