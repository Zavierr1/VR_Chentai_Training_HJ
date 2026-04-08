using UnityEngine;
using UnityEngine.UI;
using BNG; // Akses input VR
using TMPro;
using System.Collections;

public class InputTutorialManager : MonoBehaviour
{
    [Header("Referensi UI Utama")]
    public GameObject welcomePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image gambarController; // Komponen UI Image untuk nampilin gambar
    public UnityEngine.UI.Button tombolStartTutorial;

    [Header("Assets Gambar Kontroler")]
    public Sprite gambarAnalog;  // Gambar L1 R1
    public Sprite gambarTrigger; // Gambar L2 R2
    public Sprite gambarGrip;    // Gambar L3 R3

    [Header("Audio Feedback")]
    public AudioSource suaraStepSukses; // Bunyi ting/klik tiap tahap beres
    public AudioSource suaraTutorialSelesai; // Bunyi sukses besar pas selesai

    // Variabel internal
    private int tahapTutorial = 0; 
    private bool kiriOk = false;
    private bool kananOk = false;

    void Start()
    {
        if (welcomePanel != null) welcomePanel.SetActive(true);
        if (tombolStartTutorial != null) tombolStartTutorial.interactable = false;

        // Mulai langsung dari Tahap 1
        MulaiTahap(1); 
    }

    void Update()
    {
        // Mengecek input berdasarkan tahap yang sedang aktif
        switch (tahapTutorial)
        {
            case 1: // CEK ANALOG (L1/R1)
                if (InputBridge.Instance.LeftThumbstickAxis.magnitude > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightThumbstickAxis.magnitude > 0.5f) kananOk = true;
                
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(2);
                break;

            case 2: // CEK TRIGGER (L2/R2)
                if (InputBridge.Instance.LeftTrigger > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightTrigger > 0.5f) kananOk = true;
                
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(3);
                break;

            case 3: // CEK GRIP (L3/R3)
                if (InputBridge.Instance.LeftGrip > 0.5f) kiriOk = true;
                if (InputBridge.Instance.RightGrip > 0.5f) kananOk = true;
                
                if (kiriOk && kananOk) LanjutKeTahapBerikutnya(4);
                break;
        }
    }

    private void MulaiTahap(int tahap)
    {
        tahapTutorial = tahap;
        kiriOk = false; // Reset status tangan kiri
        kananOk = false; // Reset status tangan kanan

        switch (tahap)
        {
            case 1:
                titleText.text = "PANDUAN PERGERAKAN";
                descText.text = "Gerakkan <color=#00FFFF>Analog Kiri & Kanan</color>\n(L1 / R1) untuk mengkalibrasi ruang.";
                if (gambarController != null && gambarAnalog != null) 
                    gambarController.sprite = gambarAnalog;
                break;

            case 2:
                titleText.text = "PANDUAN INTERAKSI";
                descText.text = "Tekan tombol <color=#00FFFF>Picu Telunjuk</color>\n(L2 / R2) untuk menekan tombol di Panel UI.";
                if (gambarController != null && gambarTrigger != null) 
                    gambarController.sprite = gambarTrigger;
                break;

            case 3:
                titleText.text = "PANDUAN MENGAMBIL BARANG";
                descText.text = "Genggam tombol <color=#00FFFF>Samping Jari</color>\n(L3 / R3) untuk mengambil part mesin.";
                if (gambarController != null && gambarGrip != null) 
                    gambarController.sprite = gambarGrip;
                break;

            case 4: // SELESAI
                titleText.text = "KALIBRASI SELESAI!";
                descText.text = "<color=green>Kerja Bagus!</color>\nSistem telah terbuka. Silakan tekan [Mulai Tutorial] di mesin utama.";
                
                // Sembunyikan gambar karena sudah selesai
                if (gambarController != null) gambarController.gameObject.SetActive(false); 
                if (tombolStartTutorial != null) tombolStartTutorial.interactable = true;
                if (suaraTutorialSelesai != null) suaraTutorialSelesai.Play();

                // Tutup panel otomatis setelah 3 detik
                Invoke("TutupPanel", 3f); 
                break;
        }
    }

    private void LanjutKeTahapBerikutnya(int tahapSelanjutnya)
    {
        // Jeda sejenak biar ga tembus 2 tahap sekaligus
        tahapTutorial = -1; 

        if (suaraStepSukses != null) suaraStepSukses.Play();
        
        StartCoroutine(JedaTransisi(tahapSelanjutnya));
    }

    private IEnumerator JedaTransisi(int tahapSelanjutnya)
    {
        // Jeda sangat singkat (setengah detik) memberi kesan transisi yang mulus
        yield return new WaitForSeconds(0.5f);
        MulaiTahap(tahapSelanjutnya);
    }

    private void TutupPanel()
    {
        if (welcomePanel != null) welcomePanel.SetActive(false);
    }
}