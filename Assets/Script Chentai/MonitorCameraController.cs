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
    // >>> SISTEM SLIDESHOW BUKU PANDUAN (KANAN)
    // ==========================================
    [Header("Data Slideshow Buku Panduan (Kanan)")]
    public GameObject panelLegenda;
    public TextMeshProUGUI textLegenda;
    public Button tombolNextSlide; 
    public Button tombolPrevSlide; 

    public List<SlideInfo> slideAreaAtas;
    public List<SlideInfo> slideAreaBawah; 

    // ==========================================
    // >>> SISTEM SLIDESHOW CONTROL PANEL (KIRI)
    // ==========================================
    [Header("Data Slideshow Panel Control (Kiri)")]
    public GameObject panelLegendaKiri;
    public TextMeshProUGUI textLegendaKiri;
    public Button tombolNextSlideKiri; 
    public Button tombolPrevSlideKiri; 
    
    public List<SlideInfo> slidePanelControl;

    [Header("Pengaturan Tampilan Titik")]
    [Tooltip("Centang untuk melihat SEMUA titik A,B,C sekaligus. Hilangkan centang untuk 1 per 1.")]
    public bool tampilkanSemuaTitik = false;

    // Variabel Navigasi Internal
    private List<SlideInfo> slideAktif = new List<SlideInfo>();
    private int indeksSlideSekarang = 0;
    private int areaTutorialAktif = 0; // 0 = Atas, 1 = Bawah, 2 = Panel Control Kiri

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
        
        MatikanSemuaSlideshow();
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
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("INFO AREA ATAS:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
        
        if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaAtas);
        TampilkanSlideSekarang();
    }

    public void KePanelControl() 
    { 
        MulaiPindahKamera(targetPanelControl); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        
        MatikanSemuaSlideshow();

        // Mulai mode slideshow Kiri
        areaTutorialAktif = 2; // 2 = Mode Panel Control
        indeksSlideSekarang = 0;
        slideAktif = slidePanelControl;

        UpdateTeksUI("INFO PANEL KONTROL:\nGunakan tombol panah [<] [>] untuk membaca panduan.");

        if (panelLegendaKiri != null) panelLegendaKiri.SetActive(true);
        if (tombolNextSlideKiri != null) tombolNextSlideKiri.gameObject.SetActive(true);
        if (tombolPrevSlideKiri != null) tombolPrevSlideKiri.gameObject.SetActive(true);

        if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slidePanelControl);
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
                MatikanSemuaTitikDiArea(slideAreaAtas);
                areaTutorialAktif = 1;
                slideAktif = slideAreaBawah;
                indeksSlideSekarang = 0;
                
                MulaiPindahKamera(targetInfoBawah);
                UpdateTeksUI("INFO AREA BAWAH:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
                
                if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaBawah);
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1)
            {
                tutorialSelesai = true;
                MatikanSemuaSlideshow();
                KePosisiDefault();
            }
            // Jika areaTutorialAktif == 2 (Panel Control), dibiarkan mentok saja karena keluarnya pakai tombol Back.
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
                MatikanSemuaTitikDiArea(slideAreaBawah);
                areaTutorialAktif = 0;
                slideAktif = slideAreaAtas;
                indeksSlideSekarang = slideAktif.Count - 1; 
                
                MulaiPindahKamera(targetInfoAtas);
                UpdateTeksUI("INFO AREA ATAS:\nGunakan tombol panah [<] [>] untuk membaca panduan.");
                
                if (tampilkanSemuaTitik) NyalakanSemuaTitikDiArea(slideAreaAtas);
                TampilkanSlideSekarang();
            }
            // Jika areaTutorialAktif == 2, mentok di slide pertama.
        }
    }

    private void TampilkanSlideSekarang()
    {
        MatikanLampuSlide(slideAreaAtas);
        MatikanLampuSlide(slideAreaBawah);
        MatikanLampuSlide(slidePanelControl);

        if (slideAktif.Count == 0) return;

        SlideInfo slideSekarang = slideAktif[indeksSlideSekarang];
        
        if (slideSekarang.uiTitik != null) slideSekarang.uiTitik.SetActive(true);
        
        if (slideSekarang.mesinHighlight != null)
        {
            foreach (var highlight in slideSekarang.mesinHighlight)
            {
                if (highlight != null) highlight.MulaiKedip();
            }
        }

        // Logic UI untuk Panel Kiri vs Kanan
        if (areaTutorialAktif == 2)
        {
            if (textLegendaKiri != null) textLegendaKiri.text = slideSekarang.teksLegenda;
            
            bool bisaMundur = (indeksSlideSekarang > 0);
            bool bisaMaju = (indeksSlideSekarang < slideAktif.Count - 1);
            
            if (tombolPrevSlideKiri != null) tombolPrevSlideKiri.interactable = bisaMundur;
            if (tombolNextSlideKiri != null) tombolNextSlideKiri.interactable = bisaMaju;
        }
        else
        {
            if (textLegenda != null) textLegenda.text = slideSekarang.teksLegenda;
            
            bool bisaMundur = !(areaTutorialAktif == 0 && indeksSlideSekarang == 0);
            bool bisaMaju = true; // Karena lanjut dari atas ke bawah
            
            if (tombolPrevSlide != null) tombolPrevSlide.interactable = bisaMundur;
            if (tombolNextSlide != null) tombolNextSlide.interactable = bisaMaju;
        }
    }

    private void MatikanSemuaSlideshow()
    {
        // Panel Kanan
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(false);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(false);
        if (textLegenda != null) textLegenda.text = "";
        if (panelLegenda != null) panelLegenda.SetActive(false);

        // Panel Kiri
        if (tombolNextSlideKiri != null) tombolNextSlideKiri.gameObject.SetActive(false);
        if (tombolPrevSlideKiri != null) tombolPrevSlideKiri.gameObject.SetActive(false);
        if (textLegendaKiri != null) textLegendaKiri.text = "";
        if (panelLegendaKiri != null) panelLegendaKiri.SetActive(false);

        MatikanSemuaTitikDiArea(slideAreaAtas);
        MatikanSemuaTitikDiArea(slideAreaBawah);
        MatikanSemuaTitikDiArea(slidePanelControl);
        
        MatikanLampuSlide(slideAreaAtas);
        MatikanLampuSlide(slideAreaBawah);
        MatikanLampuSlide(slidePanelControl);
    }

    private void MatikanLampuSlide(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
        {
            if (!tampilkanSemuaTitik && slide.uiTitik != null) slide.uiTitik.SetActive(false);

            if (slide.mesinHighlight != null)
            {
                foreach (var highlight in slide.mesinHighlight)
                {
                    if (highlight != null) highlight.BerhentiKedip();
                }
            }
        }
    }

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
        
        // UpdateTeksUI("TUGAS: Pasang Part A."); <--- HAPUS BARIS INI
        if (managerPartA != null) { managerPartA.AktifkanGrup(); managerPartA.UpdateHighlightBerurutan(); }
    }

    public void KePartB() 
    { 
        MulaiPindahKamera(targetPartB); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();

        // UpdateTeksUI("TUGAS: Pasang Part B."); <--- HAPUS BARIS INI
        if (managerPartB != null) { managerPartB.AktifkanGrup(); managerPartB.UpdateHighlightBerurutan(); }
    }

    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombol(); 
        if (tombolBack != null) { tombolBack.gameObject.SetActive(true); tombolBack.interactable = true; }
        MatikanSemuaSlideshow();

        // UpdateTeksUI("TUGAS: Pasang Part C."); <--- HAPUS BARIS INI
        if (managerPartC != null) { managerPartC.AktifkanGrup(); managerPartC.UpdateHighlightBerurutan(); }
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

    public void UpdateTeksUI(string pesan) { if (textInstruksi != null) textInstruksi.text = pesan; }

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