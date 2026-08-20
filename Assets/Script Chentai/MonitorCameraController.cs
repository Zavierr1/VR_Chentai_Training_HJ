using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement; 

// Serializable data for one slide of the monitor guide book: legend text, UI dot,
// machine highlights (blinking), and ghost hints at the snap zones.
[System.Serializable]
public class SlideInfo
{
    [TextArea] public string teksLegenda;
    public GameObject uiTitik; 
    public List<KelapKelipTutorial> mesinHighlight; 
    
    [Tooltip("Tarik objek SnapZone yang punya script TutorialDynamicHint ke sini")]
    public List<TutorialDynamicHint> ghostHighlight; 
}

// Controls the in-game monitor/CCTV camera, the guide book slideshow, the part
// assembly flow (A, B, C), and the success/finish flow for the tutorial.
public class MonitorCameraController : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Transform cctvCamera;
    public TextMeshProUGUI textInstruksi;

    [Header("Referensi Fisik (SnapGroupManager)")]
    public SnapGroupManager managerPartA;
    public SnapGroupManager managerPartB;
    public SnapGroupManager managerPartC;
    public bool debugBypassPerakitan = false;

    [Header("Audio Feedback")]
    [Tooltip("Suara saat muncul layar SELAMAT (Victory)")]
    public AudioSource suaraVictory;

    [Header("Voice Over")]
    [Tooltip("Masukkan AudioSource untuk VO Perakitan")]
    public AudioSource voMulaiTutorial;
    public AudioSource voMulaiPemasangan;

    [Header("Referensi Tombol UI Utama")]
    public Button tombolStartTutorial; 
    public Button tombolPanduanPojokKanan; // '?' button.
    public Button tombolPartA;
    public Button tombolPartB;
    public Button tombolPartC;
    public Button tombolBack;         
    public Button tombolNext;
    public Button tombolReset; // Button to continue to the next scene after the tutorial.

    [Header("Tombol Ekstra Panduan")]
    [Tooltip("Tombol 'Klik untuk mulai pemasangan' (Muncul di ujung slide)")]
    public Button tombolMulaiPemasangan;
    [Tooltip("Tombol Back/Close saat membuka ulang panduan (Review)")]
    public Button tombolClosePanduan;

    [Header("Scene Transition")]
    [Tooltip("Tarik Canvas/Panel ucapan selamat yang muncul di akhir ke sini")]
    public GameObject panelSelesaiTutorial; 
    [Tooltip("Tarik Teks (TextMeshPro) untuk ucapan selamat ke sini")]
    public TextMeshProUGUI teksSelesaiTutorial;
    public Button tombolFinish; 
    public string namaSceneBerikutnya = "NamaSceneKamuDisini";

    [Header("Data Slideshow Buku Panduan (Kanan)")]
    public GameObject panelLegenda;
    public TextMeshProUGUI textLegenda;
    public Button tombolNextSlide; 
    public Button tombolPrevSlide; 

    public List<SlideInfo> slideAreaAtas;
    public List<SlideInfo> slideInfoPartC;
    public List<SlideInfo> slideInfoPartB;
    public List<SlideInfo> slideAreaBawah; 
    public List<SlideInfo> slideInfoPartA;

    public bool tampilkanSemuaTitik = false;

    private List<SlideInfo> slideAktif = new List<SlideInfo>();
    private int indeksSlideSekarang = 0;
    private int areaTutorialAktif = 0; 

    [Header("Target Posisi (Waypoints)")]
    public Transform targetDefault;
    public Transform targetInfoAtas;
    public Transform targetPartC;
    public Transform targetPartB;
    public Transform targetInfoBawah;
    public Transform targetPartA;
    [Tooltip("Waypoint yang memperlihatkan lokasi panel START (tempat pemain harus berjalan)")]
    public Transform targetPanelStart;

    public float transisiDurasi = 1.0f;
    private Coroutine moveCoroutine;
    private int tahapPerakitan = 1; 

    [Header("Referensi Pop-Out Control Panel")]
    public PopOutPanelController panelControlPopOut;

    [Header("Daftar Tugas (Tutorial)")]
    [Tooltip("Teks daftar tugas yang PERSISTEN di TV (tutorial mode). Kosongkan jika tidak dipakai (misal scene Assessment)")]
    public TextMeshProUGUI daftarTugasUI;
    [Tooltip("Panel container daftar tugas. Dihidupkan/dimatikan otomatis sesuai tampilkanDaftarTugas")]
    public GameObject panelDaftarTugas;
    [Tooltip("Centang = tampilkan daftar tugas. Matikan di scene Assessment")]
    public bool tampilkanDaftarTugas = true;

    private bool tutorialSelesai = false;
    [HideInInspector] public bool isSedangKalibrasiVR = false;

    // Status tiap tugas di daftar tugas (tutorial mode).
    private bool tugasPengenalanSelesai = false;
    private bool tugasPartASelesai = false;
    private bool tugasPartBSelesai = false;
    private bool tugasPartCSelesai = false;
    private bool tugasKalibrasiSelesai = false;

    // Initializes the camera position, hides UI, and clears the TV screen.
    void Start()
    {
        if (targetDefault != null && cctvCamera != null)
        {
            cctvCamera.position = targetDefault.position;
            cctvCamera.rotation = targetDefault.rotation;
        }

        if (panelSelesaiTutorial != null)
        {
            panelSelesaiTutorial.SetActive(false);
        }
        
        MatikanSemuaSlideshow();

        // Clear the TV screen by default when the game starts.
        KunciSemuaTombol();
        UpdateTeksUI("");

        // Task list is hidden during onboarding; it is shown in ArahkanKePanelStart.
        if (panelDaftarTugas != null) panelDaftarTugas.SetActive(false);
        PerbaruiDaftarTugas();

        // Auto-subscribe to calibration success so the last task gets checked off.
        CalibrationManager kalibrasi = FindObjectOfType<CalibrationManager>();
        if (kalibrasi != null) kalibrasi.onKalibrasiBerhasilSelesai.AddListener(TandaiKalibrasiSelesai);
    }

    // Locks the whole system while VR calibration is in progress.
    public void KunciSistemUtama(bool isKunci)
    {
        isSedangKalibrasiVR = isKunci;
        if (isKunci)
        {
            KunciSemuaTombol();
        }
        else KePosisiDefault(); 
    }

