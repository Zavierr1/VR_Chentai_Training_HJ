using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events; // 1. Wajib tambahkan ini untuk membuat kolom Event di Inspector

[System.Serializable]
public class SnapData
{
    public SnapZone snapZone;
    public string tagYangBenar = "Untagged";
}

public class SnapGroupManager : MonoBehaviour
{
    [Header("Status Grup")]
    public bool grupAktifDiAwal = false;

    [Header("Urutan Pemasangan & Syarat Tag")]
    public List<SnapData> urutanSnap; 

    [Header("Grup Selanjutnya (Target Unlock)")]
    public List<SnapGroupManager> managerGrupBerikutnya;

    // 2. TAMBAHAN BARU: Event yang akan dipicu saat semua part di grup ini terpasang!
    [Header("Event UI & Kamera")]
    [Tooltip("Apa yang terjadi kalau grup ini selesai? (Misal: Panggil BukaKunciPartB di CameraManager)")]
    public UnityEvent onGrupSelesai;
    
    // Variabel pengunci agar tidak terpanggil berkali-kali
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

        foreach (SnapGroupManager nextManager in managerGrupBerikutnya)
        {
            if (nextManager != null)
            {
                if (semuaTerpasangDanBenar) 
                {
                    nextManager.AktifkanGrup();
                }
                else 
                {
                    nextManager.NonaktifkanGrup();
                }
            }
        }

        // 3. LOGIKA BARU: Jika semua terpasang dan benar, jalankan Event ke Kamera/UI
        if (semuaTerpasangDanBenar && !isSudahSelesai)
        {
            onGrupSelesai?.Invoke(); // Panggil fungsi di Inspector
            isSudahSelesai = true;   // Kunci agar tidak spam panggil
        }
        else if (!semuaTerpasangDanBenar)
        {
            isSudahSelesai = false; // Buka kunci lagi kalau ada barang yang dicabut
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