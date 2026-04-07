using UnityEngine;
using System;
using System.Collections;

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
    
    // >>> TAMBAHAN: Referensi ke NPC kamu
    [Header("Referensi NPC")]
    [Tooltip("Drag karakter NPC yang memiliki script NPCFactoryShow ke sini")]
    public NPCFactoryShow npcPekerja;

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

    [Header("Machine Sound")]
    [Tooltip("AudioSource dengan loop clip suara mesin berjalan")]
    public AudioSource machineAudio;

    [Tooltip("Drag objek yang memiliki script HorizontalCutter ke sini")]
    public HorizontalCutter horizontalCutter;

    [Header("Komponen Lainnya (Opsional)")]
    [Tooltip("Masukkan komponen seperti BrushRotate, FeederVibration, atau FoilSpawner ke sini agar ikut nyala/mati")]
    public MonoBehaviour[] machineParts;

    // Variabel untuk memastikan NPC tidak dipanggil berkali-kali
    private bool npcSudahDipanggil = false;

    void Start()
    {
        if (forceOffComponentsOnStart)
        {
            SetMachineOutputs(false);
            isMachineOn = false;
            OnMachineStateChanged?.Invoke(false);
        }

        // >>> LOGIKA DEBUG: Langsung nyalakan mesin & panggil NPC saat game Play
        if (autoStartForDebug)
        {
            if (npcPekerja != null && !npcSudahDipanggil)
            {
                Debug.Log("Debug Mode: Memulai pertunjukan NPC!");
                npcPekerja.MesinSelesaiDiperbaiki();
                npcSudahDipanggil = true; 

                // Mesin nyala nunggu NPC sampai
                if (otomatisNyalaOlehNPC)
                {
                    StartCoroutine(NyalakanMesinOtomatis());
                }
                else
                {
                    StartMachine(); // Nyala instan kalau centang delay dimatikan
                }
            }
            else 
            {
                // Kalau NPC gak diisi di Inspector, mesin langsung nyala
                StartMachine();
            }
        }
    }

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

        if (machineAudio != null)
        {
            if (active) machineAudio.Play();
            else        machineAudio.Stop();
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

        if (isMachineOn) StopMachine();
        else StartMachine();
    }

    public void SetPartTerpasang()
    {
        partTerpasangSaatIni++;
        Debug.Log($"Alat terpasang di Snap Zone! Mesin siap dinyalakan. Jumlah part terpasang: {partTerpasangSaatIni}");

        // >>> LOGIKA NORMAL: Jika bermain tanpa debug, NPC jalan saat semua part beres dipasang
        if (wajibAdaPart && partTerpasangSaatIni >= targetJumlahPart && !autoStartForDebug)
        {
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

    private IEnumerator NyalakanMesinOtomatis()
    {
        // Tunggu sekian detik sesuai durasi animasi NPC jalan ke mesin
        yield return new WaitForSeconds(delayOtomatisNyala);

        // Tambahkan pengaman 'autoStartForDebug' di pengecekan ini
        // Jadi kalau mode debug, dia akan tetap maksa nyala walau part belum dipasang
        if (partTerpasangSaatIni >= targetJumlahPart || !wajibAdaPart || autoStartForDebug)
        {
            StartMachine();
        }
        else
        {
            Debug.LogWarning("Mesin batal otomatis nyala karena part keburu dicabut player!");
        }
    }

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