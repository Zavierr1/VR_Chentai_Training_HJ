using UnityEngine;

public class TabletJadiSpawner : MonoBehaviour
{
    [Header("Pengaturan Spawn")]
    public GameObject tabletPrefab;
    public Transform spawnPoint;
    public float tabletLifetime = 3f;

    [Header("Koneksi ke Ejector")]
    [Tooltip("Tarik objek yang punya script EjectorController ke kotak ini")]
    public EjectorController ejectorMesin; // <--- BARIS BARU

    [Header("Efek Tambahan (Opsional)")]
    public ParticleSystem pressEffect; 
    public AudioClip pressSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SpawnTablet()
    {
        if (tabletPrefab != null && spawnPoint != null)
        {
            GameObject newTablet = Instantiate(tabletPrefab, spawnPoint.position, spawnPoint.rotation);
            Destroy(newTablet, tabletLifetime);
            
            // >>> TAMBAHAN BARU: Suruh ejector melempar dadu probabilitas!
            if (ejectorMesin != null)
            {
                ejectorMesin.CekEject();
            }

            if (pressEffect != null) pressEffect.Play();
            if (pressSound != null && audioSource != null) audioSource.PlayOneShot(pressSound);
        }
        else
        {
            Debug.LogWarning("⚠️ Prefab Tablet atau Spawn Point belum di-assign di Inspector!");
        }
    }
}