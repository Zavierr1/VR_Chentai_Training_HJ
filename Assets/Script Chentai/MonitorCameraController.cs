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
    public SnapGroupManager managerPartA;
    public SnapGroupManager managerPartB;
    public SnapGroupManager managerPartC;

    [Header("Referensi Tombol UI Utama")]
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;
    public Button tombolPanelControl; 
    public Button tombolBack;         
    public Button tombolNext;

    // ==========================================
    // >>> VARIABEL SISTEM LEGENDA & INFO (YANG SEMPAT HILANG)
    // ==========================================
    [Header("Referensi UI Legenda (Blueprint)")]
    public TextMeshProUGUI textLegenda;
    public GameObject grupTitikPartA;
    public GameObject grupTitikPartB;
    public GameObject grupTitikPartC;

    [Header("Referensi Tombol INFO (Edukasi)")]
    public Button tombolInfoAreaAtas;
    public Button tombolInfoAreaTengah;
    public Button tombolInfoAreaBawah;

    [Header("Target Posisi Kamera INFO")]
    public Transform targetInfoAtas;
    public Transform targetInfoTengah;
    public Transform targetInfoBawah;
    // ==========================================

    [Header("Target Posisi (Waypoints Utama)")]
    public Transform targetDefault;
    public Transform targetPartA;
    public Transform targetPartB;
    public Transform targetPartC;
    public Transform targetPanelControl;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;
    private int tahapPerakitan = 1; 

    // Pengingat Buku Panduan
    private bool sudahLihatInfoAtas = false;
    private bool sudahLihatInfoTengah = false;
    private bool sudahLihatInfoBawah = false;

    void Start()
    {
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }
        KePosisiDefault();
    }

    // --- LOGIKA CLEAN UI ---
    private void RefreshTampilanTombolDefault()
    {
        if (tombolBack != null) tombolBack.gameObject.SetActive(false); 
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 
        
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(true); 
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(true);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(true);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(true);

        // Pastikan tombol Info menyala di menu utama
        if (tombolInfoAreaAtas != null) tombolInfoAreaAtas.gameObject.SetActive(true);
        if (tombolInfoAreaTengah != null) tombolInfoAreaTengah.gameObject.SetActive(true);
        if (tombolInfoAreaBawah != null) tombolInfoAreaBawah.gameObject.SetActive(true);

        // Logika Pengunci: Cek apakah semua buku panduan sudah dibuka minimal 1 kali?
        bool semuaInfoSudahDibaca = sudahLihatInfoAtas && sudahLihatInfoTengah && sudahLihatInfoBawah;

        if (tombolPartA != null) tombolPartA.interactable = semuaInfoSudahDibaca && (tahapPerakitan == 1);
        if (tombolPartB != null) tombolPartB.interactable = semuaInfoSudahDibaca && (tahapPerakitan == 2);
        if (tombolPartC != null) tombolPartC.interactable = semuaInfoSudahDibaca && (tahapPerakitan == 3);
        
        if (tombolPanelControl != null) tombolPanelControl.interactable = true;
    }

    private void KunciSemuaTombolKecualiBack()
    {
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(false);
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 

        // Matikan tombol info
        if (tombolInfoAreaAtas != null) tombolInfoAreaAtas.gameObject.SetActive(false);
        if (tombolInfoAreaTengah != null) tombolInfoAreaTengah.gameObject.SetActive(false);
        if (tombolInfoAreaBawah != null) tombolInfoAreaBawah.gameObject.SetActive(false);

        if (tombolBack != null) 
        {
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true;
        }
    }

    private void MatikanSemuaTitikLegenda()
    {
        // Fungsi ini sempat hilang, ini untuk mematikan titik A, B, C di layar
        if (grupTitikPartA != null) grupTitikPartA.SetActive(false);
        if (grupTitikPartB != null) grupTitikPartB.SetActive(false);
        if (grupTitikPartC != null) grupTitikPartC.SetActive(false);
    }

    private void MatikanSemuaHighlight()
    {
        if (managerPartA != null) managerPartA.MatikanHighlight();
        if (managerPartB != null) managerPartB.MatikanHighlight();
        if (managerPartC != null) managerPartC.MatikanHighlight();
    }

    // --- FUNGSI NAVIGASI KAMERA ---
    public void KePosisiDefault() 
    { 
        MulaiPindahKamera(targetDefault); 
        RefreshTampilanTombolDefault();
        MatikanSemuaTitikLegenda();
        if (textLegenda != null) textLegenda.text = "";

        bool semuaInfoSudahDibaca = sudahLihatInfoAtas && sudahLihatInfoTengah && sudahLihatInfoBawah;

        if (!semuaInfoSudahDibaca) 
        {
            UpdateTeksUI("SISTEM TERKUNCI.\nHarap buka semua Buku Panduan (Tombol Info) terlebih dahulu.");
        }
        else 
        {
            if (tahapPerakitan == 1) UpdateTeksUI("SISTEM OFFLINE.\nTekan [Part A] untuk memulai perakitan.");
            else if (tahapPerakitan == 2) UpdateTeksUI("PROGRES: 33%.\nTekan [Part B] untuk melanjutkan.");
            else if (tahapPerakitan == 3) UpdateTeksUI("PROGRES: 66%.\nTekan [Part C] untuk melanjutkan.");
            else UpdateTeksUI("PROGRES: 100%.\nPerakitan selesai. Silakan cek Control Panel.");
        }
    }

    // --- FUNGSI BUKU PANDUAN (INFO) ---
    public void LihatInfoAreaAtas()
    {
        sudahLihatInfoAtas = true; 
        MulaiPindahKamera(targetInfoAtas); 
        KunciSemuaTombolKecualiBack();     
        MatikanSemuaTitikLegenda();
        if (textLegenda != null) textLegenda.text = ""; 
        UpdateTeksUI("INFO AREA ATAS:\nMenjelaskan komponen bawaan mesin di area ini.");
    }

    public void LihatInfoAreaTengah()
    {
        sudahLihatInfoTengah = true; 
        MulaiPindahKamera(targetInfoTengah); 
        KunciSemuaTombolKecualiBack();     
        MatikanSemuaTitikLegenda();
        if (textLegenda != null) textLegenda.text = ""; 
        UpdateTeksUI("INFO AREA TENGAH:\nMenjelaskan komponen bawaan mesin di area ini.");
    }

    public void LihatInfoAreaBawah()
    {
        sudahLihatInfoBawah = true; 
        MulaiPindahKamera(targetInfoBawah); 
        KunciSemuaTombolKecualiBack();     
        MatikanSemuaTitikLegenda();
        if (textLegenda != null) textLegenda.text = ""; 
        UpdateTeksUI("INFO AREA BAWAH:\nMenjelaskan komponen bawaan mesin di area ini.");
    }
    
    // --- FUNGSI PERAKITAN ---
    public void KePartA() 
    { 
        MulaiPindahKamera(targetPartA); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("TUGAS: Pasang Part A."); 
        
        MatikanSemuaTitikLegenda();
        if (grupTitikPartA != null) grupTitikPartA.SetActive(true);
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Hopper\nB. Feeder\nC. Vibrator";

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

        MatikanSemuaTitikLegenda();
        if (grupTitikPartB != null) grupTitikPartB.SetActive(true);
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Komponen B1\nB. Komponen B2";
        
        if (managerPartB != null) 
        {
            managerPartB.AktifkanGrup();
            managerPartB.UpdateHighlightBerurutan();
        }
    }

    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("TUGAS: Pasang Part C."); 

        MatikanSemuaTitikLegenda();
        if (grupTitikPartC != null) grupTitikPartC.SetActive(true);
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Komponen C1";
        
        if (managerPartC != null) 
        {
            managerPartC.AktifkanGrup();
            managerPartC.UpdateHighlightBerurutan();
        }
    }

    public void KePanelControl() 
    { 
        MulaiPindahKamera(targetPanelControl); 
        KunciSemuaTombolKecualiBack(); 
        UpdateTeksUI("INFO: Panel Kontrol Utama.");
        MatikanSemuaTitikLegenda();
        if (textLegenda != null) textLegenda.text = "";
    }

    // --- LOGIKA PROGRESS ---
    public void PartSelesai()
    {
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(false);
        if (tombolBack != null) tombolBack.gameObject.SetActive(false);

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
        
        if (tahapPerakitan == 2 && managerPartB != null) managerPartB.AktifkanGrup();
        else if (tahapPerakitan == 3 && managerPartC != null) managerPartC.AktifkanGrup();

        KePosisiDefault(); 
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