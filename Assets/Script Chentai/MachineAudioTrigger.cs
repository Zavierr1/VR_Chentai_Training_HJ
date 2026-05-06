using UnityEngine;
using System.Collections; // Wajib ditambahkan untuk menggunakan Coroutine

[RequireComponent(typeof(AudioSource))]
public class MachineAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;
    
    [Header("Pengaturan Audio")]
    [Tooltip("Durasi audio menyala dalam detik")]
    public float playDuration = 1.46f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Panggil fungsi ini tepat di saat mesin mulai menyala/berjalan
    public void PlayMachineSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            // Menjalankan timer otomatis
            StartCoroutine(PlayAndStopRoutine());
        }
    }

    // Fungsi tambahan kalau sewaktu-waktu mesin dimatikan paksa sebelum 1.46 detik
    public void StopMachineSoundForce()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StopAllCoroutines(); // Hentikan timer
            audioSource.Stop();  // Matikan suara
        }
    }

    // Ini adalah Coroutine (fungsi yang bisa berjalan beriringan dengan waktu)
    private IEnumerator PlayAndStopRoutine()
    {
        audioSource.Play();
        Debug.Log("Audio Mesin Menyala!");

        // Tunggu persis sesuai durasi yang diatur (1.46 detik)
        yield return new WaitForSeconds(playDuration);

        audioSource.Stop();
        Debug.Log("Audio Mesin Berhenti setelah " + playDuration + " detik.");
    }
}