using UnityEngine;
using BNG; // Wajib pakai ini untuk memanggil fitur Grabbable

public class BarangRespawn : MonoBehaviour
{
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    
    private Rigidbody rb;
    private Grabbable grabbable;

    void Start()
    {
        // Catat posisi dan rotasi saat game baru dimulai
        posisiAwal = transform.position;
        rotasiAwal = transform.rotation;
        
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
    }

    public void KembalikanKeMeja()
    {
        // 1. Jika barang masih dipegang tangan player saat menyentuh lantai, lepas paksa!
        if (grabbable != null && grabbable.BeingHeld)
        {
            grabbable.DropItem(false, false); //
        }

        // 2. Reset kecepatan jatuh biar pas balik ke meja gak mantul/meluncur
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; //
            rb.angularVelocity = Vector3.zero; //
        }

        // 3. Kembalikan ke posisi awal
        transform.position = posisiAwal;
        transform.rotation = rotasiAwal;
    }
}