// After the onboarding ends: pans the CCTV camera toward the panel so the
    // player knows where to walk, then asks them to press START there.
    public void ArahkanKePanelStart()
    {
        isSedangKalibrasiVR = false;

        // Show the persistent task list now that onboarding (Selesai) is over.
        if (panelDaftarTugas != null) panelDaftarTugas.SetActive(tampilkanDaftarTugas);
        PerbaruiDaftarTugas();

        // Move the camera to the waypoint that shows the panel (fallback: default view).
        MulaiPindahKamera(targetPanelStart != null ? targetPanelStart : targetDefault);
        RefreshTampilanTombolDefault();
        MatikanSemuaSlideshow();

        if (!tutorialSelesai)
        {
            UpdateTeksUI("Tekan tombol <color=#00FFFF>[START]</color> (Trigger) untuk mulai pengenalan mesin.");
        }
    }

    // Restores the default button layout for the TV view.
    private void RefreshTampilanTombolDefault()
    {
        KunciSemuaTombol(); 
        if (isSedangKalibrasiVR) return;

        // Always show the reset button whenever the TV returns to the default view.
        if (tombolReset != null) tombolReset.gameObject.SetActive(true);

        if (!tutorialSelesai)
        {
            if (tombolStartTutorial != null) { tombolStartTutorial.gameObject.SetActive(true); tombolStartTutorial.interactable = true; }
        }
        else
        {
            if (tahapPerakitan <= 3)
            {
                if (tombolPartA != null) { tombolPartA.gameObject.SetActive(true); tombolPartA.interactable = (tahapPerakitan == 1); }
                if (tombolPartB != null) { tombolPartB.gameObject.SetActive(true); tombolPartB.interactable = (tahapPerakitan == 2); }
                if (tombolPartC != null) { tombolPartC.gameObject.SetActive(true); tombolPartC.interactable = (tahapPerakitan == 3); }
            }
            
            if (tombolPanduanPojokKanan != null) { tombolPanduanPojokKanan.gameObject.SetActive(true); tombolPanduanPojokKanan.interactable = true; }
        }
    }

    // Hides every navigation button on the monitor.
    private void KunciSemuaTombol()
    {
        if (tombolStartTutorial != null) tombolStartTutorial.gameObject.SetActive(false);
        if (tombolPartA != null) tombolPartA.gameObject.SetActive(false);
        if (tombolPartB != null) tombolPartB.gameObject.SetActive(false);
        if (tombolPartC != null) tombolPartC.gameObject.SetActive(false);
        if (tombolNext != null) tombolNext.gameObject.SetActive(false); 
        if (tombolBack != null) tombolBack.gameObject.SetActive(false); 
        if (tombolPanduanPojokKanan != null) tombolPanduanPojokKanan.gameObject.SetActive(false);
        if (tombolFinish != null) tombolFinish.gameObject.SetActive(false);
        if (tombolReset != null) tombolReset.gameObject.SetActive(false);
    }

    // Starts the machine introduction tutorial slideshow.
    public void MulaiTutorial()
    {
        KunciSemuaTombol();
        if (panelLegenda != null) panelLegenda.SetActive(true);
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);

        // Play the VO when the start button is pressed.
        if (voMulaiTutorial != null) 
        {
            voMulaiTutorial.Play();
        }
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("PENGENALAN BAGIAN MESIN:\nMari kenali bagian mesin sebelum merakit. Gunakan tombol [<] [>] untuk detail.");
        TampilkanSlideSekarang();
    }

    // Called by the '?' button to review the guide book.
    public void BukaPanduanUlang()
    {
        KunciSemuaTombol();
        if (panelLegenda != null) panelLegenda.SetActive(true);
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(true);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(true);
        
        areaTutorialAktif = 0; 
        indeksSlideSekarang = 0;
        slideAktif = slideAreaAtas;

        MulaiPindahKamera(targetInfoAtas);
        UpdateTeksUI("ULASAN PANDUAN MESIN:\nTekan tombol panah [<] [>] untuk membaca ulang informasi mesin.");
        TampilkanSlideSekarang();
    }

    // Called by the "Click to start assembly" button.
    public void MulaiPemasanganSistem()
    {
        tutorialSelesai = true; // Lock so the start button does not reappear.
        MatikanSemuaSlideshow();

        // Stop only the tutorial VO (machine intro) if it is still playing.
        if (voMulaiTutorial != null && voMulaiTutorial.isPlaying)
        {
            voMulaiTutorial.Stop();
        }

        // Play the assembly VO.
        if (voMulaiPemasangan != null)
        {
            voMulaiPemasangan.Play();
        }

        // Task 1 (Pengenalan Mesin) is done once the assembly phase begins.
        tugasPengenalanSelesai = true;
        PerbaruiDaftarTugas();

        KePosisiDefault();
    }

    // Called by the "Close/Back" button while reviewing.
    public void TutupPanduanReview()
    {
        MatikanSemuaSlideshow();
        KePosisiDefault();
    }

    // Advances the slideshow, moving between guide areas when the last slide is reached.
    public void NextSlide()
    {
        if (indeksSlideSekarang < slideAktif.Count - 1)
        {
            indeksSlideSekarang++;
            TampilkanSlideSekarang();
        }
        else
        {
            // All text updates here were removed to avoid overwriting each other.
            if (areaTutorialAktif == 0) 
            {
                areaTutorialAktif = 1; slideAktif = slideInfoPartC; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartC); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1) 
            {
                areaTutorialAktif = 2; slideAktif = slideInfoPartB; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartB); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 2) 
            {
                areaTutorialAktif = 3; slideAktif = slideAreaBawah; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetInfoBawah); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 3) 
            {
                areaTutorialAktif = 4; slideAktif = slideInfoPartA; indeksSlideSekarang = 0;
                MulaiPindahKamera(targetPartA); 
                TampilkanSlideSekarang();
            }
        }
    }

    // Goes back through the slideshow, moving between guide areas at the first slide.
    public void PrevSlide()
    {
        if (indeksSlideSekarang > 0)
        {
            indeksSlideSekarang--;
            TampilkanSlideSekarang();
        }
        else
        {
            // All text updates here were also removed.
            if (areaTutorialAktif == 4) 
            {
                areaTutorialAktif = 3; slideAktif = slideAreaBawah; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetInfoBawah); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 3) 
            {
                areaTutorialAktif = 2; slideAktif = slideInfoPartB; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetPartB); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 2) 
            {
                areaTutorialAktif = 1; slideAktif = slideInfoPartC; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetPartC); 
                TampilkanSlideSekarang();
            }
            else if (areaTutorialAktif == 1) 
            {
                areaTutorialAktif = 0; slideAktif = slideAreaAtas; indeksSlideSekarang = slideAktif.Count - 1; 
                MulaiPindahKamera(targetInfoAtas); 
                TampilkanSlideSekarang();
            }
        }
    }

    // Shows the current slide: enables its highlights and updates navigation buttons.
    private void TampilkanSlideSekarang()
    {
        MatikanSemuaLampuSlide();

        if (slideAktif.Count == 0) return;

        SlideInfo slideSekarang = slideAktif[indeksSlideSekarang];
        if (slideSekarang.uiTitik != null) slideSekarang.uiTitik.SetActive(true);
        
        if (slideSekarang.mesinHighlight != null)
        {
            foreach (var highlight in slideSekarang.mesinHighlight)
                if (highlight != null) highlight.MulaiKedip();
        }

        if (slideSekarang.ghostHighlight != null)
        {
            foreach (var ghost in slideSekarang.ghostHighlight)
                if (ghost != null) ghost.PaksaMunculInfo();
        }

        if (textLegenda != null) textLegenda.text = slideSekarang.teksLegenda;
        
        bool isLastSlideTotal = (areaTutorialAktif == 4 && indeksSlideSekarang == slideAktif.Count - 1);
        bool bisaMundur = !(areaTutorialAktif == 0 && indeksSlideSekarang == 0);
        bool bisaMaju = !isLastSlideTotal; // Cannot go next when at the far right end.
        
        if (tombolPrevSlide != null) tombolPrevSlide.interactable = bisaMundur;
        if (tombolNextSlide != null) tombolNextSlide.interactable = bisaMaju;

        // LOGIC FOR SHOWING START vs CLOSE BUTTON.
        if (tombolMulaiPemasangan != null) tombolMulaiPemasangan.gameObject.SetActive(isLastSlideTotal && !tutorialSelesai);
        
        // The Close Guide button always appears when tutorialSelesai is true (review mode).
        if (tombolClosePanduan != null) tombolClosePanduan.gameObject.SetActive(tutorialSelesai);
    }

    // Disables the slideshow UI and all its highlights.
    private void MatikanSemuaSlideshow()
    {
        if (tombolNextSlide != null) tombolNextSlide.gameObject.SetActive(false);
        if (tombolPrevSlide != null) tombolPrevSlide.gameObject.SetActive(false);
        if (tombolMulaiPemasangan != null) tombolMulaiPemasangan.gameObject.SetActive(false);
        if (tombolClosePanduan != null) tombolClosePanduan.gameObject.SetActive(false);
        if (textLegenda != null) textLegenda.text = "";
        if (panelLegenda != null) panelLegenda.SetActive(false);

        MatikanSemuaLampuSlide();
    }

    // Turns off all slide highlights across every guide area.
    private void MatikanSemuaLampuSlide()
    {
        MatikanSpesifikLampu(slideAreaAtas);
        MatikanSpesifikLampu(slideAreaBawah);
        MatikanSpesifikLampu(slideInfoPartA);
        MatikanSpesifikLampu(slideInfoPartB);
        MatikanSpesifikLampu(slideInfoPartC);
    }

    // Turns off the highlights of one slide list.
    private void MatikanSpesifikLampu(List<SlideInfo> daftarSlide)
    {
        foreach (var slide in daftarSlide)
        {
            if (slide.uiTitik != null) slide.uiTitik.SetActive(false);

            if (slide.mesinHighlight != null)
            {
                foreach (var highlight in slide.mesinHighlight)
                    if (highlight != null) highlight.BerhentiKedip();
            }

            if (slide.ghostHighlight != null)
            {
                foreach (var ghost in slide.ghostHighlight)
                    if (ghost != null) ghost.HentikanInfo();
            }
        }
    }

    // Moves the camera back to the default view and restores the button layout.
    public void KePosisiDefault() 
    { 
        MulaiPindahKamera(targetDefault); 
        RefreshTampilanTombolDefault();
        MatikanSemuaSlideshow();

        if (isSedangKalibrasiVR) return;

        if (!tutorialSelesai) UpdateTeksUI("Tekan tombol [START] (Trigger) untuk mulai pengenalan mesin.");
        else 
        {
            if (tahapPerakitan == 1) UpdateTeksUI("MODE PERAKITAN.\nTekan tombol [Part A] untuk mulai memasang Cover yang ada di atas meja sebelah kanan.");
            else if (tahapPerakitan == 2) UpdateTeksUI("PROGRES: 33%.\nTekan tombol [Part B] untuk memasang bagian mesin yang ada di atas meja tengah.");
            else if (tahapPerakitan == 3) UpdateTeksUI("PROGRES: 66% (step terakhir).\nTekan tombol [Part C] untuk memasang bagian mesin yang ada di atas meja sebelah kiri.");
            else UpdateTeksUI("PROGRES: 100% (SELESAI!).");
        }
    }
    
    // Focuses the camera on Part A and activates its snap group.
    public void KePartA() 
    { 
        // Stop the assembly VO if the player quickly jumps to Part A.
        if (voMulaiPemasangan != null && voMulaiPemasangan.isPlaying)
        {
            voMulaiPemasangan.Stop();
        }

        MulaiPindahKamera(targetPartA); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartA != null) 
        { 
            managerPartA.AktifkanGrup(); 
            managerPartA.UpdateHighlightBerurutan(); 
        }
    }

    // Focuses the camera on Part B and activates its snap group.
    public void KePartB() 
    { 
        MulaiPindahKamera(targetPartB); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartB != null) 
        { 
            managerPartB.AktifkanGrup(); 
            managerPartB.UpdateHighlightBerurutan(); 
        } 
    }

    // Focuses the camera on Part C and activates its snap group.
    public void KePartC() 
    { 
        MulaiPindahKamera(targetPartC); 
        KunciSemuaTombol(); 
        if (tombolBack != null) 
        { 
            tombolBack.gameObject.SetActive(true); 
            tombolBack.interactable = true; 
        } 
        MatikanSemuaSlideshow(); 
        if (managerPartC != null) 
        { 
            managerPartC.AktifkanGrup(); 
            managerPartC.UpdateHighlightBerurutan(); 
        } 
    }

    // Marks all assembly stages complete and tells the player to wait for the NPC.
    public void SemuaPartTelahTerpasang()
    {
        tahapPerakitan = 4; // Mark all stages as complete (100%).

        // Check off all remaining assembly tasks.
        tugasPartASelesai = true;
        tugasPartBSelesai = true;
        tugasPartCSelesai = true;
        PerbaruiDaftarTugas();
        
        // Return the camera to the main TV view.
        KePosisiDefault(); 
        
        // Tell the player to wait for the NPC to start and inspect the machine.
        UpdateTeksUI("PERAKITAN SELESAI!\nBagus sekali. Sekarang tunggu rekan kerjamu (NPC) untuk menyalakan dan mengecek mesin.");
        
        // SAFEGUARD: Force all snap groups back on so the player can look around.
        if (managerPartA != null) managerPartA.AktifkanGrup();
        if (managerPartB != null) managerPartB.AktifkanGrup();
        if (managerPartC != null) managerPartC.AktifkanGrup();
    }

    // Called by the NPC when it returns to its starting position.
    public void MunculkanControlPanelDariNPC()
    {
        // Make sure the camera is on the main TV.
        KePosisiDefault(); 
        
        // Update the instruction text.
        UpdateTeksUI("PENGECEKAN SELESAI!\nMesin beroperasi normal. Silakan periksa detail pada Control Panel yang muncul.");
        
        // SHOW THE POP-OUT PANEL AUTOMATICALLY.
        if (panelControlPopOut != null)
        {
            panelControlPopOut.ShowPanel();
        }
    }
    
    // Called by the "CLOSE" button on the red Control Panel.
    public void TutupControlPanelDanSelesai()
    {
        // Check whether we are at the final stage or debug mode is active.
        if (tahapPerakitan > 3 || debugBypassPerakitan)
        {
            KunciSemuaTombol();
            MatikanSemuaSlideshow();

            if (panelControlPopOut != null) panelControlPopOut.HidePanel();
            if (panelSelesaiTutorial != null) panelSelesaiTutorial.SetActive(true);
            
            if (teksSelesaiTutorial != null)
            {
                teksSelesaiTutorial.text = "<color=yellow>SELAMAT!</color>\nAnda telah menyelesaikan mode Tutorial.\nSilakan tekan tombol Finish untuk melanjutkan ke mode Assessment.";
            }

            if (tombolFinish != null)
            {
                tombolFinish.gameObject.SetActive(true);
                tombolFinish.interactable = true;
            }

            if (suaraVictory != null) suaraVictory.Play();
            
            UpdateTeksUI(""); 
            MulaiPindahKamera(targetDefault); 
        }
        else
        {
            KePosisiDefault();
        }
    }
    
    // Shows the "NEXT" button when a part assembly is completed.
    public void PartSelesai() 
    { 
        KunciSemuaTombol(); 
        if (tombolNext != null) 
        { 
            tombolNext.gameObject.SetActive(true); 
            tombolNext.interactable = true; 
        } 
        UpdateTeksUI("KERJA BAGUS!.\nTekan tombol [NEXT] untuk lanjut."); 

        // Check off the task of the part that just finished (based on the current stage).
        if (tahapPerakitan == 1) tugasPartASelesai = true;
        else if (tahapPerakitan == 2) tugasPartBSelesai = true;
        else if (tahapPerakitan == 3) tugasPartCSelesai = true;
        PerbaruiDaftarTugas();
    }

    // Marks the calibration task as done when the player finishes it.
    public void TandaiKalibrasiSelesai()
    {
        tugasKalibrasiSelesai = true;
        PerbaruiDaftarTugas();
    }

    // Re-renders the persistent task list on the TV (tutorial mode only).
    private void PerbaruiDaftarTugas()
    {
        if (daftarTugasUI == null) return;

        bool[] statusSelesai = { tugasPengenalanSelesai, tugasPartASelesai, tugasPartBSelesai, tugasPartCSelesai, tugasKalibrasiSelesai };
        string[] namaTugas = { "1. Pengenalan Mesin", "2. Pasang Cover (Part A)", "3. Pasang Sealing Roll (Part B)", "4. Pasang Slider (Part C)", "5. Kalibrasi Suhu & Knob" };

        // The current (in-progress) task is the first one that is not done yet.
        int indeksTugasAktif = System.Array.IndexOf(statusSelesai, false);

        StringBuilder teksDaftar = new StringBuilder();
        for (int i = 0; i < namaTugas.Length; i++)
        {
            if (i > 0) teksDaftar.Append('\n');
            if (statusSelesai[i])
            {
                teksDaftar.Append("<color=#00FF00>[X]</color> <color=#9A9A9A>").Append(namaTugas[i]).Append("</color>");
            }
            else if (i == indeksTugasAktif)
            {
                teksDaftar.Append("<color=#FFFF00><b>").Append(namaTugas[i]).Append("</b></color>");
            }
            else
            {
                teksDaftar.Append("<color=#9A9A9A>[ ] ").Append(namaTugas[i]).Append("</color>");
            }
        }
        daftarTugasUI.text = teksDaftar.ToString();
    }

    // Advances to the next assembly stage.
    public void LanjutKeTahapBerikutnya() 
    { 
        tahapPerakitan++; 
        KePosisiDefault(); 
    }

    // Sets the instruction text on the monitor.
    public void UpdateTeksUI(string pesan) 
    { 
        if (textInstruksi != null) 
            textInstruksi.text = pesan; 
    }

    // Loads the next scene after the tutorial.
    public void PindahKeSceneBerikutnya() 
    { 
        if (!string.IsNullOrEmpty(namaSceneBerikutnya)) 
            SceneManager.LoadScene(namaSceneBerikutnya); 
    }

    // Starts a smooth camera move toward the target waypoint.
    private void MulaiPindahKamera(Transform targetTujuan)
    {
        if (cctvCamera == null || targetTujuan == null) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(ProsesPindahKamera(targetTujuan));
    }

    // Lerps the camera position and rotation to the target over the transition duration.
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

    // Restarts the tutorial by reloading the current scene.
    public void ResetTutorialDariAwal()
    {
        MatikanSemuaSlideshow(); // Turn off any active UI.
        
        // Tell MainMenuManager to jump straight into the tutorial after reload.
        MainMenuManager.autoStartTutorial = true; 
        
        // Reload the scene so all physics, part positions, and script states reset cleanly.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}
