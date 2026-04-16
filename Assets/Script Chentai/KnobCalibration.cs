using UnityEngine;
using BNG;

[RequireComponent(typeof(Grabbable))]
public class KnobCalibration : MonoBehaviour
{
    public enum AxisPutaran { X, Y, Z }

    [Header("Pengaturan Target Kalibrasi")]
    [Tooltip("Pilih sumbu putar knob-mu (Jika muternya di Z, ubah ke Z)")]
    public AxisPutaran sumbuRotasi = AxisPutaran.Z; // <--- SEKARANG BISA DIPILIH DI INSPECTOR

    [Tooltip("Target putaran saat ini (diatur otomatis oleh CalibrationManager)")]
    public float targetRotasi = 180f;
    
    [Tooltip("Toleransi derajat. Jika masuk range ini, dianggap SUKSES")]
    public float toleransiSukses = 5f; 

    [Header("Feedback Haptic & Audio")]
    [Tooltip("Intensitas getaran maksimal saat mendekati target (0 - 1)")]
    public float getaranMaksimal = 0.8f;
    [Tooltip("Audio yang diputar saat masuk ke titik yang pas (Sweet Spot)")]
    public AudioSource suaraKlikSukses;

    // --- Variabel Internal ---
    private Grabbable grabbableKomponen;
    private bool sudahBunyiKlik = false;
    
    [HideInInspector]
    public bool isKalibrasiSukses = false;

    void Start()
    {
        grabbableKomponen = GetComponent<Grabbable>();
        grabbableKomponen.enabled = false; 
    }

    public void MulaiFaseKalibrasi(float targetRandom)
    {
        targetRotasi = targetRandom;
        isKalibrasiSukses = false;
        sudahBunyiKlik = false;
        
        grabbableKomponen.enabled = true; 
        Debug.Log($"[KNOB] Fase kalibrasi dimulai. Cari titik di sekitar: {targetRotasi} derajat.");
    }

    public void SelesaiKalibrasi()
    {
        grabbableKomponen.enabled = false;
    }

    // Fungsi bantuan untuk mengambil nilai rotasi sesuai sumbu yang dipilih
    private float AmbilRotasiSaatIni()
    {
        if (sumbuRotasi == AxisPutaran.X) return transform.localEulerAngles.x;
        if (sumbuRotasi == AxisPutaran.Y) return transform.localEulerAngles.y;
        return transform.localEulerAngles.z; // Default ke Z jika dipilih Z
    }

    void Update()
    {
        if (grabbableKomponen.BeingHeld)
        {
            CekPutaranDanGetar();
        }
        else
        {
            if (!isKalibrasiSukses) sudahBunyiKlik = false; 
        }
    }

    private void CekPutaranDanGetar()
    {
        // 1. Ambil rotasi sesuai sumbu yang di-setting
        float rotasiSaatIni = AmbilRotasiSaatIni();

        // 2. Hitung selisih
        float selisihJarak = Mathf.DeltaAngle(rotasiSaatIni, targetRotasi);
        selisihJarak = Mathf.Abs(selisihJarak); 

        // 3. Logika Sukses
        if (selisihJarak <= toleransiSukses)
        {
            isKalibrasiSukses = true;
            if (!sudahBunyiKlik)
            {
                if (suaraKlikSukses != null) suaraKlikSukses.Play();
                InputBridge.Instance.VibrateController(0.5f, 0.2f, 0.1f, grabbableKomponen.GetPrimaryGrabber().HandSide);
                sudahBunyiKlik = true;
            }
        }
        else
        {
            isKalibrasiSukses = false;
            sudahBunyiKlik = false;

            float radiusRadar = 45f;
            if (selisihJarak <= radiusRadar)
            {
                float persentaseGetaran = 1f - (selisihJarak / radiusRadar);
                float kekuatanGetar = persentaseGetaran * getaranMaksimal;
                InputBridge.Instance.VibrateController(0.1f, kekuatanGetar, 0.05f, grabbableKomponen.GetPrimaryGrabber().HandSide);
            }
        }
    }

    public float GetPersentaseAkurasi()
    {
        // Gunakan fungsi bantuan yang sama untuk hitung UI Slider
        float rotasiSaatIni = AmbilRotasiSaatIni();
        float selisihJarak = Mathf.DeltaAngle(rotasiSaatIni, targetRotasi);
        selisihJarak = Mathf.Abs(selisihJarak);

        float rangeMaksimal = 45f;
        float skor = 1f - Mathf.Clamp01(selisihJarak / rangeMaksimal);
        return skor;
    }
}