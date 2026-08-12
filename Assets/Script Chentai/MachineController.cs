using UnityEngine;
using System;
using System.Collections;

// Central controller for the machine's on/off state. Gates starting on the number
// of installed parts, drives all machine outputs (spawner, cutter, parts, animators,
// audio), and coordinates with the NPC and assessment timer.
public class MachineController : MonoBehaviour
{
    public event Action<bool> OnMachineStateChanged;

    [Header("Status Mesin")]
    public bool isMachineOn = false;

    [Header("Debug Mode")]
    [Tooltip("Centang ini jika ingin mesin langsung nyala & NPC langsung jalan saat Play tanpa harus pasang part (Untuk Testing)")]
    public bool autoStartForDebug = false;

    [Tooltip("Centang jika mesin otomatis nyala sendiri untuk menyesuaikan animasi NPC")]
    public bool otomatisNyalaOlehNPC = true;
    [Tooltip("Waktu tunggu (detik) dari NPC dipanggil sampai mesin menyala (Sesuaikan dengan durasi NPC jalan ke mesin)")]
    public float delayOtomatisNyala = 4.0f;

    [Header("Startup Setup")]
    [Tooltip("Jika true, semua komponen di list akan dipaksa OFF saat Start")]
    public bool forceOffComponentsOnStart = true;

    [Header("Referensi Komponen Utama")]
    [Tooltip("Drag objek yang memiliki script TabletSpawner ke sini")]
    public TabletSpawner tabletSpawner;
    
    [Header("Referensi NPC & Timer")]
    [Tooltip("Drag karakter NPC yang memiliki script NPCFactoryShow ke sini")]  
    public NPCFactoryShow npcPekerja;

    [Tooltip("Drag script AssessmentTimer dari UI ke sini agar saat 9 part terpasang timer berhenti")]  
    public AssessmentTimer timerAssessment;

    [Header("Sistem Keamanan (Multi Snap Zone)")]
    [Tooltip("Centang jika mesin WAJIB menunggu part terpasang")]
    public bool wajibAdaPart = true;

    [Tooltip("Berapa banyak part yang HARUS terpasang agar mesin bisa nyala?")]
    public int targetJumlahPart = 2;

    [Tooltip("Jumlah part yang saat ini sedang menempel (Otomatis, jangan diubah)")]
    public int partTerpasangSaatIni = 0;

    private float lastToggleTime = 0f;
    private float toggleCooldown = 0.5f;

    [Header("Pengaturan Assessment (Ujian)")]
    [Tooltip("Centang HANYA DI SCENE ASSESSMENT: Timer tidak akan berhenti saat rakitan selesai, tapi menunggu sampai kalibrasi sukses.")]
    public bool tungguKalibrasiUntukTimer = false;

    public Animator[] machineAnimators;

    [Header("Machine Sound")]
    [Tooltip("AudioSource dengan loop clip suara mesin berjalan")]
    public AudioSource machineAudio;

    [Tooltip("Drag objek yang memiliki script HorizontalCutter ke sini")]
    public HorizontalCutter horizontalCutter;

    [Header("Komponen Lainnya (Opsional)")]
    [Tooltip("Masukkan komponen seperti BrushRotate, FeederVibration, atau FoilSpawner ke sini agar ikut nyala/mati")]
    public MonoBehaviour[] machineParts;

    // Prevents the NPC from being summoned more than once.
    private bool npcSudahDipanggil = false;

    // Forces all outputs off at start and, in debug mode, starts everything immediately.
    void Start()
    {
        if (forceOffComponentsOnStart)
        {
            SetMachineOutputs(false);
            isMachineOn = false;
            OnMachineStateChanged?.Invoke(false);
        }

        // DEBUG LOGIC: Immediately start the machine and summon the NPC on Play.
        if (autoStartForDebug)
        {
            if (npcPekerja != null && !npcSudahDipanggil)
            {
                Debug.Log("Debug Mode: Memulai pertunjukan NPC!");
                npcPekerja.MesinSelesaiDiperbaiki();
                npcSudahDipanggil = true; 

                // Machine starts after the NPC arrives.
                if (otomatisNyalaOlehNPC)
                {
                    StartCoroutine(NyalakanMesinOtomatis());
                }
                else
                {
                    StartMachine(); // Start instantly if the delay flag is disabled.
                }
            }
            else 
            {
                // If no NPC is assigned in the Inspector, start the machine directly.
                StartMachine();
            }
        }
    }

