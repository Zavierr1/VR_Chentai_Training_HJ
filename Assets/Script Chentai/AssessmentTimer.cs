using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BNG; // Tambahkan ini untuk mengakses script Grabbable dari BNG

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

    [Header("Panel Hasil Assessment")]
    [Tooltip("Masukkan Script Assessment Score Manager untuk mengambil skor akhir (opsional)")]
    public AssessmentScoreManager scoreManager;
    [Tooltip("Tarik Panel/Canvas UI Kemenangan ke sini")]
    public GameObject panelHasilSukses;
    [Tooltip("Tarik TextMeshPro untuk menampilkan Pesan, Waktu, & Skor")]
    public TextMeshProUGUI teksHasilSukses;

    // --- Status Publik agar bisa dicek oleh script lain ---
    [HideInInspector]
    public bool isAssessmentJalan = false;
    
    private float sisaWaktu;
    private bool sudahSelesai = false;

    void Start()
    {
        // 1. Kunci semua part agar tidak bisa diambil player sebelum menekan tombol mulai
        KunciSemuaBarang(true);

        // 2. Set waktu ke nilai awal (misal 300)
        sisaWaktu = waktuAssessment;
        
        // 3. Tampilkan teks 05:00 di awal sebelum mulai
        UpdateTeksTimer(sisaWaktu);

        // 4. Pastikan panel hasil disembunyikan di awal
        if (panelHasilSukses != null) 
            panelHasilSukses.SetActive(false);

        // 5. Sambungkan event tombol jika tombol Mulai di-assign
        if (tombolMulai != null)
        {
            tombolMulai.onClick.AddListener(MulaiAssessment);
        }
    }

    // Fungsi ini dipanggil otomatis ketika tombol ditekan
    public void MulaiAssessment()
    {
        if (sudahSelesai) return;

        isAssessmentJalan = true;

        // Buka kunci barang agar player sudah mulai bisa merakit!
        KunciSemuaBarang(false);
        
        // Sembunyikan tombol Mulai dari layar
        if (tombolMulai != null)
        {
            tombolMulai.gameObject.SetActive(false);
        }

        Debug.Log("<color=green>[ASSESSMENT] Waktu Dimulai!</color>");
    }

    // >>> DIPANGGIL OLEH MachineController SAAT SEMUA BARANG TERPASANG (SEBELUM NPC JALAN) <<<
    public void BerhentiTimerKarenaBerhasil()
    {
        if (!isAssessmentJalan) return;
        
        isAssessmentJalan = false; // Matikan segera agar detiknya tidak berkurang selagi NPC animasi
        sudahSelesai = true; 
        
        KunciSemuaBarang(true); // Kunci ulang semua benda jika masih ada yang melayang/sisa

        Debug.Log("<color=green>[ASSESSMENT] Mesin menyala! Timer dihentikan. Menunggu animasi NPC selesai.</color>");
    }

    // >>> DIPANGGIL DARI EVENT NPCFactoryShow (onNPCKembaliKePosisi) SAAT NPC SUDAH BALIK KE POSISI <<<
    public void TampilkanPanelHasilSukses()
    {
        // Hitung waktu yang ditorehkan (Waktu Max - Sisa Waktu)
        float waktuYangDipakai = waktuAssessment - sisaWaktu;
        int menit = Mathf.FloorToInt(waktuYangDipakai / 60);
        int detik = Mathf.FloorToInt(waktuYangDipakai % 60);
        string waktuFormat = string.Format("{0:00}:{1:00}", menit, detik);

        // Ambil Skor dari indikator lain
        int skorAkhir = (scoreManager != null) ? scoreManager.currentScore : 100; // Asumsi 100 kalau tidak pakai manager

        // Tampilkan Teks!
        if (teksHasilSukses != null)
        {
            teksHasilSukses.text = $"<color=yellow>SELAMAT!</color>\n\n" +
                                   $"Anda berhasil merakit mesin dengan sempurna.\n" +
                                   $"Waktu yang Terpakai: <color=green>{waktuFormat}</color>\n" +
                                   $"Skor Akhir Anda: <color=yellow>{skorAkhir}</color>";
        }

        if (panelHasilSukses != null)
        {
            panelHasilSukses.SetActive(true);
        }
        
        // Sembunyikan Teks Timer yang lama jika perlu
        if (teksTimer != null) teksTimer.gameObject.SetActive(false);
    }

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
        // Jika assessment sedang berjalan, kurangi waktu terus menerus
        if (isAssessmentJalan)
        {
            if (sisaWaktu > 0)
            {
                sisaWaktu -= Time.deltaTime;
                UpdateTeksTimer(sisaWaktu);
            }
            else // Bila waktu tepat menyentuh angka 0
            {
                sisaWaktu = 0;
                isAssessmentJalan = false; // Matikan timer
                UpdateTeksTimer(sisaWaktu);
                WaktuHabis();
            }
        }
    }

    // Fungsi untuk mengubah angka detik/float menjadi format Jam:Menit (MM:SS)
    private void UpdateTeksTimer(float waktu)
    {
        if (teksTimer != null)
        {
            int menit = Mathf.FloorToInt(waktu / 60);
            int detik = Mathf.FloorToInt(waktu % 60);
            
            // Format angka agar selalu dua digit, contohnya "05:09", bukan "5:9"
            teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
            
            // Opsional: Bikin teks jadi MERAH kalau waktu sisa di bawah 1 menit (60 detik)
            if (waktu < 60f)
            {
                teksTimer.color = Color.red;
            }
            else
            {
                teksTimer.color = Color.white; // Asumsi warna dasar timer adalah putih
            }
        }
    }

    // Fungsi ini terpanggil murni saat timer menyentuh 00:00
    private void WaktuHabis()
    {
        Debug.Log("<color=red>[ASSESSMENT] WAKTU HABIS! GAGAL.</color>");
        
        // TODO: Anda bisa panggil event memunculkan Panel "GAGAL" di sini nantinya
        // Contoh: panelGagal.SetActive(true);
    }
}