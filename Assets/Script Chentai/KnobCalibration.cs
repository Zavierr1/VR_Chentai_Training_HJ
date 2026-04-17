using UnityEngine;
using BNG;

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

    private Grabbable grabbableKomponen;
    private CalibrationManager managerKalibrasi;
    
    private float rotasiSebelumnya;
    private float akumulasiPutaran = 0f;

    void Awake()
    {
        grabbableKomponen = GetComponent<Grabbable>();
        grabbableKomponen.enabled = false; 
    }

    public void SetupKnobUntukKalibrasi(CalibrationManager manager)
    {
        managerKalibrasi = manager;
        grabbableKomponen.enabled = true;
        
        rotasiSebelumnya = AmbilRotasiSaatIni();
        akumulasiPutaran = 0f;
    }

    public void SelesaiKalibrasi()
    {
        grabbableKomponen.enabled = false;
    }

    private float AmbilRotasiSaatIni()
    {
        if (sumbuRotasi == AxisPutaran.X) return transform.localEulerAngles.x;
        if (sumbuRotasi == AxisPutaran.Y) return transform.localEulerAngles.y;
        return transform.localEulerAngles.z;
    }

    void Update()
    {
        if (grabbableKomponen.BeingHeld)
        {
            float rotasiSekarang = AmbilRotasiSaatIni();
            
            // Hitung selisih putaran
            float delta = Mathf.DeltaAngle(rotasiSebelumnya, rotasiSekarang);
            rotasiSebelumnya = rotasiSekarang;

            akumulasiPutaran += delta;

            // Jika putaran mencapai batas derajat, ubah angka kerapatan di UI!
            if (Mathf.Abs(akumulasiPutaran) >= derajatPerAngka)
            {
                // Hitung berapa step (bisa +1 atau -1 tergantung arah putaran)
                int langkahAngka = Mathf.FloorToInt(akumulasiPutaran / derajatPerAngka);
                akumulasiPutaran -= (langkahAngka * derajatPerAngka);

                if (langkahAngka != 0 && managerKalibrasi != null)
                {
                    managerKalibrasi.UbahKerapatanDariKnob(langkahAngka);

                    // Getaran mekanik ringan saat memutar
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

    public void BeriFeedbackSukses(bool sedangDipegang)
    {
        if (suaraKlikSukses != null) suaraKlikSukses.Play();
        if (sedangDipegang && grabbableKomponen.BeingHeld)
        {
            // Getar kencang menandakan Sealing Roll terkunci mantap
            InputBridge.Instance.VibrateController(0.8f, 0.5f, 0.1f, grabbableKomponen.GetPrimaryGrabber().HandSide);
        }
    }
}