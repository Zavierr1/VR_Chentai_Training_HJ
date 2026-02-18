using UnityEngine;

public class InkJetPrinter : MonoBehaviour
{
    [Header("Setup")]
    public Transform nozzlePoint;     // Titik ujung nozzle (Empty GameObject)
    public GameObject textPrefab;     // Prefab Teks/Codingan (misal: TextMeshPro)
    
    [Range(-0.05f, 0.1f)] 
    public float surfaceOffset = 0.001f;

    [Header("Settings")]
    public float maxDistance = 0.5f;  // Jarak maksimal semprotan (pendek aja)
    public float printDelay = 0.5f;   // Jeda waktu antar "tembakan" (detik)

    private float _nextPrintTime = 0f;

    void Update()
    {
        // Kita visualisasikan garis Raycast di Scene View biar gampang debug
        Debug.DrawRay(nozzlePoint.position, nozzlePoint.forward * maxDistance, Color.red);

        // Logika menembak
        // Cek apakah waktu sekarang sudah melewati waktu tunggu (delay)
        if (Time.time >= _nextPrintTime)
        {
            TryPrint();
        }
    }

    void TryPrint()
    {
        RaycastHit hit;

        // Tembakkan sinar gaib (Raycast) lurus ke depan dari Nozzle
        if (Physics.Raycast(nozzlePoint.position, nozzlePoint.forward, out hit, maxDistance))
        {
            // Cek apakah yang kena adalah Alufoil
            // Pastikan objek Alufoil kamu sudah dikasih Tag "Alufoil"
            if (hit.collider.CompareTag("Alufoil"))
            {
                SpawnCode(hit);
                
                // Reset timer delay agar tidak spamming text di satu titik
                _nextPrintTime = Time.time + printDelay;
            }
        }
    }

    void SpawnCode(RaycastHit hitInfo)
    {
        // KITA UBAH RUMUS POSISINYA
        // Posisi = Titik kena raycast + (Arah pantul * jarak offset yang kamu atur)
        Vector3 spawnPos = hitInfo.point + (hitInfo.normal * surfaceOffset);

        // Rotasi: Agar teks menghadap keluar dari permukaan
        // Jika teks terbalik (mirror), hapus tanda minus (-) di depan hitInfo.normal
        Quaternion spawnRot = Quaternion.LookRotation(hitInfo.normal);
        
        // PENTING: Kadang TextMeshPro perlu diputar 180 derajat kalau madep belakang
        // Coba nyalakan baris di bawah ini kalau teks-nya membelakangi kamera
        // spawnRot *= Quaternion.Euler(0, 180, 0); 

        GameObject newText = Instantiate(textPrefab, spawnPos, spawnRot);
        newText.transform.SetParent(hitInfo.transform);
    }
}
