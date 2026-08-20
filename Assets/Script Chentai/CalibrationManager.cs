using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using BNG; 

// Orchestrates the machine calibration sequence: scrambles the temperature and
// sealing-roll density values, then accepts player input through UI buttons and
// a physical knob until both are in the valid range.
public class CalibrationManager : MonoBehaviour
{
    [Header("Mode Configuration")]
    [Tooltip("Centang jika ini adalah Scene Assessment agar hint kelap-kelip TIDAK muncul")]
    public bool isAssessmentMode = false;

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
    
    // --- DATA VARIABLES ---
    private int suhuSaatIni;
    private int kerapatanSealingRoll; 
    private bool isFaseKalibrasiAktif = false;
    private bool sudahSelesai = false;
    private bool flagKnobSukses = false;

    // Hides the calibration panel and wires up the temperature buttons.
    void Start()
    {
        if (panelKalibrasi != null) panelKalibrasi.SetActive(false);
        if (tombolPlusSuhu != null) tombolPlusSuhu.onClick.AddListener(TambahSuhu);
        if (tombolMinusSuhu != null) tombolMinusSuhu.onClick.AddListener(KurangiSuhu);
    }

    // Starts the tutorial calibration sequence that waits for the machine to be on,
    // then forces the setup phase.
    public void MulaiTutorialCalibrationSequence()
    {
        if (sudahSelesai) return;
        StartCoroutine(SequenceTutorialKhusus());
    }

    // Waits for the machine to turn on, pauses, then stops it and begins the
    // temperature/knob setup phase.
    private IEnumerator SequenceTutorialKhusus()
    {
        yield return new WaitUntil(() => mesinUtama.isMachineOn);
        yield return new WaitForSeconds(4f); 
        if (mesinUtama != null) mesinUtama.StopMachine();

        MulaiSetupSuhuDanKnob();
    }

    // Scrambles the temperature and sealing-roll density values, resets the state
    // flags, updates the UI, and enables the physical knob for calibration.
    private void MulaiSetupSuhuDanKnob()
    {
        // 1. Scramble the temperature (use the UI buttons to fix it to 100-120).
        suhuSaatIni = Random.Range(0, 2) == 0 ? Random.Range(70, 95) : Random.Range(125, 150);

        // 2. Scramble the sealing-roll density (use the physical knob to set exactly 100).
        kerapatanSealingRoll = Random.Range(0, 2) == 0 ? Random.Range(50, 99) : Random.Range(101, 150);

        if (knobPillowBlock != null) 
        {
            knobPillowBlock.SetupKnobUntukKalibrasi(this);
            
            // Enable the blinking hint unless this is an assessment scene.
            if (!isAssessmentMode)
            {
                knobPillowBlock.SetStatusHint(true);
            }
        }

        isFaseKalibrasiAktif = true;
        sudahSelesai = false;
        flagKnobSukses = false;
        
        UpdateTampilanSuhu();
        UpdateTampilanKnob();
        if (panelKalibrasi != null) panelKalibrasi.SetActive(true);
    }

   
    // PART 1: TEMPERATURE CONTROL (VIA UI BUTTONS)
    public void TambahSuhu() { if (!isFaseKalibrasiAktif) return; suhuSaatIni += 1; BeriFeedbackKlikUI(); UpdateTampilanSuhu(); CekKondisiSuksesSemua(); }
    public void KurangiSuhu() { if (!isFaseKalibrasiAktif) return; suhuSaatIni -= 1; BeriFeedbackKlikUI(); UpdateTampilanSuhu(); CekKondisiSuksesSemua(); }

    // Plays a click sound and vibrates the controller on every temperature button press.
    private void BeriFeedbackKlikUI()
    {
        if (suaraTombolSuhu != null) suaraTombolSuhu.Play();
        ControllerHand tangan = InputBridge.Instance.LeftTrigger > 0.5f ? ControllerHand.Left : ControllerHand.Right;
        InputBridge.Instance.VibrateController(0.1f, 0.2f, 0.05f, tangan);
    }

