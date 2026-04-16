using UnityEngine;
using BNG; // Wajib untuk sistem Haptic dan Grabbable BNG

[RequireComponent(typeof(Grabbable))]
public class KnobCalibration : MonoBehaviour
{
    [Header("Pengaturan Target Kalibrasi")]
    [Tooltip("Target putaran saat ini (diatur otomatis oleh CalibrationManager)")]
    public float targetRotasiY = 180f;
    
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
    public bool isKalibrasiSukses = false; // Dibaca oleh CalibrationManager nanti

    void Start()
    {
        grabbableKomponen = GetComponent<Grabbable>();
        
        // Opsional: Matikan Grab di awal game, biar player nggak iseng muter
        // sebelum fase kalibrasi dimulai.
        grabbableKomponen.enabled = false; 
    }

    // Fungsi ini dipanggil dari CalibrationManager nanti
    public void MulaiFaseKalibrasi(float targetRandom)
    {
        targetRotasiY = targetRandom;
        isKalibrasiSukses = false;
        sudahBunyiKlik = false;
        
        // Aktifkan agar bisa dipegang
        grabbableKomponen.enabled = true; 
        
        Debug.Log($"[KNOB] Fase kalibrasi dimulai. Cari titik di sekitar: {targetRotasiY} derajat.");
    }

    // Fungsi untuk mematikan interaksi saat kalibrasi selesai
    public void SelesaiKalibrasi()
    {
        grabbableKomponen.enabled = false;
    }

    void Update()
    {
        // 1. Cek apakah kenop sedang digenggam oleh player
        if (grabbableKomponen.BeingHeld)
        {
            CekPutaranDanGetar();
        }
        else
        {
            // Reset status bunyi kalau player lepas tangan, biar bisa bunyi lagi kalau dipegang ulang
            if (!isKalibrasiSukses) sudahBunyiKlik = false; 
        }
    }

    private void CekPutaranDanGetar()
    {
        // 2. Ambil rotasi lokal Y dari kenop saat ini (Pastikan sumbu putarmu benar Y. Jika salah, ganti ke X atau Z)
        // Kita pakai eulerAngles lokal karena kenop ini adalah child dari objek Press
        float rotasiSaatIni = transform.localEulerAngles.y;

        // 3. Hitung selisih jarak rotasi saat ini dengan target
        float selisihJarak = Mathf.DeltaAngle(rotasiSaatIni, targetRotasiY);
        selisihJarak = Mathf.Abs(selisihJarak); // Jadikan positif

        // 4. Logika Sweet Spot (Sukses)
        if (selisihJarak <= toleransiSukses)
        {
            isKalibrasiSukses = true;

            // Mainkan suara KLIK sekali saja saat masuk zona
            if (!sudahBunyiKlik)
            {
                if (suaraKlikSukses != null) suaraKlikSukses.Play();
                
                // >>> PERBAIKAN DI SINI: Menggunakan GetPrimaryGrabber() <<<
                InputBridge.Instance.VibrateController(0.5f, 0.2f, 0.1f, grabbableKomponen.GetPrimaryGrabber().HandSide);
                
                sudahBunyiKlik = true;
            }
        }
        else
        {
            // Kalau keluar dari zona, berarti belum sukses
            isKalibrasiSukses = false;
            sudahBunyiKlik = false;

            // 5. Logika Radar Haptic (Makin dekat, makin getar)
            // Misal: radius radar kita adalah 45 derajat.
            float radiusRadar = 45f;
            
            if (selisihJarak <= radiusRadar)
            {
                // Hitung kekuatan getaran: kalau selisih 45 = getar 0. Kalau selisih 10 = getar kuat.
                float persentaseGetaran = 1f - (selisihJarak / radiusRadar);
                float kekuatanGetar = persentaseGetaran * getaranMaksimal;

                // >>> PERBAIKAN DI SINI JUGA: Menggunakan GetPrimaryGrabber() <<<
                InputBridge.Instance.VibrateController(0.1f, kekuatanGetar, 0.05f, grabbableKomponen.GetPrimaryGrabber().HandSide);
            }
        }
    }

    public float GetPersentaseAkurasi()
    {
        float rotasiSaatIni = transform.localEulerAngles.y;
        float selisihJarak = Mathf.DeltaAngle(rotasiSaatIni, targetRotasiY);
        selisihJarak = Mathf.Abs(selisihJarak);

        // Kita asumsikan jarak terjauh yang dideteksi UI adalah 45 derajat
        float rangeMaksimal = 45f;
        
        // Hitung persentase: 1.0 = tepat di target, 0.0 = di luar range 45 derajat
        float skor = 1f - Mathf.Clamp01(selisihJarak / rangeMaksimal);
        return skor;
    }
}