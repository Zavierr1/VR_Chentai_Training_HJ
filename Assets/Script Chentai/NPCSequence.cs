using System.Collections;
using UnityEngine;

public class NPCFactoryShow : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Animator anim;
    
    [Tooltip("Kecepatan jalan NPC (Meter per detik)")]
    public float walkSpeed = 1.2f;
    [Tooltip("Kecepatan putar badan NPC")]
    public float turnSpeed = 5.0f;

    [Header("Titik Lokasi (Empty GameObject)")]
    public Transform titikMesin;
    public Transform titikTwerk;
    public Transform titikKursi; // Posisikan Empty GameObject ini di tempat NPC harus mulai proses duduk

    [Header("Durasi Animasi (Sesuaikan dengan durasi klip Mixamo)")]
    public float durasiPencetTombol = 2.0f;
    public float durasiTwerk = 4.0f;
    public float durasiBackflip = 1.5f;
    public float durasiProsesDuduk = 2.0f; // Waktu yang dibutuhkan animasi Sit_To_Stand untuk selesai

    public void MesinSelesaiDiperbaiki()
    {
        StartCoroutine(MulaiSequenceKocak());
    }

    private IEnumerator MulaiSequenceKocak()
    {
        // 1. JALAN KE MESIN
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanKeTarget(titikMesin));

        // 2. PENCET TOMBOL
        anim.SetTrigger("PushButton");
        yield return new WaitForSeconds(durasiPencetTombol);

        // 3. JALAN KE AREA JOGET
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanKeTarget(titikTwerk));

        // 4. MENGHADAP KE DEPAN SEBELUM JOGET
        yield return StartCoroutine(PutarBadan(titikTwerk.rotation));

        // 5. TWERK
        anim.SetTrigger("Twerk");
        yield return new WaitForSeconds(durasiTwerk);

        // 6. BACKFLIP
        anim.SetTrigger("Backflip");
        yield return new WaitForSeconds(durasiBackflip);

        // 7. PERSIAPAN DUDUK
        // Teleport root NPC ke titik kursi agar posisinya akurat 100%
        transform.position = titikKursi.position;
        transform.rotation = titikKursi.rotation;
        
        // Panggil animasi duduk (yang sudah di-reverse speed-nya jadi -1)
        anim.SetTrigger("StandToSit");
        yield return new WaitForSeconds(durasiProsesDuduk);

        // 8. MULAI KERJA (Looping di atas meja)
        anim.SetTrigger("SitWork");
    }

    // --- FUNGSI BANTUAN UNTUK PERGERAKAN ---

    private IEnumerator JalanKeTarget(Transform target)
    {
        Vector3 posisiTujuan = new Vector3(target.position.x, transform.position.y, target.position.z);

        while (Vector3.Distance(transform.position, posisiTujuan) > 0.1f)
        {
            Vector3 arah = (posisiTujuan - transform.position).normalized;
            if (arah != Vector3.zero)
            {
                Quaternion rotasiTujuan = Quaternion.LookRotation(arah);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotasiTujuan, turnSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, posisiTujuan, walkSpeed * Time.deltaTime);
            yield return null; 
        }
    }

    private IEnumerator PutarBadan(Quaternion targetRotasi)
    {
        while (Quaternion.Angle(transform.rotation, targetRotasi) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotasi; 
    }
}   