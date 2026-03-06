using UnityEngine;
using System;

public class MachineController : MonoBehaviour
{
    public event Action<bool> OnMachineStateChanged;

    [Header("Status Mesin")]
    public bool isMachineOn = false;

    [Header("Debug Mode")]
    [Tooltip("Centang ini jika ingin mesin langsung nyala saat Play tanpa harus pasang part (Untuk Testing)")]
    public bool autoStartForDebug = false; // <--- TAMBAHAN UNTUK DEBUG

    [Header("Startup Setup")]
    [Tooltip("Jika true, semua komponen di list akan dipaksa OFF saat Start")]
    public bool forceOffComponentsOnStart = true;

    [Header("Referensi Komponen Utama")]
    [Tooltip("Drag objek yang memiliki script TabletSpawner ke sini")]
    public TabletSpawner tabletSpawner;

    [Header("Sistem Keamanan (Multi Snap Zone)")]
    [Tooltip("Centang jika mesin WAJIB menunggu part terpasang")]
    public bool wajibAdaPart = true;

    [Tooltip("Berapa banyak part yang HARUS terpasang agar mesin bisa nyala?")]
    public int targetJumlahPart = 2;

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

    void Start()
    {
        if (forceOffComponentsOnStart)
        {
            SetMachineOutputs(false);
            isMachineOn = false;
            OnMachineStateChanged?.Invoke(false);
        }

        // >>> TAMBAHAN: Langsung nyalakan mesin saat game Play jika mode debug aktif
        if (autoStartForDebug)
        {
            StartMachine();
        }
    }

    public void StartMachine()
    {
        if (isMachineOn) return;

        // >>> LOGIKA BARU: Cek apakah jumlah part belum terpenuhi (dan abaikan jika sedang mode Debug)
        if (wajibAdaPart && partTerpasangSaatIni < targetJumlahPart && !autoStartForDebug)
        {
            Debug.LogWarning($"Mesin menolak menyala! Baru {partTerpasangSaatIni} dari {targetJumlahPart} part yang terpasang.");
            return; // Gagalkan proses menyala
        }

        isMachineOn = true;
        Debug.Log("Mesin Dinyalakan!");

        SetMachineOutputs(true);
        OnMachineStateChanged?.Invoke(true);
    }

    public void StopMachine()
    {
        if (!isMachineOn) return;

        isMachineOn = false;
        Debug.Log("Mesin Dimatikan!");

        SetMachineOutputs(false);
        OnMachineStateChanged?.Invoke(false);
    }

    private void SetMachineOutputs(bool active)
    {
        if (tabletSpawner != null) tabletSpawner.isMachineRunning = active;

        if (horizontalCutter != null)
        {
            horizontalCutter.loop = active;
            if (active) horizontalCutter.StartCut();
        }

        foreach (var part in machineParts)
        {
            if (part != null) part.enabled = active;
        }

        foreach (var anim in machineAnimators)
        {
            if (anim != null) anim.enabled = active;
        }
    }

    public void ToggleMachine()
    {
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

        if (partTerpasangSaatIni < 0) partTerpasangSaatIni = 0;

        Debug.Log($"1 Part dilepas! Total terpasang: {partTerpasangSaatIni} / {targetJumlahPart}");

        // Fitur Realistis: Jangan matikan mesin jika mode debug sedang aktif
        if (isMachineOn && partTerpasangSaatIni < targetJumlahPart && !autoStartForDebug)
        {
            Debug.LogWarning("ALARM: Part mesin dicabut saat beroperasi! Mesin dimatikan otomatis.");
            StopMachine();
        }
    }
}
