using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events; 

[System.Serializable]
public class SnapData
{
    public SnapZone snapZone;
    public string tagYangBenar = "Untagged";

    public KelapKelipTutorial highlightMeja;
    
    // >>> TAMBAHAN BARU: Referensi Grabbable untuk dikunci
    [Tooltip("Tarik komponen Grabbable dari objek meja ke sini")]
    public Grabbable bendaDiMeja; 
}

public class SnapGroupManager : MonoBehaviour
{
    [Header("Status Grup")]
    public bool grupAktifDiAwal = false;

    [Header("Urutan Pemasangan & Syarat Tag")]
    public List<SnapData> urutanSnap; 

    [Header("Grup Selanjutnya (Target Unlock)")]
    public List<SnapGroupManager> managerGrupBerikutnya;

    [Header("Event UI & Kamera")]
    public UnityEvent onGrupSelesai;
    
    private bool isSudahSelesai = false; 

    void Start()
    {
        CekStatusGrup();
    }

    public void CekStatusGrup()
    {
        StopAllCoroutines();
        StartCoroutine(PengecekanTertunda());
    }

    private IEnumerator PengecekanTertunda()
    {
        yield return new WaitForSeconds(0.05f);

        // >>> LOGIKA BARU: Kunci/Buka fungsi genggam (Grab) pada objek
        foreach(var data in urutanSnap)
        {
            if (data.bendaDiMeja != null)
            {
                data.bendaDiMeja.enabled = grupAktifDiAwal; // Kalau mati, objek gak bisa di-grab!
            }
        }

        bool semuaTerpasangDanBenar = true;

        for (int i = 0; i < urutanSnap.Count; i++)
        {
            SnapData currentData = urutanSnap[i];
            if (currentData.snapZone == null) continue;

            bool currentAdaBarang = currentData.snapZone.HeldItem != null;

            if (!grupAktifDiAwal)
            {
                if (currentAdaBarang) currentData.snapZone.ReleaseAll();
                currentData.snapZone.gameObject.SetActive(false);
                semuaTerpasangDanBenar = false;
                continue;
            }

            if (i == 0)
            {
                currentData.snapZone.gameObject.SetActive(true);
            }
            else
            {
                SnapData previousData = urutanSnap[i - 1];
                bool prevAdaBarang = previousData.snapZone.HeldItem != null;
                bool prevTagBenar = prevAdaBarang && previousData.snapZone.HeldItem.gameObject.CompareTag(previousData.tagYangBenar);
                
                if (prevTagBenar || currentAdaBarang)
                {
                    currentData.snapZone.gameObject.SetActive(true);
                }
                else
                {
                    if (currentAdaBarang) currentData.snapZone.ReleaseAll(); 
                    currentData.snapZone.gameObject.SetActive(false);
                }
            }

            if (!currentAdaBarang || !currentData.snapZone.HeldItem.gameObject.CompareTag(currentData.tagYangBenar))
            {
                semuaTerpasangDanBenar = false;
            }
        }

        UpdateHighlightBerurutan();

        foreach (SnapGroupManager nextManager in managerGrupBerikutnya)
        {
            if (nextManager != null)
            {
                if (semuaTerpasangDanBenar) nextManager.AktifkanGrup();
                else nextManager.NonaktifkanGrup();
            }
        }

        if (semuaTerpasangDanBenar && !isSudahSelesai)
        {
            onGrupSelesai?.Invoke(); 
            isSudahSelesai = true;   
        }
        else if (!semuaTerpasangDanBenar)
        {
            isSudahSelesai = false; 
        }
    }

    public void UpdateHighlightBerurutan()
    {
        if (!grupAktifDiAwal) return;

        bool sudahAdaYangAktif = false;

        foreach (var data in urutanSnap)
        {
            // 1. RESET: Matikan kelap-kelip dan KUNCI tangannya untuk SEMUA objek
            if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
            if (data.bendaDiMeja != null) data.bendaDiMeja.enabled = false;

            // 2. Cari Snap Zone PERTAMA yang MASIH KOSONG
            if (!sudahAdaYangAktif && data.snapZone.HeldItem == null)
            {
                // 3. BUKA KUNCI dan NYALAKAN LAMPU hanya untuk objek urutan ini saja!
                if (data.highlightMeja != null) data.highlightMeja.MulaiKedip();
                if (data.bendaDiMeja != null) data.bendaDiMeja.enabled = true;
                
                sudahAdaYangAktif = true; // Segel! Biar urutan selanjutnya gak ikutan nyala/kebuka
            }
        }
    }

    public void MatikanHighlight()
    {
        foreach (var data in urutanSnap)
        {
            if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
        }
    }
    
    public void AktifkanGrup()
    {
        grupAktifDiAwal = true;
        CekStatusGrup(); 
    }

    public void NonaktifkanGrup()
    {
        bool adaYangNempel = false;
        foreach (SnapData data in urutanSnap)
        {
            if (data.snapZone != null && data.snapZone.HeldItem != null)
            {
                adaYangNempel = true;
                break;
            }
        }
        if (adaYangNempel) return;
        grupAktifDiAwal = false;
        CekStatusGrup(); 
    }
}