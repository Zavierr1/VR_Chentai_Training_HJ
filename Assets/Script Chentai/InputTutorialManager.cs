using UnityEngine;
using UnityEngine.UI;
using BNG;
using TMPro;
using System.Collections;

public class InputTutorialManager : MonoBehaviour
{
    [Header("Referensi UI Utama")]
    public GameObject welcomePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image gambarController; 
    public MonitorCameraController monitorMesinUtama;
    public GameObject tombolSelesaiUI; 

    [Header("Assets Gambar Kontroler")]
    public Sprite gambarFullController;
    public Sprite gambarTrigger;       
    public Sprite gambarAnalog;         
    public Sprite gambarGrip;

    [Header("Audio Feedback & VO")]
    public AudioSource suaraStepSukses;
    public AudioSource suaraTutorialSelesai;
    public AudioSource suaraVOSelamatDatang;

    [Header("Objek Praktek Langsung")]
    [Tooltip("Tombol UI untuk ditembak/diklik pake Trigger")]
    public GameObject tombolLatihanTrigger;
    
    [Tooltip("Barang latihan di meja untuk tes Grip")]
    public Grabbable barangLatihanGrip;

    [Header("Debug / Testing")]
    public UnityEngine.UI.Button tombolNextDebug;

    private int tahapTutorial = 0; 
    
    // Variabel untuk deteksi Analog (Tahap 2)
    private bool kiriOk = false;
    private bool kananOk = false;

    void Start()
    {
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(true);
        if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(false); 

        SembunyikanSemuaAlatPraktek();

        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(true);
        tahapTutorial = 0; 
        StartCoroutine(SequencePembukaanOtomatis());
    }

    private void SembunyikanSemuaAlatPraktek()
    {
        if (tombolLatihanTrigger != null) tombolLatihanTrigger.SetActive(false);
        if (barangLatihanGrip != null) barangLatihanGrip.gameObject.SetActive(false);
    }

    private IEnumerator SequencePembukaanOtomatis()
    {
        yield return new WaitForSeconds(2f);

        if (welcomePanel != null) welcomePanel.SetActive(true);
        titleText.text = "SELAMAT DATANG DI VR";
        descText.text = "Selamat datang di Modul Pelatihan VR Mesin Stripping. Mari kenali kontrolermu sebelum mulai merakit.";
        
        if (gambarController != null && gambarFullController != null) 
        {
            gambarController.gameObject.SetActive(true);
            gambarController.sprite = gambarFullController;
        }

        yield return new WaitForSeconds(2f);

        float durasiVO = 4f; 
        if (suaraVOSelamatDatang != null && suaraVOSelamatDatang.clip != null)
        {
            suaraVOSelamatDatang.Play();
            durasiVO = suaraVOSelamatDatang.clip.length; 
        }

        yield return new WaitForSeconds(durasiVO + 5f);
        MulaiTahap(1);
    }

    private void MulaiTahap(int tahap)
    {
        tahapTutorial = tahap;
        SembunyikanSemuaAlatPraktek(); // Bersihkan alat praktek sebelumnya

        switch (tahap)
        {
            case 1:
                titleText.text = "CARA TAP (INTERAKSI)";
                descText.text = "Arahkan laser dari tanganmu dan tekan tombol <color=#00FFFF>Trigger</color> pada tombol 'Next' yang muncul di layar.";
                if (gambarController != null && gambarTrigger != null) gambarController.sprite = gambarTrigger;
                
                if (tombolLatihanTrigger != null) tombolLatihanTrigger.SetActive(true);
                break;

            case 2:
                // >>> KEMBALI KE SISTEM DETEKSI INPUT SEDERHANA <<<
                kiriOk = false; kananOk = false; // Reset status deteksi
                titleText.text = "CARA BERGERAK & MENGARAHKAN PANDANGAN";
                descText.text = "Gunakan jempolmu untuk menggeser stik <color=#00FFFF>Analog Kiri / Kanan</color>.\nIni berfungsi seperti Joystick pada umumnya.";
                if (gambarController != null && gambarAnalog != null) gambarController.sprite = gambarAnalog;
                break;

            case 3:
                titleText.text = "CARA MENGAMBIL BARANG";
                descText.text = "Gunakan jari tengahmu untuk menahan tombol <color=#00FFFF>Grip</color> dan ambil barang yang ada di depanmu.";
                if (gambarController != null && gambarGrip != null) gambarController.sprite = gambarGrip;
                
                if (barangLatihanGrip != null) barangLatihanGrip.gameObject.SetActive(true);
                break;

            case 4:
                titleText.text = "TUTORIAL SELESAI!";
                descText.text = "<color=green>Kerja Bagus!</color>\nTekan tombol 'Selesai' di bawah ini untuk memulai.";
                
                if (gambarController != null) gambarController.gameObject.SetActive(false); 
                if (suaraTutorialSelesai != null) suaraTutorialSelesai.Play();
                if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(false);
                if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(true);
                break;
        }
    }

    // Dipanggil oleh UI Button "KLIK SAYA"
    public void SuksesLatihanTrigger()
    {
        if (tahapTutorial == 1) LanjutKeTahapBerikutnya(2);
    }

    void Update()
    {
        // Deteksi Analog (Cek apakah pemain sudah menggeser kedua analog)
        if (tahapTutorial == 2) 
        {
            if (InputBridge.Instance.LeftThumbstickAxis.magnitude > 0.5f) kiriOk = true;
            if (InputBridge.Instance.RightThumbstickAxis.magnitude > 0.5f) kananOk = true;
            
            if (kiriOk && kananOk) LanjutKeTahapBerikutnya(3);
        }
        // Deteksi Grip (Cek apakah barang latihan sudah digenggam)
        else if (tahapTutorial == 3) 
        {
            if (barangLatihanGrip != null && barangLatihanGrip.BeingHeld)
            {
                LanjutKeTahapBerikutnya(4);
            }
        }
    }

    public void LewatiTahapIniDebug()
    {
        if (tahapTutorial == 0) { StopAllCoroutines(); MulaiTahap(1); }
        else if (tahapTutorial == 1) LanjutKeTahapBerikutnya(2);
        else if (tahapTutorial == 2) LanjutKeTahapBerikutnya(3);
        else if (tahapTutorial == 3) LanjutKeTahapBerikutnya(4);
    }

    private void LanjutKeTahapBerikutnya(int tahapSelanjutnya)
    {
        tahapTutorial = -1; 
        if (suaraStepSukses != null) suaraStepSukses.Play();
        StartCoroutine(JedaTransisi(tahapSelanjutnya, 0.5f));
    }

    private IEnumerator JedaTransisi(int tahapSelanjutnya, float lamaJeda)
    {
        yield return new WaitForSeconds(lamaJeda);
        MulaiTahap(tahapSelanjutnya);
    }

    public void TutupWelcomePanel()
    {
        // Hentikan VO Selamat Datang (jika masih bunyi)
        if (suaraVOSelamatDatang != null && suaraVOSelamatDatang.isPlaying)
        {
            suaraVOSelamatDatang.Stop();
        }

        // >>> REVISI: Hentikan VO Selesai Kalibrasi Kontroler jika pemain langsung klik Selesai <<<
        if (suaraTutorialSelesai != null && suaraTutorialSelesai.isPlaying)
        {
            suaraTutorialSelesai.Stop();
        }

        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(false);
    }
}