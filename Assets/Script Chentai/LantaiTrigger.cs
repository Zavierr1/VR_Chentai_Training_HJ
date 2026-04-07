using UnityEngine;

public class LantaiTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah objek yang menyentuh lantai punya script BarangRespawn
        // Pakai GetComponentInParent jaga-jaga kalau collidernya ada di child
        BarangRespawn barangJatuh = other.GetComponentInParent<BarangRespawn>();
        
        if (barangJatuh != null)
        {
            // Jika iya, panggil fungsi kembalikan
            barangJatuh.KembalikanKeMeja();
            Debug.Log("Barang jatuh! Dikembalikan ke meja: " + other.gameObject.name);
        }
    }
}