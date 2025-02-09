window.initializeVideoPlayer = async (dotNetHelper) => {
    try {
        await new Promise(resolve => setTimeout(resolve, 500));

        const video = document.getElementById('videoPlayer');
        if (!video) {
            console.warn('Video element not found, retrying...');
            setTimeout(() => window.initializeVideoPlayer(), 500);
            return;
        }

        console.log('Video element found, initializing...');

        video.addEventListener('loadedmetadata', () => {
            console.log('Video metadata loaded, duration:', video.duration);
        });

        video.addEventListener('progress', () => {
            try {
                if (video.buffered && video.buffered.length > 0) {
                    const bufferedEnd = video.buffered.end(video.buffered.length - 1);
                    const duration = video.duration;
                    const percentBuffered = (bufferedEnd / duration) * 100;
                    console.log(`Buffered: ${bufferedEnd}s (${percentBuffered.toFixed(2)}%)`);
                }
            } catch (error) {
                console.error('Buffering error:', error);
            }
        });

        video.addEventListener('error', (e) => {
            const err = video.error;
            console.error('Video error:', err);

            if (err.code === 3) { // MEDIA_ERR_DECODE
                console.log('Attempting to recover from decode error...');

                // Tries to load the video again
                video.load();

                // Delay before playing the video
                setTimeout(() => {
                    video.play().catch(playError => {
                        console.error('Failed to recover:', playError);
                    });
                }, 1000);
            }
        });

        // Support for seeking
        video.addEventListener('seeking', () => {
            console.log('Seeking to:', video.currentTime);
        });

        video.addEventListener('canplay', () => {
            console.log('Video can start playing');
        });

        // support for loggin to user viewing history
        video.addEventListener('play', async () => {
            await dotNetHelper.invokeMethodAsync('HandleVideoPlay');
        });


        console.log('Video player initialized successfully');
    } catch (error) {
        console.error('Error initializing video player:', error);
    }
};