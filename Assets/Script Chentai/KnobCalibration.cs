using UnityEngine;
using BNG;

// Converts physical rotation of a grabbable knob into density adjustments for the
// sealing roll during calibration. Emits tick sounds and haptic feedback while being
// turned and a success click when the correct value is reached.
[RequireComponent(typeof(Grabbable))]
public class KnobCalibration : MonoBehaviour
{
    public enum AxisPutaran { X, Y, Z }

    [Header("Pengaturan Mekanik Putaran")]
    public AxisPutaran sumbuRotasi = AxisPutaran.Z;
    
    [Tooltip("Berapa derajat putaran untuk mengubah 1 angka? (Misal: 5 derajat = angka naik/turun 1)")]
    public float derajatPerAngka = 5f;

    [Header("Feedback Audio & Haptic")]
    [Tooltip("Bunyi keras 'TEK' saat Sealing Roll sejajar di angka 100")]
    public AudioSource suaraKlikSukses;
    [Tooltip("Bunyi 'tik' kecil seperti roda gigi saat diputar (Opsional)")]
    public AudioSource suaraTikKecil; 

    [Header("Tutorial Hint")]
    [Tooltip("Masukkan script KelapKelipTutorial yang menempel di Knob ini")]
    public KelapKelipTutorial efekKelapKelip;

    private Grabbable grabbableKomponen;
    private CalibrationManager managerKalibrasi;
    
    private float rotasiSebelumnya;
    private float akumulasiPutaran = 0f;

    // Caches the Grabbable and auto-finds the blink hint if not assigned.
    void Awake()
    {
        grabbableKomponen = GetComponent<Grabbable>();
        grabbableKomponen.enabled = false; 
        
        // Auto-find the hint if it was not dragged into the Inspector.
        if (efekKelapKelip == null) efekKelapKelip = GetComponent<KelapKelipTutorial>();
    }

    // Enables the knob and prepares it for the calibration session.
    public void SetupKnobUntukKalibrasi(CalibrationManager manager)
    {
        managerKalibrasi = manager;
        grabbableKomponen.enabled = true;
        
        rotasiSebelumnya = AmbilRotasiSaatIni();
        akumulasiPutaran = 0f;
    }

    // Turns the blinking hint on or off.
    public void SetStatusHint(bool aktif)
    {
        if (efekKelapKelip != null)
        {
            if (aktif) efekKelapKelip.MulaiKedip();
            else efekKelapKelip.BerhentiKedip();
        }
    }

    // Disables the knob and turns off the hint when calibration is complete.
    public void SelesaiKalibrasi()
    {
        grabbableKomponen.enabled = false;
        SetStatusHint(false); // Make sure the hint is off when calibration finishes.
    }

    // Reads the current rotation angle on the configured axis.
    private float AmbilRotasiSaatIni()
    {
        if (sumbuRotasi == AxisPutaran.X) return transform.localEulerAngles.x;
        if (sumbuRotasi == AxisPutaran.Y) return transform.localEulerAngles.y;
        return transform.localEulerAngles.z;
    }

    // While the knob is held, accumulates rotation and reports density changes.
    void Update()
    {
        if (grabbableKomponen.BeingHeld)
        {
            // Turn off the hint while the knob is held/turned.
            SetStatusHint(false);

            float rotasiSekarang = AmbilRotasiSaatIni();
            
            // Calculate the rotation delta since the last frame.
            float delta = Mathf.DeltaAngle(rotasiSebelumnya, rotasiSekarang);
            rotasiSebelumnya = rotasiSekarang;

            akumulasiPutaran += delta;

            // Once the accumulated rotation reaches a step threshold, change the value.
            if (Mathf.Abs(akumulasiPutaran) >= derajatPerAngka)
            {
                // Count steps (+1 or -1 depending on rotation direction).
                int langkahAngka = Mathf.FloorToInt(akumulasiPutaran / derajatPerAngka);
                akumulasiPutaran -= (langkahAngka * derajatPerAngka);

                if (langkahAngka != 0 && managerKalibrasi != null)
                {
                    managerKalibrasi.UbahKerapatanDariKnob(langkahAngka);

                    // Light mechanical vibration while turning.
                    InputBridge.Instance.VibrateController(0.05f, 0.1f, 0.05f, grabbableKomponen.GetPrimaryGrabber().HandSide);
                    if (suaraTikKecil != null) suaraTikKecil.Play();
                }
            }
        }
        else
        {
            rotasiSebelumnya = AmbilRotasiSaatIni(); 
        }
    }

    // Plays the success click and a strong vibration when the value reaches 100.
    public void BeriFeedbackSukses(bool sedangDipegang)
    {
        if (suaraKlikSukses != null) suaraKlikSukses.Play();
        if (sedangDipegang && grabbableKomponen.BeingHeld)
        {
            // Strong vibration signals the sealing roll locked firmly into place.
            InputBridge.Instance.VibrateController(0.8f, 0.5f, 0.1f, grabbableKomponen.GetPrimaryGrabber().HandSide);
        }
    }
}
