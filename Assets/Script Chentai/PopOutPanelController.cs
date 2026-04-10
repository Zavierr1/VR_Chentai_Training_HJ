using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopOutPanelController : MonoBehaviour
{
    [Header("Komponen Utama")]
    [Tooltip("Masukkan Canvas utama Pop-Out kamu ke sini")]
    public GameObject panelUIUtama; 
    
    [Tooltip("Kamera HD khusus yang merender gambar ke RawImage panel ini")]
    public Camera popOutCamera; 

    // >>> TAMBAHAN BARU: Sistem Layar Toggle
    [Header("Referensi Layar (Toggle 3D vs Asli)")]
    [Tooltip("GameObject RawImage yang menampilkan Render Texture 3D")]
    public GameObject layar3D;
    [Tooltip("GameObject Image yang menampilkan foto mesin asli")]
    public GameObject layarFotoAsli;

    [Header("Referensi UI Tombol")]
    public TextMeshProUGUI textLegenda;
    public Button tombolNext;
    public Button tombolPrev;
    public Button tombolClose; 
    
    // >>> TAMBAHAN BARU: Tombol Toggle
    [Tooltip("Tombol untuk mengganti mode 3D / Foto Asli")]
    public Button tombolToggleVisual; 

    [Header("Data Slideshow")]
    public List<SlideInfo> daftarSlide; 

    private int indeksSlideSekarang = 0;
    private bool sedangLihatFotoAsli = false; // Status saklar saat ini

    void Start()
    {
        HidePanel();
    }

    public void ShowPanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(true);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(true);

        // Pastikan saat panel baru dibuka, mode yang aktif selalu mode 3D
        sedangLihatFotoAsli = false;
        UpdateTampilanLayar();

        indeksSlideSekarang = 0;
        TampilkanSlideSekarang();
    }

    public void HidePanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(false);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(false);
        
        MatikanSemuaLampu();
    }

    // >>> FUNGSI BARU: Dipanggil saat tombol Toggle dipencet <<<
    public void ToggleTampilanVisual()
    {
        sedangLihatFotoAsli = !sedangLihatFotoAsli; // Balikkan status (True jadi False, False jadi True)
        UpdateTampilanLayar();
    }

    private void UpdateTampilanLayar()
    {
        // Nyala/matikan layar sesuai status saat ini
        if (layar3D != null) layar3D.SetActive(!sedangLihatFotoAsli);
        if (layarFotoAsli != null) layarFotoAsli.SetActive(sedangLihatFotoAsli);

        // Ubah otomatis teks di dalam tombol agar sesuai fungsinya
        if (tombolToggleVisual != null)
        {
            TextMeshProUGUI teksTombol = tombolToggleVisual.GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null)
            {
                teksTombol.text = sedangLihatFotoAsli ? "Lihat 3D" : "Lihat Foto Asli";
            }
        }
    }

    public void NextSlide()
    {
        if (indeksSlideSekarang < daftarSlide.Count - 1)
        {
            indeksSlideSekarang++;
            TampilkanSlideSekarang();
        }
    }

    public void PrevSlide()
    {
        if (indeksSlideSekarang > 0)
        {
            indeksSlideSekarang--;
            TampilkanSlideSekarang();
        }
    }

    private void TampilkanSlideSekarang()
    {
        MatikanSemuaLampu(); 

        if (daftarSlide == null || daftarSlide.Count == 0) return;

        SlideInfo slideSekarang = daftarSlide[indeksSlideSekarang];

        if (textLegenda != null) textLegenda.text = slideSekarang.teksLegenda;
        if (slideSekarang.uiTitik != null) slideSekarang.uiTitik.SetActive(true);

        if (slideSekarang.mesinHighlight != null)
        {
            foreach (var highlight in slideSekarang.mesinHighlight)
            {
                if (highlight != null) highlight.MulaiKedip();
            }
        }

        if (tombolPrev != null) tombolPrev.interactable = (indeksSlideSekarang > 0);
        if (tombolNext != null) tombolNext.interactable = (indeksSlideSekarang < daftarSlide.Count - 1);
    }

    private void MatikanSemuaLampu()
    {
        if (daftarSlide == null) return;

        foreach (var slide in daftarSlide)
        {
            if (slide.uiTitik != null) slide.uiTitik.SetActive(false);

            if (slide.mesinHighlight != null)
            {
                foreach (var highlight in slide.mesinHighlight)
                {
                    if (highlight != null) highlight.BerhentiKedip();
                }
            }
        }
    }
}