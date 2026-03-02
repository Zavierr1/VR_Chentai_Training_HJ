using UnityEngine;

public class TabletJadiSpawner : MonoBehaviour
{
   [Header("Pengaturan Spawn")]
    [Tooltip("Masukkan prefab tablet yang sudah jadi (depan belakang) ke sini.")]
    public GameObject tabletPrefab;

    [Tooltip("Titik lokasi di mana tablet akan muncul (buat Empty GameObject di posisi cetakan).")]
    public Transform spawnPoint;

    public float tabletLifetime = 3f;

    [Header("Efek Tambahan (Opsional)")]
    [Tooltip("Masukkan efek partikel debu/percikan jika ada.")]
    public ParticleSystem pressEffect; 
    
    [Tooltip("Masukkan suara mesin press di sini.")]
    public AudioClip pressSound;
    private AudioSource audioSource;

    void Start()
    {
        // Mengambil komponen AudioSource jika kamu ingin menambahkan suara
        audioSource = GetComponent<AudioSource>();
    }

    // Fungsi ini yang akan memunculkan tablet
    public void SpawnTablet()
    {
        if (tabletPrefab != null && spawnPoint != null)
        {
            // Melakukan spawn prefab tablet tepat di posisi dan rotasi titik spawnPoint
            GameObject newTablet = Instantiate(tabletPrefab, spawnPoint.position, spawnPoint.rotation);

            Destroy(newTablet, tabletLifetime);
            

            // Memainkan efek partikel (opsional)
            if (pressEffect != null)
            {
                pressEffect.Play();
            }

            // Memainkan suara (opsional)
            if (pressSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pressSound);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Prefab Tablet atau Spawn Point belum di-assign di Inspector!");
        }
    }
}
