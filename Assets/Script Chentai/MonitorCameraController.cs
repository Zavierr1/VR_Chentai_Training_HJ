using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

[System.Serializable]
public class SlideInfo
{
    [TextArea] public string teksLegenda;
    public GameObject uiTitik; 
    public List<KelapKelipTutorial> mesinHighlight; 
}

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
    public Button tombolStartTutorial; 
    public Button tombolPanduanPojokKanan; 
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;
    public Button tombolPanelControl; 
    public Button tombolBack;         
    public Button tombolNext;

    // ==========================================
    // >>> SISTEM SLIDESHOW ALUR LINEAR
    // ==========================================
    [Header("Data Slideshow Buku Panduan")]
    public GameObject panelLegenda;
    public TextMeshProUGUI textLegenda;
    public Button tombolNextSlide; 
    public Button tombolPrevSlide; 

    [Tooltip("Centang untuk melihat SEMUA titik A,B,C,D,E sekaligus. Hilangkan centang untuk melihat 1 per 1 sesuai slide.")]
    public bool tampilkanSemuaTitik = false;

    public List<SlideInfo> slideAreaAtas;
    public List<SlideInfo> slideAreaBawah; 

    private List<SlideInfo> slideAktif = new List<SlideInfo>();
    private int indeksSlideSekarang = 0;
    private int areaTutorialAktif = 0; // 0 = Atas, 1 = Bawah
    // ==========================================

    [Header("Target Posisi (Waypoints)")]
    public Transform targetDefault;
    public Transform targetInfoAtas;
    public Transform targetInfoBawah;
    public Transform targetPartA;
    public Transform targetPartB;
    public Transform targetPartC;
    public Transform targetPanelControl;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;
    private int tahapPerakitan = 1; 

    private bool tutorialSelesai = false;

    void Start()
    {
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }
        
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(false);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(false);
        if (panelLegenda != null) panelLegenda.SetActive(false);

        KePosisiDefault();
    }

    private void RefreshTampilanTombolDefault()
    {
        KunciSemuaTombol(); 

        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(true); 

        if (!tutorialSelesai)
        {
            if (tombolStartTutorial != null) tombolStartTutorial.gameObject.SetActive(true);
        }
        else
        {
            if (tombolPartA != null) { tombolPartA.gameObject.SetActive(true); tombolPartA.interactable = (tahapPerakitan == 1); }
            if (tombolPartB != null) { tombolPartB.gameObject.SetActive(true); tombolPartB.interactable = (tahapPerakitan == 2); }
            if (tombolPartC != null) { tombolPartC.gameObject.SetActive(true); tombolPartC.interactable = (tahapPerakitan == 3); }
            
            if (tombolPanduanPojokKanan != null) tombolPanduanPojokKanan.gameObject.SetActive(true);
        }
    }

    private void KunciSemuaTombol()
    {
        if (tombolStartTutorial != null) tombolStartTutorial.gameObject.SetActive(false);
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolPanelControl != null) tombolPanelControl.gameObject.SetActive(false);
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 
        if (tombolBack != null) tombolBack.gameObject.SetActive(false); 
        if (tombolPanduanPojokKanan != null) tombolPanduanPojokKanan.gameObject.SetActive(false);
    }

    public void MulaiTutorial()
    {
        KunciSemuaTombol();
        
        if (panelLegenda != null) panelLegenda.SetActive(true);
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("INFO AREA ATAS:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
        
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);

        // Jika opsi nyala, langsung tampilkan semua titik untuk area atas
        if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaAtas);

        TampilkanSlideSekarang();
    }

    public void NextSlide()
    {
        if (indeksSlideSekarang < slideAktif.Count - 1)
        {
            indeksSlideSekarang++;
            TampilkanSlideSekarang();
        }
        else
        {
            if (areaTutorialAktif == 0) 
            {
                // SEBELUM PINDAH: Matikan semua titik area atas secara paksa
                MatikanSemuaTitikDiArea(slideAreaAtas);

                areaTutorialAktif = 1;
                slideAktif = slideAreaBawah;
                indeksSlideSekarang = 0;
                
                MulaiPindahKamera(targetInfoBawah);
                UpdateTeksUI("INFO AREA BAWAH:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
                
                // NYALAKAN TITIK AREA BAWAH
                if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaBawah);

                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1)
            {
                tutorialSelesai = true;
                MatikanSemuaSlideshow();
                KePosisiDefault();
            }
        }
    }

    public void PrevSlide()
    {
        if (indeksSlideSekarang > 0)
        {
            indeksSlideSekarang--;
            TampilkanSlideSekarang();
        }
        else
        {
            if (areaTutorialAktif == 1)
            {
                // SEBELUM PINDAH: Matikan semua titik area bawah secara paksa
                MatikanSemuaTitikDiArea(slideAreaBawah);

                areaTutorialAktif = 0;
                slideAktif = slideAreaAtas;
                indeksSlideSekarang = slideAktif.Count - 1; 
                
                MulaiPindahKamera(targetInfoAtas);
                UpdateTeksUI("INFO AREA ATAS:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
                
                // NYALAKAN TITIK AREA ATAS KEMBALI
                if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaAtas);

                TampilkanSlideSekarang();
            }
        }
    }

    private void TampilkanSlideSekarang()
    {
        MatikanLampuSlide(slideAreaAtas);
        MatikanLampuSlide(slideAreaBawah);

        if (slideAktif.Count == 0) return;

        SlideInfo slideSekarang = slideAktif[indeksSlideSekarang];
        
        // Nyalakan titik slide sekarang (aman dilakukan walau tampilkanSemuaTitik nyala)
        if (slideSekarang.uiTitik != null) slideSekarang.uiTitik.SetActive(true);
        
        if (textLegenda != null) textLegenda.text = slideSekarang.teksLegenda;
        if (slideSekarang.mesinHighlight != null)
        {
            foreach (var highlight in slideSekarang.mesinHighlight)
            {
                if (highlight != null) highlight.MulaiKedip();
            }
        }

        bool bisaMundur = !(areaTutorialAktif == 0 && indeksSlideSekarang == 0);
        if (tombolPrevSlide != null) tombolPrevSlide.interactable = bisaMundur;
    }

    private void MatikanSemuaSlideshow()
    {
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(false);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(false);
        if (textLegenda != null) textLegenda.text = "";
        if (panelLegenda != null) panelLegenda.SetActive(false);

        MatikanSemuaTitikDiArea(slideAreaAtas);
        MatikanSemuaTitikDiArea(slideAreaBawah);
        
        // Pastikan highlight kelap-kelip mati
        MatikanLampuSlide(slideAreaAtas);
        MatikanLampuSlide(slideAreaBawah);
    }

    // Fungsi khusus untuk mengatur kedip material (dan matikan titik JIKA mode 1 per 1 dipilih)
    private void MatikanLampuSlide(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
        {
            // Matikan titik hanya jika opsi tampilkanSemuaTitik TIDAK dicentang
            if (!tampilkanSemuaTitik && slide.uiTitik != null) 
            {
                slide.uiTitik.SetActive(false);
            }

            if (slide.mesinHighlight != null)
            {
                foreach (var highlight in slide.mesinHighlight)
                {
                    if (highlight != null) highlight.BerhentiKedip();
                }
            }
        }
    }

    // Helper Functions untuk kontrol titik masal
    private void NyalakanSemuaTitikDiArea(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
            if (slide.uiTitik != null) slide.uiTitik.SetActive(true);
    }

    private void MatikanSemuaTitikDiArea(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
            if (slide.uiTitik != null) slide.uiTitik.SetActive(false);
    }

    public void KePosisiDefault() 
    { 
        MulaiPindahKamera(targetDefault); 
        RefreshTampilanTombolDefault();
        MatikanSemuaSlideshow();

        if (!tutorialSelesai) 
        {
            UpdateTeksUI("SISTEM TERKUNCI.\nTekan [MULAI TUTORIAL] untuk mempelajari mesin.");
        }
        else 
        {
            if (tahapPerakitan == 1) UpdateTeksUI("SISTEM OFFLINE.\nTekan [Part A] untuk memulai perakitan.");
            else if (tahapPerakitan == 2) UpdateTeksUI("PROGRES: 33%.\nTekan [Part B] untuk melanjutkan.");
            else if (tahapPerakitan == 3) UpdateTeksUI("PROGRES: 66%.\nTekan [Part C] untuk melanjutkan.");
            else UpdateTeksUI("PROGRES: 100%.\nPerakitan selesai. Silakan cek Control Panel.");
        }
    }
    
    public void KePartA() 
    { 
        MulaiPindahKamera(targetPartA); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();
        
        UpdateTeksUI("TUGAS: Pasang Part A."); 
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Hopper\nB. Feeder\nC. Vibrator";

        if (managerPartA != null) { managerPartA.AktifkanGrup(); managerPartA.UpdateHighlightBerurutan(); }
    }

    public void KePartB() 
    { 
        MulaiPindahKamera(targetPartB); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();

        UpdateTeksUI("TUGAS: Pasang Part B."); 
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Komponen B1\nB. Komponen B2";
        
        if (managerPartB != null) { managerPartB.AktifkanGrup(); managerPartB.UpdateHighlightBerurutan(); }
    }

    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();

        UpdateTeksUI("TUGAS: Pasang Part C."); 
        if (textLegenda != null) textLegenda.text = "<color=yellow>DAFTAR KOMPONEN:</color>\nA. Komponen C1";
        
        if (managerPartC != null) { managerPartC.AktifkanGrup(); managerPartC.UpdateHighlightBerurutan(); }
    }

    public void KePanelControl() 
    { 
        MulaiPindahKamera(targetPanelControl); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();
        UpdateTeksUI("INFO: Panel Kontrol Utama.");
    }

    public void PartSelesai()
    {
        KunciSemuaTombol();
        if (tombolNext != null) { tombolNext.gameObject.SetActive(true); tombolNext.interactable = true; }
        UpdateTeksUI("BAGUS! Komponen terpasang.\nTekan [NEXT] untuk lanjut.");
    }

    public void LanjutKeTahapBerikutnya()
    {
        tahapPerakitan++; 
        KePosisiDefault(); 
    }

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