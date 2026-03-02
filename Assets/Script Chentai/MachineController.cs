using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("Status Mesin")]
    public bool isMachineOn = false;

    [Header("Referensi Komponen Utama")]
    [Tooltip("Drag objek yang memiliki script TabletSpawner ke sini")]
    public TabletSpawner tabletSpawner;

    [Header("Sistem Keamanan (Multi Snap Zone)")]
    [Tooltip("Centang jika mesin WAJIB menunggu part terpasang")]
    public bool wajibAdaPart = true; 
    
    [Tooltip("Berapa banyak part yang HARUS terpasang agar mesin bisa nyala?")]
    public int targetJumlahPart = 2; // Ubah angka ini sesuai jumlah part mesinmu

    [Tooltip("Jumlah part yang saat ini sedang menempel (Otomatis, jangan diubah)")]
    public int partTerpasangSaatIni = 0;

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

        // >>> LOGIKA BARU: Cek apakah jumlah part yang nempel sudah memenuhi target
        if (wajibAdaPart && partTerpasangSaatIni < targetJumlahPart)
        {
            Debug.LogWarning($"Mesin menolak menyala! Baru {partTerpasangSaatIni} dari {targetJumlahPart} part yang terpasang.");
            return; // Gagalkan proses menyala
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
        partTerpasangSaatIni++;
        Debug.Log($"Alat terpasang di Snap Zone! Mesin siap dinyalakan. Jumlah part terpasang: {partTerpasangSaatIni}");
    }

    public void SetPartDilepas()
    {
        partTerpasangSaatIni--;
        
        // Jaga-jaga agar angkanya tidak minus kalau ada bug dari sistem VR
        if (partTerpasangSaatIni < 0) partTerpasangSaatIni = 0; 
        
        Debug.Log($"1 Part dilepas! Total terpasang: {partTerpasangSaatIni} / {targetJumlahPart}");
        
        // Fitur Realistis: Jika ada 1 part saja yang dicabut saat mesin jalan, mesin langsung mati total!
        if (isMachineOn && partTerpasangSaatIni < targetJumlahPart)
        {
            Debug.LogWarning("ALARM: Part mesin dicabut saat beroperasi! Mesin dimatikan otomatis.");
            StopMachine();
        }
    }
}