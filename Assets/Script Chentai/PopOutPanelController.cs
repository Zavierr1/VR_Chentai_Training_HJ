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

    [Header("Referensi Layar (Toggle 3D vs Asli)")]
    [Tooltip("GameObject RawImage yang menampilkan Render Texture 3D")]
    public GameObject layar3D;
    [Tooltip("GameObject Image yang menampilkan foto mesin asli")]
    public GameObject layarFotoAsli;

    [Header("Referensi UI Slideshow (Panel Penuh)")]
    [Tooltip("Container Panel yang berisi tombol Next/Prev/Close/Toggle/Legend/Layar")]
    public GameObject panelSlideshow; 
    public TextMeshProUGUI textLegenda;
    public Button tombolNext;
    public Button tombolPrev;
    public Button tombolClose; 
    [Tooltip("Tombol untuk mengganti mode 3D / Foto Asli")]
    public Button tombolToggleVisual; 

    [Header("Referensi UI Intro (Awal)")]
    [Tooltip("Teks intro: 'Periksa detail control panel mesin'")]
    public TextMeshProUGUI teksIntro;
    [Tooltip("Tombol 'Control Panel' untuk membuka panel penuh")]
    public Button tombolBukaPanel;

    [Header("Data Slideshow")]
    public List<SlideInfo> daftarSlide; 

    private int indeksSlideSekarang = 0;
    private bool sedangLihatFotoAsli = false;

    void Start()
    {
        HidePanel();
    }

    public void ShowPanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(true);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(true);

        // Start in INTRO mode: panelSlideshow INACTIVE, only Intro + Close visible
        EnterIntroMode();
    }

    public void HidePanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(false);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(false);

        // Ensure both intro and full panel UI are hidden
        SetIntroActive(false);
        SetFullPanelActive(false);
        
        MatikanSemuaLampu();
    }

    // Called by tombolBukaPanel.onClick to transition from intro to full panel
    public void BukaPanelKontrol()
    {
        EnterFullPanelMode();
    }

    private void EnterIntroMode()
    {
        // Hide slideshow panel (the original UI container)
        if (panelSlideshow != null) panelSlideshow.SetActive(false);
        
        // Close button stays ACTIVE always (visible in both intro and full)
        if (tombolClose != null) tombolClose.gameObject.SetActive(true);
        
        // Show intro UI
        if (teksIntro != null) teksIntro.gameObject.SetActive(true);
        if (tombolBukaPanel != null) tombolBukaPanel.gameObject.SetActive(true);
        
        // Reset to 3D mode
        sedangLihatFotoAsli = false;
        UpdateTampilanLayar();
    }

    private void EnterFullPanelMode()
    {
        // Hide intro UI
        if (teksIntro != null) teksIntro.gameObject.SetActive(false);
        if (tombolBukaPanel != null) tombolBukaPanel.gameObject.SetActive(false);
        
        // Show slideshow panel (the original UI container)
        if (panelSlideshow != null) panelSlideshow.SetActive(true);
        
        // Explicitly activate all slideshow UI elements
        if (textLegenda != null) textLegenda.gameObject.SetActive(true);
        if (tombolNext != null) tombolNext.gameObject.SetActive(true);
        if (tombolPrev != null) tombolPrev.gameObject.SetActive(true);
        if (tombolToggleVisual != null) tombolToggleVisual.gameObject.SetActive(true);
        
        // Close button stays active
        if (tombolClose != null) tombolClose.gameObject.SetActive(true);
        
        // Reset slide index and show first slide
        indeksSlideSekarang = 0;
        TampilkanSlideSekarang();
    }

    private void SetIntroActive(bool active)
    {
        if (teksIntro != null) teksIntro.gameObject.SetActive(active);
        if (tombolBukaPanel != null) tombolBukaPanel.gameObject.SetActive(active);
    }

    private void SetFullPanelActive(bool active)
    {
        if (panelSlideshow != null) panelSlideshow.SetActive(active);
        if (textLegenda != null) textLegenda.gameObject.SetActive(active);
        if (tombolNext != null) tombolNext.gameObject.SetActive(active);
        if (tombolPrev != null) tombolPrev.gameObject.SetActive(active);
        if (tombolClose != null) tombolClose.gameObject.SetActive(active);
        if (tombolToggleVisual != null) tombolToggleVisual.gameObject.SetActive(active);
    }

    public void ToggleTampilanVisual()
    {
        sedangLihatFotoAsli = !sedangLihatFotoAsli;
        UpdateTampilanLayar();
    }

    private void UpdateTampilanLayar()
    {
        if (layar3D != null) layar3D.SetActive(!sedangLihatFotoAsli);
        if (layarFotoAsli != null) layarFotoAsli.SetActive(sedangLihatFotoAsli);

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