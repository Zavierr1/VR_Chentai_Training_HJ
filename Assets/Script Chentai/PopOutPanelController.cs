using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages a pop-out control panel that shows machine details via a slideshow.
// Supports toggling between a live 3D render (RenderTexture) and a real photo.
public class PopOutPanelController : MonoBehaviour
{
    [Header("Komponen Utama")]
    [Tooltip("Masukkan Canvas utama Pop-Out kamu ke sini")]
    public GameObject panelUIUtama; 
    
    [Tooltip("Kamera HD khusus yang merender gambar ke RawImage panel ini")]
    public Camera popOutCamera; 

    // Screen toggle system (3D vs real photo).
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
    
    // Toggle button for switching between 3D and real photo modes.
    [Tooltip("Tombol untuk mengganti mode 3D / Foto Asli")]
    public Button tombolToggleVisual; 

    [Header("Data Slideshow")]
    public List<SlideInfo> daftarSlide; 

    private int indeksSlideSekarang = 0;
    private bool sedangLihatFotoAsli = false; // Current toggle state.

    // Hides the panel at start.
    void Start()
    {
        HidePanel();
    }

    // Shows the panel and always starts in 3D mode.
    public void ShowPanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(true);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(true);

        // Always reset to 3D mode when the panel opens.
        sedangLihatFotoAsli = false;
        UpdateTampilanLayar();

        indeksSlideSekarang = 0;
        TampilkanSlideSekarang();
    }

    // Hides the panel and turns off the pop-out camera.
    public void HidePanel()
    {
        if (panelUIUtama != null) panelUIUtama.SetActive(false);
        if (popOutCamera != null) popOutCamera.gameObject.SetActive(false);
        
        MatikanSemuaLampu();
    }

    // Toggles between the 3D render and the real photo.
    public void ToggleTampilanVisual()
    {
        sedangLihatFotoAsli = !sedangLihatFotoAsli; // Flip the state.
        UpdateTampilanLayar();
    }

    // Enables/disables the two screens and updates the toggle button label.
    private void UpdateTampilanLayar()
    {
        // Turn the screens on/off according to the current state.
        if (layar3D != null) layar3D.SetActive(!sedangLihatFotoAsli);
        if (layarFotoAsli != null) layarFotoAsli.SetActive(sedangLihatFotoAsli);

        // Update the toggle button text automatically.
        if (tombolToggleVisual != null)
        {
            TextMeshProUGUI teksTombol = tombolToggleVisual.GetComponentInChildren<TextMeshProUGUI>();
            if (teksTombol != null)
            {
                teksTombol.text = sedangLihatFotoAsli ? "Lihat 3D" : "Lihat Foto Asli";
            }
        }
    }

    // Advances to the next slide.
    public void NextSlide()
    {
        if (indeksSlideSekarang < daftarSlide.Count - 1)
        {
            indeksSlideSekarang++;
            TampilkanSlideSekarang();
        }
    }

    // Goes back to the previous slide.
    public void PrevSlide()
    {
        if (indeksSlideSekarang > 0)
        {
            indeksSlideSekarang--;
            TampilkanSlideSekarang();
        }
    }

    // Displays the current slide's legend, highlights, and navigation state.
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

    // Turns off all slide highlights.
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
