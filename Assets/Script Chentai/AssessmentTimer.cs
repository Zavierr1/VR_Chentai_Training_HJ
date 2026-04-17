using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BNG; 
using UnityEngine.SceneManagement; 

public class AssessmentTimer : MonoBehaviour
{
    [Header("Pengaturan UI")]
    [Tooltip("Masukkan Tombol Mulai (Start) dari Canvas ke sini")]
    public UnityEngine.UI.Button tombolMulai;
    [Tooltip("Masukkan Teks UI (TextMeshPro) yang dipakai untuk angka Timer ke sini")]
    public TextMeshProUGUI teksTimer;

    [Header("Pengaturan Waktu")]
    [Tooltip("Waktu maksimal Assessment dalam detik. (300 detik = 5 Menit)")]
    public float waktuAssessment = 300f; 

    [Header("Pengaturan Benda (Part)")]
    [Tooltip("Masukkan semua part/barang (Grabbable) yang ada di meja ke dalam list ini agar dikunci sebelum mulai")]
    public Grabbable[] bendaAssessment;

    [Header("Panel Hasil Akhir (Unified)")]
    public AssessmentScoreManager scoreManager;
    [Tooltip("Tarik Panel/Canvas UI Hasil (Satu panel untuk Menang/Kalah) ke sini")]
    public GameObject panelHasilAkhir;
    [Tooltip("Tarik TextMeshPro untuk menampilkan Pesan Sukses / Gagal")]
    public TextMeshProUGUI teksHasilAkhir;
    
    [Header("Tombol Navigasi Hasil")]
    [Tooltip("Tombol untuk kembali ke Main Menu (Muncul saat Sukses)")]
    public UnityEngine.UI.Button tombolFinish;
    [Tooltip("Tombol untuk mengulang ujian (Muncul saat Gagal)")]
    public UnityEngine.UI.Button tombolRestart;
    
    [Header("Pengaturan Scene")]
    [Tooltip("Ketik nama scene Main Menu kamu dengan persis (huruf besar/kecil berpengaruh)")]
    public string namaSceneMainMenu = "Nama_Scene_Main_Menu";

    [HideInInspector]
    public bool isAssessmentJalan = false;
    
    private float sisaWaktu;
    private bool sudahSelesai = false;

    void Start()
    {
        KunciSemuaBarang(true);
        sisaWaktu = waktuAssessment;
        UpdateTeksTimer(sisaWaktu);

        // Pastikan panel hasil disembunyikan di awal
        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(false);

        // Sambungkan event tombol secara otomatis lewat script
        if (tombolMulai != null) tombolMulai.onClick.AddListener(MulaiAssessment);
        if (tombolFinish != null) tombolFinish.onClick.AddListener(KembaliKeMainMenu);
        if (tombolRestart != null) tombolRestart.onClick.AddListener(RestartAssessment);
    }

    public void MulaiAssessment()
    {
        if (sudahSelesai) return;

        isAssessmentJalan = true;
        KunciSemuaBarang(false);
        
        // >>> PERBAIKAN: Hanya matikan gameObject tombol Mulai-nya saja, 
        // JANGAN matikan parent-nya agar UI Timer tetap terlihat.
        if (tombolMulai != null) 
        {
            tombolMulai.gameObject.SetActive(false);
        }

        Debug.Log("<color=green>[ASSESSMENT] Waktu Dimulai!</color>");
    }

    public void BerhentiTimerKarenaBerhasil()
    {
        if (!isAssessmentJalan) return;
        
        isAssessmentJalan = false; 
        sudahSelesai = true; 
        
        KunciSemuaBarang(true); 

        Debug.Log("<color=green>[ASSESSMENT] Mesin menyala! Timer dihentikan.</color>");
    }

    // >>> LOGIKA KETIKA MENANG (WAKTU BELUM HABIS TAPI MESIN NYALA) <<<
    public void TampilkanPanelHasilSukses()
    {
        float waktuYangDipakai = waktuAssessment - sisaWaktu;
        int menit = Mathf.FloorToInt(waktuYangDipakai / 60);
        int detik = Mathf.FloorToInt(waktuYangDipakai % 60);
        string waktuFormat = string.Format("{0:00}:{1:00}", menit, detik);

        int skorAkhir = (scoreManager != null) ? scoreManager.currentScore : 100; 

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = $"<color=yellow>SELAMAT!</color>\n\n" +
                                   $"Anda berhasil merakit mesin dengan sempurna.\n" +
                                   $"Waktu yang Terpakai: <color=green>{waktuFormat}</color>\n" +
                                   $"Skor Akhir Anda: <color=yellow>{skorAkhir}</color>";
        }

        MunculkanPanel(true); // Panggil fungsi pembantu (True = Mode Menang)
    }

    // >>> LOGIKA KETIKA KALAH (WAKTU HABIS) <<<
    private void WaktuHabis()
    {
        Debug.Log("<color=red>[ASSESSMENT] WAKTU HABIS! GAGAL.</color>");
        
        sudahSelesai = true;
        KunciSemuaBarang(true); // Kunci agar pemain tidak bisa lanjut merakit

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = "<color=red>WAKTU HABIS!</color>\n\n" +
                                  "Anda gagal menyelesaikan perakitan mesin dalam batas waktu yang ditentukan.\n" +
                                  "Silakan coba lagi.";
        }

        MunculkanPanel(false); // Panggil fungsi pembantu (False = Mode Kalah)
    }

    // >>> FUNGSI PEMBANTU MENGATUR TOMBOL PANEL <<<
    private void MunculkanPanel(bool isMenang)
    {
        // 1. Nyalakan panel hasil akhir
        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(true);
        
        // 2. Matikan angka timer
        if (teksTimer != null) teksTimer.gameObject.SetActive(false);

        // >>> TAMBAHAN BARU: Matikan sisa background/panel awal agar tidak numpuk
        if (tombolMulai != null && tombolMulai.transform.parent != null)
        {
            // PENTING: Pastikan panelHasilAkhir BUKAN child dari parent tombolMulai ini di Unity, 
            // agar Win Panel tidak ikut mati.
            tombolMulai.transform.parent.gameObject.SetActive(false);
        }

        // 3. Logika nyala/mati tombol
        if (tombolFinish != null) tombolFinish.gameObject.SetActive(isMenang);
        if (tombolRestart != null) tombolRestart.gameObject.SetActive(!isMenang);
    }
    
    // >>> FUNGSI NAVIGASI TOMBOL <<<
    public void KembaliKeMainMenu() // <--- UBAH DI SINI
    {
        if (!string.IsNullOrEmpty(namaSceneMainMenu))
        {
            SceneManager.LoadScene(namaSceneMainMenu);
        }
        else Debug.LogError("Nama Scene Main Menu belum diisi di Inspector!");
    }

    private void RestartAssessment()
    {
        // Memuat ulang scene yang sedang aktif saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // >>> INI FUNGSI YANG TADI KELUPAAN! <<<
    private void KunciSemuaBarang(bool isKunci)
    {
        if (bendaAssessment != null && bendaAssessment.Length > 0)
        {
            foreach (var part in bendaAssessment)
            {
                if (part != null)
                {
                    // Di BNG Framework, kita bisa disable script Grabbable-nya sementara
                    part.enabled = !isKunci; 
                }
            }
        }
    }

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