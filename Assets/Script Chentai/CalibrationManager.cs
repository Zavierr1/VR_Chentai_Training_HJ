using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using BNG; 

public class CalibrationManager : MonoBehaviour
{
    [Header("Referensi UI Kalibrasi (Suhu)")]
    public GameObject panelKalibrasi;
    public TextMeshProUGUI teksAngkaSuhu;
    public TextMeshProUGUI teksStatusSuhu;
    public Image barTermometer; 
    public UnityEngine.UI.Button tombolPlusSuhu;
    public UnityEngine.UI.Button tombolMinusSuhu;

    [Header("Immersive Feedback (Audio & Haptic)")]
    public AudioSource suaraTombolSuhu;

    [Header("Referensi Mekanik Lain")]
    public KnobCalibration knobPillowBlock;
    public MachineController mesinUtama;

    [Header("Event Sukses")]
    public UnityEvent onKalibrasiBerhasilSelesai;
    
    [Header("Referensi UI Akurasi Knob")]
    public UnityEngine.UI.Slider sliderAkurasiKnob;
    public Image barAkurasiKnob; // Masukkan komponen Image dari "Fill" slider ke sini
    
    private int suhuSaatIni;
    private bool isFaseKalibrasiAktif = false;
    private bool sudahSelesai = false;

    void Start()
    {
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);
        if (tombolPlusSuhu != null) tombolPlusSuhu.onClick.AddListener(TambahSuhu);
        if (tombolMinusSuhu != null) tombolMinusSuhu.onClick.AddListener(KurangiSuhu);
    }

    // =========================================================
    // FUNGSI BARU KHUSUS TUTORIAL (Alur: Nyala -> Tunggu -> Mati -> Panel)
    // =========================================================
    public void MulaiTutorialCalibrationSequence()
    {
        if (sudahSelesai) return;
        StartCoroutine(SequenceTutorialKhusus());
    }

    private IEnumerator SequenceTutorialKhusus()
    {
        Debug.Log("[TUTORIAL] Menunggu mesin dinyalakan oleh NPC...");

        // 1. Tunggu sampai variabel isMachineOn di MachineController jadi TRUE
        yield return new WaitUntil(() => mesinUtama.isMachineOn);

        // 2. Biarkan mesin berjalan selama 4 detik (biar player liat mesinnya muter dulu)
        yield return new WaitForSeconds(4f);

        Debug.Log("[TUTORIAL] Skenario: Mesin dimatikan untuk kalibrasi.");

        // 3. Matikan mesin secara paksa lewat script
        if (mesinUtama != null) mesinUtama.StopMachine();

        // 4. Baru munculkan setup suhu dan knob
        MulaiSetupSuhuDanKnob();
    }

    // Fungsi pembantu untuk setup awal angka
    private void MulaiSetupSuhuDanKnob()
    {
        int acakKondisi = Random.Range(0, 2);
        if (acakKondisi == 0) suhuSaatIni = Random.Range(70, 95);
        else suhuSaatIni = Random.Range(125, 150);

        float targetKnobRandom = Random.Range(45f, 315f);
        if (knobPillowBlock != null) knobPillowBlock.MulaiFaseKalibrasi(targetKnobRandom);

        isFaseKalibrasiAktif = true;
        UpdateTampilanSuhuLengkap();
        if (panelKalibrasi != null) panelKalibrasi.SetActive(true);
    }

    public void TambahSuhu()
    {
        if (!isFaseKalibrasiAktif) return;
        suhuSaatIni += 1;
        BeriFeedbackKlik();
        UpdateTampilanSuhuLengkap();
    }

    public void KurangiSuhu()
    {
        if (!isFaseKalibrasiAktif) return;
        suhuSaatIni -= 1;
        BeriFeedbackKlik();
        UpdateTampilanSuhuLengkap();
    }

    private void BeriFeedbackKlik()
    {
        if (suaraTombolSuhu != null) suaraTombolSuhu.Play();
        ControllerHand tangan = InputBridge.Instance.LeftTrigger > 0.5f ? ControllerHand.Left : ControllerHand.Right;
        InputBridge.Instance.VibrateController(0.1f, 0.2f, 0.05f, tangan);
    }

    private void UpdateTampilanSuhuLengkap()
    {
        if (teksAngkaSuhu != null) teksAngkaSuhu.text = $"{suhuSaatIni}°C";
        Color warnaUI;
        if (suhuSaatIni < 100) { teksStatusSuhu.text = "UNDERHEATING"; warnaUI = new Color(0f, 0.63f, 1f); }
        else if (suhuSaatIni > 120) { teksStatusSuhu.text = "OVERHEATING!"; warnaUI = Color.red; }
        else { teksStatusSuhu.text = "OPTIMAL"; warnaUI = Color.green; }

        if (barTermometer != null) {
            barTermometer.color = warnaUI;
            barTermometer.fillAmount = (float)suhuSaatIni / 150f;
        }
    }

    void Update()
    {
        if (isFaseKalibrasiAktif && !sudahSelesai) {
            
            // >>> TAMBAHAN LOGIKA UPDATE UI SLIDER KNOB <<<
            if (knobPillowBlock != null && sliderAkurasiKnob != null)
            {
                float progress = knobPillowBlock.GetPersentaseAkurasi();
                sliderAkurasiKnob.value = progress;
                
                // Ubah warna bar perlahan dari Merah (jauh) ke Hijau (pas di target)
                if (barAkurasiKnob != null)
                {
                    barAkurasiKnob.color = Color.Lerp(Color.red, Color.green, progress);
                }
            }

            // Cek kondisi sukses
            bool suhuAman = (suhuSaatIni >= 100 && suhuSaatIni <= 120);
            bool knobAman = (knobPillowBlock != null && knobPillowBlock.isKalibrasiSukses);
            
            if (suhuAman && knobAman) StartCoroutine(ProsesKalibrasiSukses());
        }
    }

    private IEnumerator ProsesKalibrasiSukses()
    {
        sudahSelesai = true;
        isFaseKalibrasiAktif = false;
        
        if (knobPillowBlock != null) knobPillowBlock.SelesaiKalibrasi(); 
        if (teksStatusSuhu != null) teksStatusSuhu.text = "SISTEM NORMAL";
        
        // Jeda bentar biar player sadar mereka udah berhasil
        yield return new WaitForSeconds(2f);
        
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);
        if (mesinUtama != null) mesinUtama.StartMachine();
        onKalibrasiBerhasilSelesai?.Invoke();
    }
}