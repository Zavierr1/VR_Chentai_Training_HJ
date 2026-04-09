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

    [Header("Audio Feedback")]
    public AudioSource suaraStepSukses;
    public AudioSource suaraTutorialSelesai;

    [Header("Debug / Testing")]
    public UnityEngine.UI.Button tombolNextDebug;

    private int tahapTutorial = 0; 
    private bool kiriOk = false;
    private bool kananOk = false;

    void Start()
    {
        if (welcomePanel != null) welcomePanel.SetActive(true);
        if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(true);
        if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(false); 

        // KUNCI MESIN UTAMA DI AWAL GAME
        if (monitorMesinUtama != null) monitorMesinUtama.KunciSistemUtama(true);

        MulaiTahap(0); 
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

    private void MulaiTahap(int tahap)
    {
        tahapTutorial = tahap;
        kiriOk = false; 
        kananOk = false; 

        switch (tahap)
        {
            case 0:
                titleText.text = "KENALI KONTROLERMU";
                descText.text = "Perhatikan posisi tombol pada kontroler VR-mu.";
                if (gambarController != null && gambarFullController != null) 
                    gambarController.sprite = gambarFullController;
                
                StartCoroutine(JedaTransisi(1, 5f));
                break;

            case 1:
                titleText.text = "PANDUAN INTERAKSI";
                descText.text = "Tekan tombol <color=#00FFFF>Trigger</color>\n(L2 / R2) untuk menekan tombol UI.";
                if (gambarController != null && gambarTrigger != null) 
                    gambarController.sprite = gambarTrigger;
                break;

            case 2:
                titleText.text = "PANDUAN PERGERAKAN";
                descText.text = "Gerakkan <color=#00FFFF>Analog Kiri & Kanan</color>\n(L1 / R1) untuk bergerak dan memutar arah.";
                if (gambarController != null && gambarAnalog != null) 
                    gambarController.sprite = gambarAnalog;
                break;

            case 3:
                titleText.text = "PANDUAN MENGAMBIL BARANG";
                descText.text = "Genggam tombol <color=#00FFFF>Grip</color>\n(L3 / R3) untuk mengambil part mesin.";
                if (gambarController != null && gambarGrip != null) 
                    gambarController.sprite = gambarGrip;
                break;

            case 4:
                titleText.text = "KALIBRASI SELESAI!";
                descText.text = "<color=green>Kerja Bagus!</color>\nSistem telah terbuka. Silakan tekan tombol 'Selesai' di bawah.";
                
                if (gambarController != null) gambarController.gameObject.SetActive(false); 
                if (suaraTutorialSelesai != null) suaraTutorialSelesai.Play();
                if (tombolNextDebug != null) tombolNextDebug.gameObject.SetActive(false);

                if (tombolSelesaiUI != null) tombolSelesaiUI.SetActive(true);
                break;
        }
    }

    public void LewatiTahapIniDebug()
    {
        if (tahapTutorial == 0) LanjutKeTahapBerikutnya(1);
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