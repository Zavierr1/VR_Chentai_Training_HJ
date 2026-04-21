using UnityEngine;
using BNG; // Wajib pakai ini untuk memanggil fitur Grabbable

public class BarangRespawn : MonoBehaviour
{
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    
    // >>> TAMBAHAN: Variabel penyimpan status awal <<<
    private bool isKinematicAwal; 
    
    private Rigidbody rb;
    private Grabbable grabbable;

    void Start()
    {
        // Catat posisi dan rotasi saat game baru dimulai
        posisiAwal = transform.position;
        rotasiAwal = transform.rotation;
        
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();

        // >>> TAMBAHAN: Catat status awal Kinematic <<<
        if (rb != null)
        {
            isKinematicAwal = rb.isKinematic;
        }
    }

    public void KembalikanKeMeja()
    {
        // 1. Jika barang masih dipegang tangan player saat menyentuh lantai, lepas paksa!
        if (grabbable != null && grabbable.BeingHeld)
        {
            grabbable.DropItem(false, false); 
        }

        // 2. Reset fisik dan kecepatan jatuh
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; 
            
            // >>> FIX: Kembalikan status Kinematic ke aslinya <<<
            rb.isKinematic = isKinematicAwal;
        }

        // 3. Kembalikan ke posisi awal
        transform.position = posisiAwal;
        transform.rotation = rotasiAwal;
    }
}