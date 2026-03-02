using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("Status Mesin")]
    public bool isMachineOn = false;

    [Header("Referensi Komponen Utama")]
    [Tooltip("Drag objek yang memiliki script TabletSpawner ke sini")]
    public TabletSpawner tabletSpawner;

    [Header("Sistem Keamanan (Snap Zone)")]
    [Tooltip("Centang jika mesin WAJIB menunggu part terpasang di Snap Zone")]
    public bool wajibAdaPart = true; 
    [Tooltip("Status apakah part sudah terpasang (Otomatis, jangan dicentang manual)")]
    public bool isPartTerpasang = false;

    private float lastToggleTime = 0f;
    private float toggleCooldown = 0.5f;    
    public Animator[] machineAnimators;
    
    [Tooltip("Drag objek yang memiliki script HorizontalCutter ke sini")]
    public HorizontalCutter horizontalCutter;

    [Header("Komponen Lainnya (Opsional)")]
    [Tooltip("Masukkan komponen seperti BrushRotate, FeederVibration, atau FoilSpawner ke sini agar ikut nyala/mati")]
    public MonoBehaviour[] machineParts;

    public void StartMachine()
    {
        if (isMachineOn) return; // Mencegah tombol ditekan berkali-kali saat sudah nyala

        // >>> LOGIKA SNAP ZONE <<<
        if (wajibAdaPart && !isPartTerpasang)
        {
            Debug.LogWarning("Mesin menolak menyala! Part belum terpasang di Snap Zone.");
            return; // Menghentikan proses (mesin gagal nyala)
        }

        isMachineOn = true;
        Debug.Log("Mesin Dinyalakan!");

        // 1. Nyalakan sistem jatuhnya obat
        if (tabletSpawner != null) tabletSpawner.isMachineRunning = true;
        
        // 2. Mulai siklus pemotong foil
        if (horizontalCutter != null)
        {
            horizontalCutter.loop = true;
            horizontalCutter.StartCut();
        }

        // 3. Nyalakan komponen pendukung lainnya (Sikat, Vibrator, dll)
        foreach (var part in machineParts)
        {
            if (part != null) part.enabled = true;
        }

        foreach (var anim in machineAnimators)
        {
            if (anim != null) anim.enabled = true;
        }
    }

    public void StopMachine()
    {
        if (!isMachineOn) return;
        
        isMachineOn = false;
        Debug.Log("Mesin Dimatikan!");

        if (tabletSpawner != null) tabletSpawner.isMachineRunning = false;
        
        if (horizontalCutter != null) horizontalCutter.loop = false; // Cutter akan berhenti setelah siklus potongnya selesai

        foreach (var part in machineParts)
        {
            if (part != null) part.enabled = false;
        }

        // Hentikan/Pause semua animasi
        foreach (var anim in machineAnimators)
        {
            if (anim != null) anim.enabled = false;
        }
    }

    // Fungsi tambahan: Jika kamu pakai 1 tombol untuk Nyala sekaligus Mati
    public void ToggleMachine()
    {
       // Mencegah fungsi terpanggil berkali-kali dalam waktu kurang dari 0.5 detik
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            Debug.Log("Tombol ditekan terlalu cepat (Cooldown aktif)!");
            return; 
        }

        lastToggleTime = Time.time;

        if (isMachineOn) 
        {
            StopMachine();
        }
        else 
        {
            StartMachine();
        }
    }

    public void SetPartTerpasang()
    {
        isPartTerpasang = true;
        Debug.Log("Alat terpasang di Snap Zone! Mesin siap dinyalakan.");
    }

    public void SetPartDilepas()
    {
        isPartTerpasang = false;
        Debug.Log("Alat dilepas dari Snap Zone!");
        
        // Fitur Realistis: Jika alat dicabut paksa saat mesin sedang jalan, mesin otomatis mati!
        if (isMachineOn)
        {
            StopMachine();
        }
    }
}