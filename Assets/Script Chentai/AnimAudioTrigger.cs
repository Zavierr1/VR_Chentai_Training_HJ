using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;

    // Ganti Start menjadi Awake agar inisialisasi terjadi sebelum frame 0 animasi berjalan
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Fungsi ini akan dipanggil saat animasi bergerak
    public void PlaySlideSound()
    {
        // Tambahkan baris ini untuk tes
        Debug.Log("FUNGSI PLAYSLIDESOUND BERHASIL TERPANGGIL!"); 

        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("AUDIO SEDANG DI-PLAY!"); // Tes tambahan
            }
        }
        else
        {
            Debug.LogWarning("AnimAudioTrigger: AudioSource tidak ditemukan!");
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