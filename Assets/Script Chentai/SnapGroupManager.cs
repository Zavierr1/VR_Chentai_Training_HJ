using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;

[System.Serializable]
public class SnapData
{
    [Tooltip("Masukkan Snap Zone-nya ke sini")]
    public SnapZone snapZone;
    
    [Tooltip("Ketik Tag dari barang yang BENAR (Misal: 'PartFoil', 'PartRoller')")]
    public string tagYangBenar = "Untagged";
}

public class SnapGroupManager : MonoBehaviour
{
    [Header("Status Grup")]
    [Tooltip("Centang HANYA untuk Grup 1 (Bagian paling awal). Grup 2 dan 3 jangan dicentang!")]
    public bool grupAktifDiAwal = false;

    [Header("Urutan Pemasangan & Syarat Tag")]
    [Tooltip("Masukkan urutan Snap Zone beserta TAG yang diwajibkan.")]
    public List<SnapData> urutanSnap; 

    [Header("Grup Selanjutnya (Target Unlock)")]
    [Tooltip("Masukkan script MANAGER dari grup selanjutnya")]
    public List<SnapGroupManager> managerGrupBerikutnya;

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

            // Jika grup ini sedang dikunci oleh Manager sebelumnya
            if (!grupAktifDiAwal)
            {
                if (currentAdaBarang) currentData.snapZone.ReleaseAll();
                currentData.snapZone.gameObject.SetActive(false);
                semuaTerpasangDanBenar = false;
                continue;
            }

            // Jika ini part pertama (index 0), selalu nyalakan
            if (i == 0)
            {
                currentData.snapZone.gameObject.SetActive(true);
            }
            else
            {
                // Cek status part SEBELUMNYA
                SnapData previousData = urutanSnap[i - 1];
                bool prevAdaBarang = previousData.snapZone.HeldItem != null;
                bool prevTagBenar = prevAdaBarang && previousData.snapZone.HeldItem.gameObject.CompareTag(previousData.tagYangBenar);
                
                // >>> LOGIKA BARU ANTI JATUH:
                // Buka snap zone INI JIKA part sebelumnya terpasang dengan benar, 
                // ATAU part ini sendiri SUDAH memegang barang (jangan dijatuhkan!)
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

            // Cek apakah part INI sudah terpasang dengan BENAR untuk unlock Grup Manager berikutnya
            if (!currentAdaBarang || !currentData.snapZone.HeldItem.gameObject.CompareTag(currentData.tagYangBenar))
            {
                semuaTerpasangDanBenar = false;
            }
        }

        // LOGIKA MEMBUKA/MENUTUP GRUP SELANJUTNYA
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
    }

    public void AktifkanGrup()
    {
        grupAktifDiAwal = true;
        CekStatusGrup(); 
    }

    public void NonaktifkanGrup()
    {
        // >>> LOGIKA BARU ANTI JATUH UNTUK BEDA GRUP:
        // Cek apakah di grup ini sudah ada part yang dipasang oleh player
        bool adaYangNempel = false;
        foreach (SnapData data in urutanSnap)
        {
            if (data.snapZone != null && data.snapZone.HeldItem != null)
            {
                adaYangNempel = true;
                break;
            }
        }

        // Jika sudah ada barang yang nempel di grup ini, JANGAN matikan grupnya.
        // Biarkan progres player di grup ini aman dari efek domino.
        if (adaYangNempel) 
        {
            return;
        }

        grupAktifDiAwal = false;
        CekStatusGrup(); 
    }
}