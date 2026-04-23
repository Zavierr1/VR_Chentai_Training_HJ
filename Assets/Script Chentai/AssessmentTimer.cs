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

    // >>> TAMBAHAN BARU: Variabel untuk Teks Instruksi Awal (dari perbaikan kita sebelumnya)
    [Tooltip("Tarik objek teks instruksi (New Text) atau Panel biru utamanya ke sini")]
    public GameObject teksInstruksiAwal;

    [Header("Pengaturan Waktu")]
    [Tooltip("Waktu maksimal Assessment dalam detik. (300 detik = 5 Menit)")]
    public float waktuAssessment = 300f; 

    [Header("Pengaturan Benda (Part)")]
    [Tooltip("Masukkan semua part/barang (Grabbable) yang ada di meja ke dalam list ini agar dikunci sebelum mulai")]
    public Grabbable[] bendaAssessment;

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

    // >>> TAMBAHAN BARU: Variabel Audio Victory & Failed <<<
    [Header("Audio Feedback")]
    [Tooltip("Suara yang diputar saat pemain LULUS (Tepat Waktu)")]
    public AudioSource suaraVictory;
    [Tooltip("Suara yang diputar saat pemain GAGAL (Waktu Habis)")]
    public AudioSource suaraFailed;
    
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

        if (panelHasilAkhir != null) panelHasilAkhir.SetActive(false);

        if (tombolMulai != null) tombolMulai.onClick.AddListener(MulaiAssessment);
        if (tombolFinish != null) tombolFinish.onClick.AddListener(KembaliKeMainMenu);
        if (tombolRestart != null) tombolRestart.onClick.AddListener(RestartAssessment);
    }

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

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = $"<color=yellow>LULUS!</color>\n\n" +
                                   $"Anda berhasil merakit mesin dan mengkalibrasinya dengan sempurna.\n" +
                                   $"Selesai dalam Waktu: <color=green>{waktuFormat}</color>";
        }

        // >>> MAIN-KAN SUARA VICTORY <<<
        if (suaraVictory != null) suaraVictory.Play();

        MunculkanPanel(true);
    }

    // >>> LOGIKA KETIKA KALAH (WAKTU HABIS) <<<
    private void WaktuHabis()
    {
        Debug.Log("<color=red>[ASSESSMENT] WAKTU HABIS! GAGAL.</color>");
        
        sudahSelesai = true;
        KunciSemuaBarang(true); 

        if (teksHasilAkhir != null)
        {
            teksHasilAkhir.text = "<color=red>WAKTU HABIS!</color>\n\n" +
                                  "Sayang sekali, semua part mesin belum terpasang dan dikalibrasi dengan sempurna.\n" +
                                  "Klik tombol restart untuk mencoba kembali.";
        }

        // >>> MAIN-KAN SUARA FAILED <<<
        if (suaraFailed != null) suaraFailed.Play();

        MunculkanPanel(false); 
    }

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
    
    public void KembaliKeMainMenu() 
    {
        if (!string.IsNullOrEmpty(namaSceneMainMenu))
        {
            SceneManager.LoadScene(namaSceneMainMenu);
        }
        else Debug.LogError("Nama Scene Main Menu belum diisi di Inspector!");
    }

    private void RestartAssessment()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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