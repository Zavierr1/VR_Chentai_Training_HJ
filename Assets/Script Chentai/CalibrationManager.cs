using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using BNG; 

public class CalibrationManager : MonoBehaviour
{
    [Header("Referensi UI: SUHU MESIN")]
    public GameObject panelKalibrasi;
    public TextMeshProUGUI teksAngkaSuhu;
    public TextMeshProUGUI teksStatusSuhu;
    public Image barTermometer; 
    public UnityEngine.UI.Button tombolPlusSuhu;
    public UnityEngine.UI.Button tombolMinusSuhu;
    public AudioSource suaraTombolSuhu;

    [Header("Referensi UI: KERAPATAN SEALING ROLL (KNOB)")]
    public UnityEngine.UI.Slider sliderAkurasiKnob;
    public Image barAkurasiKnob; 
    public TextMeshProUGUI teksPersentaseKnob; 

    [Header("Referensi Mekanik Lain")]
    public KnobCalibration knobPillowBlock;
    public MachineController mesinUtama;

    [Header("Event Sukses")]
    public UnityEvent onKalibrasiBerhasilSelesai;
    
    // --- VARIABEL DATA ---
    private int suhuSaatIni;
    private int kerapatanSealingRoll; // <--- Variabel baru khusus Knob
    private bool isFaseKalibrasiAktif = false;
    private bool sudahSelesai = false;
    private bool flagKnobSukses = false;

    void Start()
    {
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);
        if (tombolPlusSuhu != null) tombolPlusSuhu.onClick.AddListener(TambahSuhu);
        if (tombolMinusSuhu != null) tombolMinusSuhu.onClick.AddListener(KurangiSuhu);
    }

    public void MulaiTutorialCalibrationSequence()
    {
        if (sudahSelesai) return;
        StartCoroutine(SequenceTutorialKhusus());
    }

    private IEnumerator SequenceTutorialKhusus()
    {
        yield return new WaitUntil(() => mesinUtama.isMachineOn);
        yield return new WaitForSeconds(4f); 
        if (mesinUtama != null) mesinUtama.StopMachine();

        MulaiSetupSuhuDanKnob();
    }

    private void MulaiSetupSuhuDanKnob()
    {
        // 1. NGACAK SUHU (Gunakan tombol UI untuk benerin ke 100-120)
        suhuSaatIni = Random.Range(0, 2) == 0 ? Random.Range(70, 95) : Random.Range(125, 150);

        // 2. NGACAK KERAPATAN SEALING ROLL (Gunakan fisik Knob untuk benerin persis ke 100)
        kerapatanSealingRoll = Random.Range(0, 2) == 0 ? Random.Range(50, 99) : Random.Range(101, 150);

        if (knobPillowBlock != null) knobPillowBlock.SetupKnobUntukKalibrasi(this);

        isFaseKalibrasiAktif = true;
        sudahSelesai = false;
        flagKnobSukses = false;
        
        UpdateTampilanSuhu();
        UpdateTampilanKnob();
        if (panelKalibrasi != null) panelKalibrasi.SetActive(true);
    }

    // ==========================================
    // BAGIAN 1: KONTROL SUHU (VIA TOMBOL UI)
    // ==========================================
    public void TambahSuhu() { if (!isFaseKalibrasiAktif) return; suhuSaatIni += 1; BeriFeedbackKlikUI(); UpdateTampilanSuhu(); CekKondisiSuksesSemua(); }
    public void KurangiSuhu() { if (!isFaseKalibrasiAktif) return; suhuSaatIni -= 1; BeriFeedbackKlikUI(); UpdateTampilanSuhu(); CekKondisiSuksesSemua(); }

    private void BeriFeedbackKlikUI()
    {
        if (suaraTombolSuhu != null) suaraTombolSuhu.Play();
        ControllerHand tangan = InputBridge.Instance.LeftTrigger > 0.5f ? ControllerHand.Left : ControllerHand.Right;
        InputBridge.Instance.VibrateController(0.1f, 0.2f, 0.05f, tangan);
    }

    private void UpdateTampilanSuhu()
    {
        if (teksAngkaSuhu != null) teksAngkaSuhu.text = $"{suhuSaatIni}°C";
        Color warnaUI;
        if (suhuSaatIni < 100) { teksStatusSuhu.text = "UNDERHEATING"; warnaUI = new Color(0f, 0.63f, 1f); }
        else if (suhuSaatIni > 120) { teksStatusSuhu.text = "OVERHEATING!"; warnaUI = Color.red; }
        else { teksStatusSuhu.text = "OPTIMAL"; warnaUI = Color.green; }

        if (barTermometer != null) { barTermometer.color = warnaUI; barTermometer.fillAmount = (float)suhuSaatIni / 150f; }
    }

    // ==========================================
    // BAGIAN 2: KONTROL KERAPATAN (VIA FISIK KNOB)
    // ==========================================
    public void UbahKerapatanDariKnob(int nilaiPerubahan)
    {
        if (!isFaseKalibrasiAktif || sudahSelesai) return;

        kerapatanSealingRoll += nilaiPerubahan;
        kerapatanSealingRoll = Mathf.Clamp(kerapatanSealingRoll, 0, 200);

        UpdateTampilanKnob();
        CekKondisiSuksesSemua();
    }

    private void UpdateTampilanKnob()
    {
        if (teksPersentaseKnob != null) teksPersentaseKnob.text = $"{kerapatanSealingRoll}%";

        // 1. GERAKAN JARUM: Kirim angka mentah langsung ke Slider (50 sampai 150)
        // Nilai 100 otomatis akan berada persis di tengah slider.
        if (sliderAkurasiKnob != null) 
        {
            sliderAkurasiKnob.value = kerapatanSealingRoll;
        }

        // 2. WARNA JARUM: Hitung jarak dari 100 HANYA untuk mengubah warna Merah -> Hijau
        float jarakDariTarget = Mathf.Abs(kerapatanSealingRoll - 100);
        float progressAkurasi = 1f - Mathf.Clamp01(jarakDariTarget / 50f); 

        if (barAkurasiKnob != null) 
        {
            // Sekarang barAkurasiKnob akan mewarnai "Jarum" nya
            barAkurasiKnob.color = Color.Lerp(Color.red, Color.green, progressAkurasi);
        }

        // 3. LOGIKA SUKSES
        if (kerapatanSealingRoll == 100 && !flagKnobSukses)
        {
            flagKnobSukses = true;
            if (knobPillowBlock != null) knobPillowBlock.BeriFeedbackSukses(true); // Bunyi TEK!
        }
        else if (kerapatanSealingRoll != 100)
        {
            flagKnobSukses = false; 
        }
    }

    // ==========================================
    // BAGIAN 3: CEK KEMENANGAN
    // ==========================================
    private void CekKondisiSuksesSemua()
    {
        bool suhuAman = (suhuSaatIni >= 100 && suhuSaatIni <= 120);
        bool kerapatanAman = (kerapatanSealingRoll == 100);

        if (suhuAman && kerapatanAman) 
        {
            StartCoroutine(ProsesKalibrasiSukses());
        }
    }

    private IEnumerator ProsesKalibrasiSukses()
    {
        sudahSelesai = true;
        isFaseKalibrasiAktif = false;
        
        if (knobPillowBlock != null) knobPillowBlock.SelesaiKalibrasi(); 
        if (teksStatusSuhu != null) teksStatusSuhu.text = "SISTEM NORMAL";
        
        yield return new WaitForSeconds(2f);
        
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);
        if (mesinUtama != null) mesinUtama.StartMachine();
        onKalibrasiBerhasilSelesai?.Invoke();
    }
}