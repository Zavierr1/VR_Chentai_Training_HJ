using UnityEngine;
using BNG; // Kita wajib memanggil sistem BNG agar bisa mengontrol HeldItem

public class SnapUrutan : MonoBehaviour
{
    [Header("Snap Zone Selanjutnya")]
    [Tooltip("Masukkan objek Snap Zone ke-2 (Bawah) ke sini")]
    public SnapZone snapZoneBerikutnya;

    void Start()
    {
        // Matikan Snap Zone berikutnya di awal permainan agar tidak bisa dipakai
        if (snapZoneBerikutnya != null)
        {
            snapZoneBerikutnya.gameObject.SetActive(false);
        }
    }

    // Dipanggil saat part 1 berhasil dipasang
    public void BukaSnapBerikutnya()
    {
        if (snapZoneBerikutnya != null)
        {
            snapZoneBerikutnya.gameObject.SetActive(true);
            Debug.Log("Snap Zone " + snapZoneBerikutnya.name + " telah terbuka!");
        }
    }

    // Dipanggil saat part 1 dilepas paksa
    public void TutupSnapBerikutnya()
    {
        if (snapZoneBerikutnya != null)
        {
            // PENTING: Jika di Snap Zone 2 kebetulan sudah ada part yang menempel, 
            // kita harus melepaskannya dulu (dijatuhkan) agar partnya tidak ikut hilang/error.
            if (snapZoneBerikutnya.HeldItem != null)
            {
                snapZoneBerikutnya.ReleaseAll();
            }
            
            // Baru setelah itu Snap Zone 2 dimatikan kembali
            snapZoneBerikutnya.gameObject.SetActive(false);
            Debug.Log("Snap Zone " + snapZoneBerikutnya.name + " dikunci kembali!");
        }
    }
}