    // Turns the machine on if the part requirement is met.
    public void StartMachine()
    {
        if (isMachineOn) return;

        if (wajibAdaPart && partTerpasangSaatIni < targetJumlahPart && !autoStartForDebug)
        {
            Debug.LogWarning($"Mesin menolak menyala! Baru {partTerpasangSaatIni} dari {targetJumlahPart} part yang terpasang.");
            return; 
        }

        isMachineOn = true;
        Debug.Log("Mesin Dinyalakan!");

        SetMachineOutputs(true);
        OnMachineStateChanged?.Invoke(true);
    }

    // Turns the machine off.
    public void StopMachine()
    {
        if (!isMachineOn) return;

        isMachineOn = false;
        Debug.Log("Mesin Dimatikan!");

        SetMachineOutputs(false);
        OnMachineStateChanged?.Invoke(false);
    }

    // Applies the machine state to every connected output component.
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

        if (machineAudio != null)
        {
            if (active) machineAudio.Play();
            else        machineAudio.Stop();
        }
    }

    // Toggles the machine on/off with a cooldown to avoid rapid presses.
    public void ToggleMachine()
    {
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            Debug.Log("Tombol ditekan terlalu cepat (Cooldown aktif)!");
            return;
        }

        lastToggleTime = Time.time;

        if (isMachineOn) StopMachine();
        else StartMachine();
    }

    // Increments the installed part count and triggers the success flow at the target.
    public void SetPartTerpasang()
    {
        partTerpasangSaatIni++;
        Debug.Log($"Alat terpasang di Snap Zone! Mesin siap dinyalakan. Jumlah part terpasang: {partTerpasangSaatIni}");

        if (wajibAdaPart && partTerpasangSaatIni >= targetJumlahPart && !autoStartForDebug)
        {
            // SAFE LOGIC: Stop the timer on full assembly ONLY if the calibration
            // switch is disabled.
            if (timerAssessment != null && !tungguKalibrasiUntukTimer)
            {
                timerAssessment.BerhentiTimerKarenaBerhasil();
            }

            if (npcPekerja != null && !npcSudahDipanggil)
            {
                Debug.Log("Semua part terpasang: Memulai pertunjukan NPC!");
                npcPekerja.MesinSelesaiDiperbaiki();
                npcSudahDipanggil = true;
                if (otomatisNyalaOlehNPC)
                {
                    StartCoroutine(NyalakanMesinOtomatis());
                }
            }
        }
    }

    // Waits for the NPC animation to reach the machine, then starts it if allowed.
    private IEnumerator NyalakanMesinOtomatis()
    {
        // Wait the configured seconds to match the NPC walking animation.
        yield return new WaitForSeconds(delayOtomatisNyala);

        // The 'autoStartForDebug' check stays here so debug mode always force-starts
        // the machine even without installed parts.
        if (partTerpasangSaatIni >= targetJumlahPart || !wajibAdaPart || autoStartForDebug)
        {
            StartMachine();
        }
        else
        {
            Debug.LogWarning("Mesin batal otomatis nyala karena part keburu dicabut player!");
        }
    }

    // Decrements the installed part count and stops the machine if a part is removed mid-operation.
    public void SetPartDilepas()
    {
        partTerpasangSaatIni--;
        if (partTerpasangSaatIni < 0) partTerpasangSaatIni = 0;

        Debug.Log($"1 Part dilepas! Total terpasang: {partTerpasangSaatIni} / {targetJumlahPart}");

        if (isMachineOn && partTerpasangSaatIni < targetJumlahPart && !autoStartForDebug)
        {
            Debug.LogWarning("ALARM: Part mesin dicabut saat beroperasi! Mesin dimatikan otomatis.");
            StopMachine();
        }
    }
}   
