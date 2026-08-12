using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BNG; 
using UnityEngine.SceneManagement; 

// Manages the assessment countdown timer. All parts are locked until the timer
// starts, and the assessment either completes (machine calibrated in time) or
// fails (time runs out), showing the appropriate result panel.
public class AssessmentTimer : MonoBehaviour
{
    [Header("Pengaturan UI")]
    [Tooltip("Masukkan Tombol Mulai (Start) dari Canvas ke sini")]
    public UnityEngine.UI.Button tombolMulai;
    [Tooltip("Masukkan Teks UI (TextMeshPro) yang dipakai untuk angka Timer ke sini")]
    public TextMeshProUGUI teksTimer;

    // Instruction text shown before the assessment begins.
    [Tooltip("Tarik objek teks instruksi (New Text) atau Panel biru utamanya ke sini")]
    public GameObject teksInstruksiAwal;

    [Header("Pengaturan Waktu")]
    [Tooltip("Waktu maksimal Assessment dalam detik. (300 detik = 5 Menit)")]
    public float waktuAssessment = 300f; 

    [Header("Pengaturan Benda (Part)")]
    [Tooltip("Masukkan semua part/barang (Grabbable) yang ada di meja ke dalam list ini agar dikunci sebelum mulai")]
    public Grabbable[] bendaAssessment;

    [Header("Referensi Kalibrasi")]
    [Tooltip("Tarik game object yang memiliki script CalibrationManager ke sini")]
    public CalibrationManager kalibrasiManager;

    [Header("Panel Hasil Akhir (Unified)")]
    [Tooltip("Tarik Panel/Canvas UI Hasil (Satu panel untuk Menang/Kalah) ke sini")]
    public GameObject panelHasilAkhir;
    [Tooltip("Tarik TextMeshPro untuk menampilkan Pesan Sukses / Gagal")]
    public TextMeshProUGUI teksHasilAkhir;
    
    [Header("Tombol Navigasi Hasil")]
    [Tooltip("Tombol untuk kembali ke Main Menu (Muncul saat Sukses)")]
    public UnityEngine.UI.Button tombolFinish;
    [Tooltip("Tombol untuk mengulang ujian (Muncul saat Gagal)")]
    public UnityEngine.UI.Button tombolRestart;

    [Header("Audio Feedback")]
    [Tooltip("Suara yang diputar saat pemain LULUS (Tepat Waktu)")]
    public AudioSource suaraVictory;
    [Tooltip("Suara yang diputar saat pemain GAGAL (Waktu Habis)")]
    public AudioSource suaraFailed;
    
    [Header("Pengaturan Scene")]
    [Tooltip("Ketik nama scene Main Menu kamu dengan persis (huruf besar/kecil berpengaruh)")]
    public string namaSceneMainMenu = "Nama_Scene_Main_Menu";

    // Whether the assessment countdown is currently running.
    [HideInInspector]
    public bool isAssessmentJalan = false;
    
    private float sisaWaktu;
    private bool sudahSelesai = false;

    // Locks all parts, initializes the timer, hides the result panel, and wires
    // up the start/finish/restart buttons.
    void Start()
    {
        KunciSemuaBarang(true);
        sisaWaktu = waktuAssessment;
        UpdateTeksTimer(sisaWaktu);

        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(false);

        if (tombolMulai != null) tombolMulai.onClick.AddListener(MulaiAssessment);
        if (tombolFinish != null) tombolFinish.onClick.AddListener(KembaliKeMainMenu);
        if (tombolRestart != null) tombolRestart.onClick.AddListener(RestartAssessment);
    }

    // Starts the assessment: unlocks all parts and hides the start button.
    public void MulaiAssessment()
    {
        if (sudahSelesai) return;

        isAssessmentJalan = true;
        KunciSemuaBarang(false);
        
        if (tombolMulai != null) 
        {
            tombolMulai.gameObject.SetActive(false);
        }

        Debug.Log("<color=green>[ASSESSMENT] Waktu Dimulai!</color>");
    }

    // Stops the timer because the machine was successfully assembled/started.
    public void BerhentiTimerKarenaBerhasil()
    {
        if (!isAssessmentJalan) return;
        
        isAssessmentJalan = false; 
        sudahSelesai = true; 
        
        KunciSemuaBarang(true); 

        Debug.Log("<color=green>[ASSESSMENT] Mesin menyala! Timer dihentikan.</color>");
    }

