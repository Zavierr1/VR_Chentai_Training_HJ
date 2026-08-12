using UnityEngine;

// Plays and stops a slide sound on an attached AudioSource.
// Intended to be triggered from Animation Events to synchronize audio with movement.
[RequireComponent(typeof(AudioSource))]
public class AnimAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;

    // Caches the AudioSource component.
    // Initialized in Awake so it is ready before the first animation frame runs.
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Plays the slide sound if the audio source is available and not already playing.
    public void PlaySlideSound()
    {
        // Test log to confirm this method is being invoked by the animation event.
        Debug.Log("FUNGSI PLAYSLIDESOUND BERHASIL TERPANGGIL!");

        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("AUDIO SEDANG DI-PLAY!"); // Additional test log.
            }
        }
        else
        {
            Debug.LogWarning("AnimAudioTrigger: AudioSource tidak ditemukan!");
        }
    }

    // Stops the slide sound if it is currently playing.
    public void StopSlideSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
