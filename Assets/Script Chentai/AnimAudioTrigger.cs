using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Fungsi ini akan dipanggil saat animasi bergerak
    public void PlaySlideSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Fungsi ini dipanggil saat animasi berhenti
    public void StopSlideSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}