    // Updates the temperature text, status label, and thermometer bar color/fill.
    private void UpdateTampilanSuhu()
    {
        if (teksAngkaSuhu != null) teksAngkaSuhu.text = $"{suhuSaatIni}°C";
        Color warnaUI;
        if (suhuSaatIni < 100) { teksStatusSuhu.text = "UNDERHEATING"; warnaUI = new Color(0f, 0.63f, 1f); }
        else if (suhuSaatIni > 120) { teksStatusSuhu.text = "OVERHEATING!"; warnaUI = Color.red; }
        else { teksStatusSuhu.text = "OPTIMAL"; warnaUI = Color.green; }

        if (barTermometer != null) { barTermometer.color = warnaUI; barTermometer.fillAmount = (float)suhuSaatIni / 150f; }
    }

    
    // PART 2: DENSITY CONTROL (VIA PHYSICAL KNOB)
    // Adjusts the sealing-roll density based on physical knob rotation, clamps the
    // value, then updates the UI and checks for the win condition.
    // nilaiPerubahan: The amount the density changed (+1 or -1 per step).
    public void UbahKerapatanDariKnob(int nilaiPerubahan)
    {
        if (!isFaseKalibrasiAktif || sudahSelesai) return;

        kerapatanSealingRoll += nilaiPerubahan;
        kerapatanSealingRoll = Mathf.Clamp(kerapatanSealingRoll, 0, 200);

        UpdateTampilanKnob();
        CekKondisiSuksesSemua();
    }

    // Updates the density percentage text, slider value, accuracy bar color, and
    // triggers knob success feedback when the value reaches exactly 100.
    private void UpdateTampilanKnob()
    {
        if (teksPersentaseKnob != null) teksPersentaseKnob.text = $"{kerapatanSealingRoll}%";

        if (sliderAkurasiKnob != null) 
        {
            sliderAkurasiKnob.value = kerapatanSealingRoll;
        }

        float jarakDariTarget = Mathf.Abs(kerapatanSealingRoll - 100);
        float progressAkurasi = 1f - Mathf.Clamp01(jarakDariTarget / 50f); 

        if (barAkurasiKnob != null) 
        {
            barAkurasiKnob.color = Color.Lerp(Color.red, Color.green, progressAkurasi);
        }

        if (kerapatanSealingRoll == 100 && !flagKnobSukses)
        {
            flagKnobSukses = true;
            if (knobPillowBlock != null) knobPillowBlock.BeriFeedbackSukses(true);
        }
        else if (kerapatanSealingRoll != 100)
        {
            flagKnobSukses = false; 
        }
    }

    
    // PART 3: WIN CONDITION CHECK
    // Checks whether both conditions are met (temperature 100-120 and density exactly
    // 100) and, if so, starts the success sequence.
    private void CekKondisiSuksesSemua()
    {
        bool suhuAman = (suhuSaatIni >= 100 && suhuSaatIni <= 120);
        bool kerapatanAman = (kerapatanSealingRoll == 100);

        if (suhuAman && kerapatanAman) 
        {
            StartCoroutine(ProsesKalibrasiSukses());
        }
    }

    // Finalizes the calibration: locks the state, hides the panel, starts the machine,
    // and invokes the success event.
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

    // Force-stops the calibration (e.g., when the assessment timer runs out):
    // disables the input phase and hides the calibration panel.
    public void BatalkanKalibrasiOtomatis()
    {
        // Disable the buttons and knob.
        isFaseKalibrasiAktif = false;
        sudahSelesai = true;
        
        // Hide the calibration UI panel.
        if (panelKalibrasi != null) 
        {
            panelKalibrasi.SetActive(false);
        }

        Debug.Log("<color=yellow>[KALIBRASI] Dihentikan paksa karena waktu habis.</color>");
    }
}
