using System.Collections;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class MonitorCameraController : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Transform cctvCamera;
    public TextMeshProUGUI textInstruksi;

    [Header("Referensi Fisik (SnapGroupManager)")]
    [Tooltip("Masukkan Part2_Manager dan Part3_Manager ke sini")]
    public SnapGroupManager managerPartA;
    public SnapGroupManager managerPartB;
    public SnapGroupManager managerPartC;

    [Header("Referensi Tombol UI")]
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;
    public Button tombolPanelControl; 
    public Button tombolBack;         
    public Button tombolNext;

    [Header("Target Posisi (Waypoints)")]
    public Transform targetDefault;
    public Transform targetPartA;
    public Transform targetPartB;
    public Transform targetPartC;
    public Transform targetPanelControl;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;
    private int tahapPerakitan = 1; 

    void Start()
    {
        // Pastikan posisi awal kamera benar
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }

        // Jalankan kondisi awal (Default View)
        KePosisiDefault();
    }

    // --- LOGIKA CLEAN UI (SET ACTIVE & INTERACTABLE) ---

    private void RefreshTampilanTombolDefault()
    {
        // 1. Tombol Back & Next HILANG (Clean)
        if (tombolBack != null) tombolBack.gameObject.SetActive(false); 
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 
        
        // 2. Tombol Navigasi Utama MUNCUL
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(true); 
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(true);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(true);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(true);

        // 3. Roadmap Logic: Tombol Part B & C tetap kelihatan tapi tidak bisa diklik jika belum tahapnya
        if (tombolPartA != null) tombolPartA.interactable = (tahapPerakitan == 1);
        if (tombolPartB != null) tombolPartB.interactable = (tahapPerakitan == 2);
        if (tombolPartC != null) tombolPartC.interactable = (tahapPerakitan == 3);
        
        if (tombolPanelControl != null) tombolPanelControl.interactable = true;
    }

    private void KunciSemuaTombolKecualiBack()
    {
        // SEMUA tombol navigasi hilang agar layar bersih saat zoom
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(false);
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 

        // HANYA tombol Back yang muncul dan bisa diklik
        if (tombolBack != null) 
        {
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true;
        }
    }

    // --- FUNGSI NAVIGASI KAMERA ---
    
    private void MatikanSemuaHighlight()
    {
        // Matikan kedipan via Manager
        if (managerPartA != null) managerPartA.MatikanHighlight();
        if (managerPartB != null) managerPartB.MatikanHighlight();
        if (managerPartC != null) managerPartC.MatikanHighlight();
    }
    public void KePosisiDefault() 
    { 
        MulaiPindahKamera(targetDefault); 
        RefreshTampilanTombolDefault();

        if (tahapPerakitan == 1) UpdateTeksUI("SISTEM OFFLINE.\nTekan [Part A] untuk memulai.");
        else if (tahapPerakitan == 2) UpdateTeksUI("PROGRES: 33%.\nTekan [Part B] untuk melanjutkan.");
        else if (tahapPerakitan == 3) UpdateTeksUI("PROGRES: 66%.\nTekan [Part C] untuk melanjutkan.");
        else UpdateTeksUI("PROGRES: 100%.\nPerakitan selesai. Silakan cek Control Panel.");
    }
    
    public void KePartA() 
    { 
        MulaiPindahKamera(targetPartA); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("TUGAS: Pasang Part A."); 
        
        // >>> TAMBAHAN BARU: Sekarang saat tombol diklik, Part A baru bisa di-grab & snap
        if (managerPartA != null) 
        {
            managerPartA.AktifkanGrup(); 
            managerPartA.UpdateHighlightBerurutan(); 
        }
    }

    public void KePartB() 
    { 
        MulaiPindahKamera(targetPartB); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("TUGAS: Pasang Part B."); 
        
        // [!!!] TAMBAHAN: Suruh Part B kelap-kelip sekarang!
        if (managerPartB != null) managerPartB.UpdateHighlightBerurutan();
    }

    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("TUGAS: Pasang Part C."); 
        
        // [!!!] TAMBAHAN: Suruh Part C kelap-kelip sekarang!
        if (managerPartC != null) managerPartC.UpdateHighlightBerurutan();
    }

    public void KePanelControl() 
    { 
        MulaiPindahKamera(targetPanelControl); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("INFO: Panel Kontrol Utama.");
    }

    // --- LOGIKA PROGRESS ---
    
    public void PartSelesai()
    {
        // >>> PERBAIKAN: Sapu bersih SEMUA tombol agar player tidak bisa iseng klik Part A lagi
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(false);
        if (tombolBack != null) tombolBack.gameObject.SetActive(false);

        // HANYA tombol Next yang boleh muncul
        if (tombolNext != null) 
        {
            tombolNext.gameObject.SetActive(true);
            tombolNext.interactable = true;
        }

        UpdateTeksUI("BAGUS! Komponen terpasang.\nTekan [NEXT] untuk lanjut.");
    }

    public void LanjutKeTahapBerikutnya()
    {
        tahapPerakitan++; 
        
        // >>> LOGIKA BARU: Buka gembok fisik part HANYA saat tombol Next diklik!
        if (tahapPerakitan == 2 && managerPartB != null) 
        {
            managerPartB.AktifkanGrup();
        }
        else if (tahapPerakitan == 3 && managerPartC != null) 
        {
            managerPartC.AktifkanGrup();
        }

        KePosisiDefault(); // Mundur ke layar utama // Akan memanggil RefreshTampilanTombolDefault (Next hilang lagi)
    }

    // --- SISTEM PERGERAKAN ---
    private void UpdateTeksUI(string pesan) { if (textInstruksi != null) textInstruksi.text = pesan; }

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