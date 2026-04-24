using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events; 

// Tambahan agar Event UI yang bisa mengirim teks (string) muncul di Inspector
[System.Serializable]
public class StringEvent : UnityEvent<string> {}

[System.Serializable]
public class SnapData
{
    public SnapZone snapZone;
    [Tooltip("Gunakan Tag yang sama untuk semua part yang identik (misal: 'Cover')")]
    public string tagYangBenar = "Cover";

    public KelapKelipTutorial highlightMeja;
    
    [Tooltip("Tarik komponen Grabbable dari objek meja ke sini")]
    public Grabbable bendaDiMeja; 

    [TextArea(2, 3)]
    [Tooltip("Teks instruksi yang akan muncul di TV saat giliran slot ini diisi")]
    public string instruksiPart = "Ambil salah satu cover dan pasang ke mesin...";
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
    
    [Tooltip("Hubungkan ke fungsi UpdateTeksUI di MonitorCameraController")]
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

        bool semuaTerpasangDanBenar = true;

        for (int i = 0; i < urutanSnap.Count; i++)
        {
            SnapData currentData = urutanSnap[i];
            if (currentData.snapZone == null) continue;

            bool currentAdaBarang = currentData.snapZone.HeldItem != null;

            // Jika grup belum aktif (misal Part B belum waktunya), sembunyikan semua Snap Zone
            if (!grupAktifDiAwal)
            {
                if (currentAdaBarang) currentData.snapZone.ReleaseAll();
                currentData.snapZone.gameObject.SetActive(false);
                semuaTerpasangDanBenar = false;
                continue;
            }

            // --- LOGIKA HOLOGRAM (SNAP ZONE) BERURUTAN ---
            if (i == 0)
            {
                // Snap Zone pertama selalu aktif jika grup aktif
                currentData.snapZone.gameObject.SetActive(true);
            }
            else
            {
                SnapData previousData = urutanSnap[i - 1];
                bool prevAdaBarang = previousData.snapZone.HeldItem != null;
                bool prevTagBenar = prevAdaBarang && previousData.snapZone.HeldItem.gameObject.CompareTag(previousData.tagYangBenar);
                
                // Snap Zone selanjutnya HANYA muncul jika posisi sebelumnya sudah terisi dengan Tag yang benar
                if (prevTagBenar || currentAdaBarang)
                {
                    currentData.snapZone.gameObject.SetActive(true);
                }
                else
                {
                    currentData.snapZone.gameObject.SetActive(false);
                }
            }

            // Cek apakah slot ini sudah terisi dengan benar
            if (!currentAdaBarang || !currentData.snapZone.HeldItem.gameObject.CompareTag(currentData.tagYangBenar))
            {
                semuaTerpasangDanBenar = false;
            }
        }

        UpdateHighlightBerurutan();

        // Aktifkan grup selanjutnya otomatis jika grup ini sudah lengkap
        if (semuaTerpasangDanBenar)
        {
            foreach (SnapGroupManager nextManager in managerGrupBerikutnya)
            {
                if (nextManager != null) nextManager.AktifkanGrup();
            }

            if (!isSudahSelesai)
            {
                onGrupSelesai?.Invoke(); 
                isSudahSelesai = true;   
            }
        }
        else
        {
            isSudahSelesai = false; 
        }
    }

    public void UpdateHighlightBerurutan()
    {
        if (!grupAktifDiAwal) return;

        bool foundFirstEmpty = false;

        foreach (var data in urutanSnap)
        {
            bool isSnapped = data.snapZone != null && data.snapZone.HeldItem != null;

            if (isSnapped)
            {
                // Jika sudah terpasang, matikan highlight di meja
                if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
            }
            else
            {
                // >>> LOGIKA BEBAS: Semua barang yang belum terpasang akan menyala bersamaan <<<
                if (data.highlightMeja != null) data.highlightMeja.MulaiKedip();
                if (data.bendaDiMeja != null) data.bendaDiMeja.enabled = true;

                // Hanya kirim instruksi teks untuk slot kosong pertama yang menjadi tujuan saat ini
                if (!foundFirstEmpty)
                {
                    onUpdateInstruksiUI?.Invoke(data.instruksiPart);
                    foundFirstEmpty = true;
                }
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
        // Jangan nonaktifkan jika sudah ada barang yang menempel
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