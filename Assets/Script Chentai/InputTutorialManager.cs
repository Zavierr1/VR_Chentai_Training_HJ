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
    
    [Tooltip("Tarik objek yang punya script MonitorCameraController ke sini")]
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
    
    [Tooltip("Masukkan sumber suara Voice Over pembukaan ke sini")]
    public AudioSource suaraVOSelamatDatang;

    [Header("Debug / Testing")]
    public UnityEngine.UI.Button tombolNextDebug;

    private int tahapTutorial = 0; 
    private bool kiriOk = false;
    private bool kananOk = false;

    void Start()
    {
        // 1. Di awal game, pastikan panel disembunyikan dulu (Delay)
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(true);
        if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(false); 

        // KUNCI MESIN UTAMA DI AWAL GAME
        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(true);

        tahapTutorial = 0; // Tahap 0 sekarang adalah status "Menunggu Intro Selesai"
        
        // Jalankan alur otomatis
        StartCoroutine(SequencePembukaanOtomatis());
    }

    void Update()
    {
        switch (tahapTutorial)
        {
            case 1: 
                if (InputBridge.Instance.LeftTrigger > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightTrigger > 0.5f) kananOk = true;
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(2);
                break;

            case 2: 
                if (InputBridge.Instance.LeftThumbstickAxis.magnitude > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightThumbstickAxis.magnitude > 0.5f) kananOk = true;
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(3);
                break;

            case 3: 
                if (InputBridge.Instance.LeftGrip > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightGrip > 0.5f) kananOk = true;
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(4);
                break;
        }
    }

    // >>> COROUTINE BARU: Mengatur Delay, Panel, dan Voice Over otomatis <<<
    private IEnumerator SequencePembukaanOtomatis()
    {
        // 1. Jeda di awal game (1.5 detik) agar player tidak kaget
        yield return new WaitForSeconds(2f);

        // 2. Munculkan Panel
        if (welcomePanel != null) welcomePanel.SetActive(true);
        titleText.text = "SELAMAT DATANG DI VR";
        descText.text = "Selamat datang di Modul Pelatihan VR Mesin Stripping. Mari kenali posisi tombol pada kontrolermu sebelum mulai merakit.";
        
        if (gambarController != null && gambarFullController != null) 
        {
            gambarController.gameObject.SetActive(true);
            gambarController.sprite = gambarFullController;
        }

        yield return new WaitForSeconds(2f);

        // 3. Mainkan Voice Over
        float durasiVO = 4f; // Waktu cadangan jika audio tidak di-assign
        if (suaraVOSelamatDatang != null && suaraVOSelamatDatang.clip != null)
        {
            suaraVOSelamatDatang.Play();
            durasiVO = suaraVOSelamatDatang.clip.length; // Otomatis mendeteksi panjang suara
        }

        // 4. Tunggu VO selesai + Jeda ekstra (misal 1.5 detik) agar player mencerna teks
        yield return new WaitForSeconds(durasiVO + 5f);

        // 5. Langsung transisi masuk ke tahap instruksi gambar kalibrasi Trigger
        MulaiTahap(1);
    }

    private void MulaiTahap(int tahap)
    {
        tahapTutorial = tahap;
        kiriOk = false; 
        kananOk = false; 

        switch (tahap)
        {
            case 1:
                titleText.text = "CARA MENYENTUH LAYAR (INTERAKSI)";
                descText.text = "Gunakan jari telunjukmu untuk menekan tombol pelatuk <color=#00FFFF>Trigger</color>.\nIni berfungsi seperti sentuhan 'Tap' pada layar HP.";
                if (gambarController != null && gambarTrigger != null) 
                    gambarController.sprite = gambarTrigger;
                break;

            case 2:
                titleText.text = "CARA BERGERAK & MENGARAHKAN PANDANGAN";
                descText.text = "Gunakan jempolmu untuk menggeser stik <color=#00FFFF>Analog Kiri / Kanan</color>.\nIni sama seperti 'Virtual Joystick' pada game mobile.";
                if (gambarController != null && gambarAnalog != null) 
                    gambarController.sprite = gambarAnalog;
                break;

            case 3:
                titleText.text = "CARA MENGAMBIL BARANG";
                descText.text = "Gunakan jari tengahmu untuk menahan tombol genggam <color=#00FFFF>Grip</color>\ndi bagian samping gagang kontroler untuk meraih objek.";
                if (gambarController != null && gambarGrip != null) 
                    gambarController.sprite = gambarGrip;
                break;

            case 4:
                titleText.text = "TUTORIAL SELESAI!";
                descText.text = "<color=green>Kerja Bagus!</color>\nArahkan tanganmu dan tekan tombol 'Selesai'\ndi bawah menggunakan Trigger untuk memulai. Perhatikan layar di sebelah kanan!.";
                
                if (gambarController != null) gambarController.gameObject.SetActive(false); 
                if (suaraTutorialSelesai != null) suaraTutorialSelesai.Play();
                if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(false);

                if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(true);
                break;
        }
    }

    public void LewatiTahapIniDebug()
    {
        if (tahapTutorial == 0) 
        {
            StopAllCoroutines(); // Matikan sequence otomatis kalau di-skip paksa
            MulaiTahap(1);
        }
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
        if (welcomePanel != null) welcomePanel.SetActive(false);
        
        // BUKA KUNCI MESIN UTAMA SETELAH TUTORIAL SELESAI
        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(false);
    }
}