    // Shows the success result panel with the elapsed time and plays victory audio.
    public void TampilkanPanelHasilSukses()
    {
        float waktuYangDipakai = waktuAssessment - sisaWaktu;
        int menit = Mathf.FloorToInt(waktuYangDipakai / 60);
        int detik = Mathf.FloorToInt(waktuYangDipakai % 60);
        string waktuFormat = string.Format("{0:00}:{1:00}", menit, detik);

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = $"<color=yellow>LULUS!</color>\n\n" +
                                   $"Anda berhasil merakit mesin dan mengkalibrasinya dengan sempurna.\n" +
                                   $"Selesai dalam Waktu: <color=green>{waktuFormat}</color>";
        }

        if (suaraVictory != null) suaraVictory.Play();

        MunculkanPanel(true);
    }

    // Handles the failure case when time runs out: locks parts, cancels any active
    // calibration, shows the fail panel, and plays failure audio.
    private void WaktuHabis()
    {
        Debug.Log("<color=red>[ASSESSMENT] WAKTU HABIS! GAGAL.</color>");
        
        sudahSelesai = true;
        KunciSemuaBarang(true); 

        // Close the calibration panel if it is currently open.
        if (kalibrasiManager != null)
        {
            kalibrasiManager.BatalkanKalibrasiOtomatis();
        }

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = "<color=red>WAKTU HABIS!</color>\n\n" +
                                  "Sayang sekali, semua part mesin belum terpasang dan dikalibrasi dengan sempurna.\n" +
                                  "Klik tombol restart untuk mencoba kembali.";
        }

        if (suaraFailed != null) suaraFailed.Play();

        MunculkanPanel(false); 
    }

    // Displays the unified result panel and toggles the Finish/Restart buttons
    // depending on whether the player passed or failed.
    // isMenang: True for the success panel, false for the failure panel.
    private void MunculkanPanel(bool isMenang)
    {
        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(true);
        if (teksTimer != null) teksTimer.gameObject.SetActive(false);

        if (teksInstruksiAwal != null) teksInstruksiAwal.SetActive(false);

        if (tombolMulai != null && tombolMulai.transform.parent != null)
        {
            tombolMulai.transform.parent.gameObject.SetActive(false);
        }

        if (tombolFinish != null) tombolFinish.gameObject.SetActive(isMenang);
        if (tombolRestart != null) tombolRestart.gameObject.SetActive(!isMenang);
    }
    
    // Loads the main menu scene specified in the inspector.
    public void KembaliKeMainMenu() 
    {
        if (!string.IsNullOrEmpty(namaSceneMainMenu))
        {
            SceneManager.LoadScene(namaSceneMainMenu);
        }
        else Debug.LogError("Nama Scene Main Menu belum diisi di Inspector!");
    }

    // Reloads the current scene to restart the assessment.
    private void RestartAssessment()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Enables or disables all assessment parts to lock or unlock them.
    // isKunci: True locks the parts (disables Grabbable), false unlocks them.
    private void KunciSemuaBarang(bool isKunci)
    {
        if (bendaAssessment != null && bendaAssessment.Length > 0)
        {
            foreach (var part in bendaAssessment)
            {
                if (part != null)
                {
                    part.enabled = !isKunci; 
                }
            }
        }
    }

    // Counts down the remaining time each frame and triggers the fail state when it reaches zero.
    void Update()
    {
        if (isAssessmentJalan)
        {
            if (sisaWaktu > 0)
            {
                sisaWaktu -= Time.deltaTime;
                UpdateTeksTimer(sisaWaktu);
            }
            else 
            {
                sisaWaktu = 0;
                isAssessmentJalan = false; 
                UpdateTeksTimer(sisaWaktu);
                WaktuHabis();
            }
        }
    }

    // Formats and displays the remaining time in MM:SS, turning red below 60 seconds.
    // waktu: The remaining time in seconds.
    private void UpdateTeksTimer(float waktu)
    {
        if (teksTimer != null)
        {
            int menit = Mathf.FloorToInt(waktu / 60);
            int detik = Mathf.FloorToInt(waktu % 60);
            teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
            
            if (waktu < 60f) teksTimer.color = Color.red;
            else teksTimer.color = Color.white; 
        }
    }
}
