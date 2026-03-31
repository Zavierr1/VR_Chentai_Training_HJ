using System.Collections;
using UnityEngine;

public class NPCFactoryShow : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Animator anim;
    
    [Tooltip("Kecepatan dorong badan (Parent) NPC")]
    public float walkSpeed = 1.2f;
    [Tooltip("Kecepatan putar badan NPC")]
    public float turnSpeed = 5.0f;

    [Header("Pengaturan Navigasi (Murni Timer & Sudut)")]
    [Tooltip("Berapa detik NPC jalan lurus ke mesin?")]
    public float durasiJalanKeMesin = 2.0f;
    
    [Tooltip("Berapa derajat NPC harus putar badan setelah dari mesin? (180 = balik badan)")]
    public float sudutPutaranJoget = 180f;
    
    [Tooltip("Berapa detik NPC jalan ke area joget setelah putar badan?")]
    public float durasiJalanKeAreaJoget = 2.0f;

    [Header("Durasi Animasi")]
    public float durasiPencetTombol = 2.0f;
    public float durasiTwerk = 4.0f;
    public float durasiBackflip = 1.5f;
    public float durasiProsesDuduk = 2.0f;

    [Header("Debug")]
    public bool autoStartOnPlay = true;

    void Start()
    {
        if (autoStartOnPlay)
            MesinSelesaiDiperbaiki();
    }

    public void MesinSelesaiDiperbaiki()
    {
        StartCoroutine(SequenceFullPakaiTimer());
    }

    private IEnumerator SequenceFullPakaiTimer()
    {
        // --- OBAT ANTI ERROR LAYER -1 ---
        yield return new WaitForSeconds(0.1f); 

        // 1. JALAN KE MESIN
        Debug.Log("[NPC] 1. Jalan lurus ke mesin...");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeMesin));

        // 2. PENCET TOMBOL
        Debug.Log("[NPC] 2. Pencet tombol!");
        anim.ResetTrigger("PushButton");
        anim.SetTrigger("PushButton");
        yield return new WaitForSeconds(durasiPencetTombol);

        // 3. PUTAR BADAN (Cari posisi kosong)
        Debug.Log("[NPC] 3. Putar badan...");
        yield return StartCoroutine(PutarBadan(sudutPutaranJoget));

        // 4. JALAN KE AREA JOGET
        Debug.Log("[NPC] 4. Jalan ke area joget...");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeAreaJoget));

        // 5. TWERK
        Debug.Log("[NPC] 5. Twerk!");
        anim.ResetTrigger("Twerk");
        anim.SetTrigger("Twerk");
        yield return new WaitForSeconds(durasiTwerk);

        // 6. BACKFLIP
        Debug.Log("[NPC] 6. Salto!");
        anim.ResetTrigger("Backflip");
        anim.SetTrigger("Backflip");
        yield return new WaitForSeconds(durasiBackflip);

        // 7. PERSIAPAN DUDUK
        // Karena tidak pakai target kursi, dia akan langsung duduk di tempat terakhir dia berdiri
        Debug.Log("[NPC] 7. Duduk di tempat.");
        anim.ResetTrigger("StandToSit");
        anim.SetTrigger("StandToSit");
        yield return new WaitForSeconds(durasiProsesDuduk);

        // 8. MULAI KERJA
        Debug.Log("[NPC] 8. Mulai kerja.");
        anim.ResetTrigger("SitWork");
        anim.SetTrigger("SitWork");
    }

    // --- FUNGSI MENDORONG PARENT MAJU PAKAI TIMER ---
    private IEnumerator JalanMaju(float durasi)
    {
        float timer = 0f;
        while (timer < durasi)
        {
            timer += Time.deltaTime;
            
            // Mendorong badannya lurus ke arah dia menghadap
            transform.position += transform.forward * walkSpeed * Time.deltaTime;
            
            yield return null; 
        }
    }

    // --- FUNGSI PUTAR BADAN PAKAI SUDUT ---
    private IEnumerator PutarBadan(float sudutTambahan)
    {
        // Kalkulasi target rotasi (Putar Y sesuai input)
        Quaternion targetRotasi = transform.rotation * Quaternion.Euler(0, sudutTambahan, 0);
        
        while (Quaternion.Angle(transform.rotation, targetRotasi) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, turnSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Kunci rotasi biar pas
        transform.rotation = targetRotasi; 
    }
}