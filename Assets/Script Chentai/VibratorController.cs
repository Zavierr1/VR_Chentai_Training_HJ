using UnityEngine;

// Shakes a "Vibrator A" object when no tablets are in the sensor zone, and toggles
// the FeederVibration particle script to match. Tracks tablet count via the zone trigger.
public class VibratorController : MonoBehaviour
{
    [Header("Target Vibrator")]
    [Tooltip("Drag objek 'Vibrator A' dari Hierarchy ke kotak ini")]
    public Transform vibratorA;

    [Header("Vibration Settings")]
    public float vibrationIntensity = 0.005f; // Seberapa kencang getarannya (jarak)
    public float vibrationSpeed = 50f;      // Seberapa cepat getarannya

    [Header("Status (Jangan diubah manual)")]
    public int tabletsInZone = 0; // Menghitung jumlah obat di area

    public FeederVibration scriptGetaranPartikel; // Mengambil script FeederVibration untuk mengontrol Particle System

    private Vector3 _originalPosition;

    public bool _isVibrating = false;

    // Saves the vibrator's original local position so it snaps back when idle.
    void Start()
    {
        if (vibratorA != null)
        {
            // Simpan posisi awal Vibrator A agar tidak bergeser jauh saat bergetar
            _originalPosition = vibratorA.localPosition;
        }
    }

    // Vibrates while the zone is empty; stops and resets once a tablet arrives.
    void Update()
    {
        if (vibratorA == null) return;

        // Logika Utama: Jika jumlah obat 0 atau kurang, maka BERGETAR!
        if (tabletsInZone <= 0)
        {
            _isVibrating = true;

            // Rumus getaran mekanik menggunakan Matematika (Sine & Cosine)
            float shakeX = Mathf.Sin(Time.time * vibrationSpeed) * vibrationIntensity;
            float shakeZ = Mathf.Cos(Time.time * vibrationSpeed * 0.8f) * vibrationIntensity;

            vibratorA.localPosition = _originalPosition + new Vector3(shakeX, 0, shakeZ);

            // 2. TRUE: Nyalakan getaran pada tumpukan pil
            if (scriptGetaranPartikel != null && !scriptGetaranPartikel.enabled)
            {
                scriptGetaranPartikel.enabled = true;
            }
        }
        else
        {
            // Jika ada obat (tabletsInZone > 0), BERHENTI BERGETAR
            if (_isVibrating)
            {
                vibratorA.localPosition = _originalPosition; // Kembalikan ke posisi semula
                _isVibrating = false;
            }

            // FALSE: Matikan script getaran pada tumpukan pil agar diam
            if (scriptGetaranPartikel != null && scriptGetaranPartikel.enabled)
            {
                scriptGetaranPartikel.enabled = false;
            }
        }
    }

    // Fungsi ini dipanggil otomatis saat benda menyentuh kotak Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah benda yang masuk punya tag "Tablet"
        if (other.CompareTag("Tablet"))
        {
            tabletsInZone++; // Tambah jumlah
        }
    }

    // Fungsi ini dipanggil otomatis saat benda keluar dari kotak Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tablet"))
        {
            tabletsInZone--; // Kurangi jumlah

            // Jaga-jaga agar angka tidak minus kalau ada bug fisika
            if (tabletsInZone < 0) tabletsInZone = 0;
        }
    }
}
