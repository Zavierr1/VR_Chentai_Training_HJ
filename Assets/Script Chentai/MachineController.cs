using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("Status Mesin")]
    public bool isMachineOn = false;

    [Header("Referensi Komponen Utama")]
    [Tooltip("Drag objek yang memiliki script TabletSpawner ke sini")]
    public TabletSpawner tabletSpawner;
    
    [Tooltip("Drag objek yang memiliki script HorizontalCutter ke sini")]
    public HorizontalCutter horizontalCutter;

    [Header("Komponen Lainnya (Opsional)")]
    [Tooltip("Masukkan komponen seperti BrushRotate, FeederVibration, atau FoilSpawner ke sini agar ikut nyala/mati")]
    public MonoBehaviour[] machineParts;

    public void StartMachine()
    {
        if (isMachineOn) return; // Mencegah tombol ditekan berkali-kali saat sudah nyala
        
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
    }

    // Fungsi tambahan: Jika kamu pakai 1 tombol untuk Nyala sekaligus Mati
    public void ToggleMachine()
    {
        if (isMachineOn) StopMachine();
        else StartMachine();
    }
}