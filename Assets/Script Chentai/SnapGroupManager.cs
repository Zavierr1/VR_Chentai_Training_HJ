using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events; 

// Tambahan agar Event UI yang bawa teks (string) bisa muncul di Inspector
[System.Serializable]
public class StringEvent : UnityEvent<string> {}

[System.Serializable]
public class SnapData
{
    public SnapZone snapZone;
    public string tagYangBenar = "Untagged";

    public KelapKelipTutorial highlightMeja;
    
    [Tooltip("Tarik komponen Grabbable dari objek meja ke sini")]
    public Grabbable bendaDiMeja; 

    // >>> TAMBAHAN: Teks instruksi spesifik untuk part ini
    [TextArea(2, 3)]
    [Tooltip("Teks yang akan muncul di panel UI saat giliran part ini dipasang")]
    public string instruksiPart = "Pasang komponen ini...";
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
    
    // >>> TAMBAHAN: Event untuk ngirim teks ke layar UI
    [Tooltip("Panggil fungsi UpdateTeksUI dari MonitorCameraController di sini")]
    public StringEvent onUpdateInstruksiUI;
    
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

        foreach(var data in urutanSnap)
        {
            if (data.bendaDiMeja != null)
            {
                data.bendaDiMeja.enabled = grupAktifDiAwal; 
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
            if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
            if (data.bendaDiMeja != null) data.bendaDiMeja.enabled = false;

            if (!sudahAdaYangAktif && (data.snapZone == null || data.snapZone.HeldItem == null))
            {
                if (data.highlightMeja != null) data.highlightMeja.MulaiKedip();
                if (data.bendaDiMeja != null) data.bendaDiMeja.enabled = true;
                
                // >>> TAMBAHAN BARU: Kirim teks intruksi ke layar UI!
                onUpdateInstruksiUI?.Invoke(data.instruksiPart);

                sudahAdaYangAktif = true; 
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