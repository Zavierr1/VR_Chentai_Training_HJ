using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Wajib dipanggil untuk mengontrol Button UI
using TMPro;

public class MonitorCameraController : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Transform cctvCamera;
    public TextMeshProUGUI textInstruksi;

    [Header("Referensi Tombol UI")]
    [Tooltip("Masukkan tombol Part A, B, C dari Canvas ke sini")]
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;

    [Header("Target Posisi (Waypoints)")]
    public Transform targetDefault;
    public Transform targetPartA;
    public Transform targetPartB;
    public Transform targetPartC;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;

    void Start()
    {
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }

        // KONDISI AWAL TUTORIAL: Hanya tombol Part A yang bisa diklik
        if (tombolPartA != null) tombolPartA.interactable = true;
        if (tombolPartB != null) tombolPartB.interactable = false; // Kunci Part B
        if (tombolPartC != null) tombolPartC.interactable = false; // Kunci Part C

        UpdateTeksUI("SISTEM OFFLINE. Tekan tombol [Part A] yang menyala untuk memulai.");
    }

    // --- FUNGSI NAVIGASI KAMERA ---
    public void KePosisiDefault() { MulaiPindahKamera(targetDefault); }
    public void KePartA() { MulaiPindahKamera(targetPartA); UpdateTeksUI("TUGAS: Pasang Part A."); }
    public void KePartB() { MulaiPindahKamera(targetPartB); UpdateTeksUI("TUGAS: Pasang Part B."); }
    public void KePartC() { MulaiPindahKamera(targetPartC); UpdateTeksUI("TUGAS: Pasang Part C."); }

    // --- FUNGSI UNTUK MEMBUKA KUNCI TOMBOL BERIKUTNYA ---
    
    // Panggil fungsi ini di BNG OnSnapEvent() milik Snap Zone Part A
    public void BukaKunciPartB()
    {
        KePosisiDefault(); // Kembalikan kamera ke view luas
        UpdateTeksUI("BAGUS! Part A terpasang. Sekarang tekan tombol [Part B].");
        
        // Matikan tombol A agar tidak diklik lagi, nyalakan tombol B
        if (tombolPartA != null) tombolPartA.interactable = false;
        if (tombolPartB != null) tombolPartB.interactable = true; 
    }

    // Panggil fungsi ini di BNG OnSnapEvent() milik Snap Zone Part B
    public void BukaKunciPartC()
    {
        KePosisiDefault(); 
        UpdateTeksUI("LUAR BIASA! Part B terpasang. Lanjutkan ke [Part C].");
        
        if (tombolPartB != null) tombolPartB.interactable = false;
        if (tombolPartC != null) tombolPartC.interactable = true; 
    }

    private void UpdateTeksUI(string pesan)
    {
        if (textInstruksi != null) textInstruksi.text = pesan;
    }

    private void MulaiPindahKamera(Transform targetTujuan)
    {
        if (cctvCamera == null || targetTujuan == null) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(ProsesPindahKamera(targetTujuan));
    }

    private IEnumerator ProsesPindahKamera(Transform target)
    {
        Vector3 posisiAwal = cctvCamera.position;
        Quaternion rotasiAwal = cctvCamera.rotation;
        float timer = 0f;

        while (timer < transisiDurasi)
        {
            timer += Time.deltaTime;
            float persen = Mathf.SmoothStep(0f, 1f, timer / transisiDurasi);
            cctvCamera.position = Vector3.Lerp(posisiAwal, target.position, persen);
            cctvCamera.rotation = Quaternion.Slerp(rotasiAwal, target.rotation, persen);
            yield return null;
        }
        cctvCamera.position = target.position;
        cctvCamera.rotation = target.rotation;
    }
}