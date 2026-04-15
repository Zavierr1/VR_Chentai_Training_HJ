using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class CalibrationManager : MonoBehaviour
{
    [Header("Referensi UI Kalibrasi")]
    [Tooltip("Tarik Panel/Canvas Kalibrasi ke sini")]
    public GameObject panelKalibrasi;
    [Tooltip("Teks untuk menampilkan angka suhu (misal: 85°C)")]
    public TextMeshProUGUI teksAngkaSuhu;
    [Tooltip("Teks untuk memberi tahu status (UNDER, SAFE, OVER)")]
    public TextMeshProUGUI teksStatusSuhu;
    
    public Button tombolPlusSuhu;
    public Button tombolMinusSuhu;

    [Header("Referensi Mekanik Lain")]
    [Tooltip("Tarik objek KNOB yang ada script KnobCalibration-nya ke sini")]
    public KnobCalibration knobPillowBlock;
    [Tooltip("Tarik mesin utama (MachineController) untuk mematikan mesin saat kalibrasi")]
    public MachineController mesinUtama;

    [Header("Event Sukses")]
    [Tooltip("Apa yang terjadi saat kalibrasi beres? (Contoh: Panggil TampilkanPanelHasilSukses dari AssessmentTimer)")]
    public UnityEvent onKalibrasiBerhasilSelesai;

    private int suhuSaatIni;
    private bool isFaseKalibrasiAktif = false;
    private bool sudahSelesai = false;

    void Start()
    {
        // Pastikan panel mati di awal
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);

        // Sambungkan tombol otomatis
        if (tombolPlusSuhu != null) tombolPlusSuhu.onClick.AddListener(TambahSuhu);
        if (tombolMinusSuhu != null) tombolMinusSuhu.onClick.AddListener(KurangiSuhu);
    }

    // >>> FUNGSI INI YANG NANTI DIPANGGIL OLEH EVENT NPC <<<
    public void MulaiSequenceKalibrasi()
    {
        if (sudahSelesai) return;
        StartCoroutine(SequenceJedaDanMunculPanel());
    }

    private IEnumerator SequenceJedaDanMunculPanel()
    {
        Debug.Log("[KALIBRASI] Menunggu 5 detik sebelum mesin dimatikan...");
        
        // 1. Tunggu 5 detik seperti idemu
        yield return new WaitForSeconds(5f);

        // 2. Matikan mesin seolah-olah NPC yang matiin karena ada yang kurang pas
        if (mesinUtama != null) mesinUtama.StopMachine();

        // 3. Acak suhu awal (Bisa kurang 70-99, atau lebih 121-150)
        int acakKondisi = Random.Range(0, 2); // 0 atau 1
        if (acakKondisi == 0) suhuSaatIni = Random.Range(70, 100);
        else suhuSaatIni = Random.Range(121, 151);

        // 4. Acak target rotasi Knob Pillow Block (Misal antara 45 sampai 315 derajat)
        float targetKnobRandom = Random.Range(45f, 315f);
        if (knobPillowBlock != null) knobPillowBlock.MulaiFaseKalibrasi(targetKnobRandom);

        // 5. Munculkan UI dan mulai pantau
        UpdateTampilanSuhu();
        if (panelKalibrasi != null) panelKalibrasi.SetActive(true);
        isFaseKalibrasiAktif = true;

        Debug.Log("[KALIBRASI] Panel Muncul! Suhu awal: " + suhuSaatIni);
    }

    // >>> KONTROL SUHU DARI TOMBOL UI <<<
    public void TambahSuhu()
    {
        if (!isFaseKalibrasiAktif) return;
        suhuSaatIni += 1; // Naik 1 derajat
        UpdateTampilanSuhu();
    }

    public void KurangiSuhu()
    {
        if (!isFaseKalibrasiAktif) return;
        suhuSaatIni -= 1; // Turun 1 derajat
        UpdateTampilanSuhu();
    }

    private void UpdateTampilanSuhu()
    {
        if (teksAngkaSuhu != null) teksAngkaSuhu.text = $"{suhuSaatIni}°C";

        if (teksStatusSuhu != null)
        {
            if (suhuSaatIni < 100) 
            {
                teksStatusSuhu.text = "<color=blue>SUHU TERLALU RENDAH (UNDER)</color>";
            }
            else if (suhuSaatIni > 120) 
            {
                teksStatusSuhu.text = "<color=red>SUHU TERLALU PANAS (OVER)</color>";
            }
            else 
            {
                teksStatusSuhu.text = "<color=green>SUHU OPTIMAL (SAFE)</color>";
            }
        }
    }

    void Update()
    {
        // Terus pantau apakah kedua syarat sudah terpenuhi
        if (isFaseKalibrasiAktif && !sudahSelesai)
        {
            bool suhuAman = (suhuSaatIni >= 100 && suhuSaatIni <= 120);
            bool knobAman = (knobPillowBlock != null && knobPillowBlock.isKalibrasiSukses);

            // Jika suhu pas DAN knob bunyi klik (pas)
            if (suhuAman && knobAman)
            {
                StartCoroutine(ProsesKalibrasiSukses());
            }
        }
    }

    private IEnumerator ProsesKalibrasiSukses()
    {
        sudahSelesai = true;
        isFaseKalibrasiAktif = false;
        
        if (knobPillowBlock != null) knobPillowBlock.SelesaiKalibrasi(); // Kunci knob biar ga diputer lagi
        
        if (teksStatusSuhu != null) teksStatusSuhu.text = "<color=yellow>KALIBRASI SELESAI!</color>";

        Debug.Log("<color=green>[KALIBRASI] SUKSES! Suhu dan Pillow Block sejajar.</color>");

        // Jeda 2 detik biar player sadar kalau dia berhasil, baru panelnya hilang
        yield return new WaitForSeconds(2f);

        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);

        // Nyalakan kembali mesin secara otomatis
        if (mesinUtama != null) mesinUtama.StartMachine();

        // Panggil event (Misal: memunculkan panel Selamat / Finish Ujian)
        onKalibrasiBerhasilSelesai?.Invoke();
    }
}