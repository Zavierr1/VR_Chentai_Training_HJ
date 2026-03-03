using UnityEngine;
using System.Collections; // Wajib untuk sistem Coroutine (Jeda Waktu)
using System.Collections.Generic;
using BNG;

public class SnapGroupManager : MonoBehaviour
{
    [Header("Status Grup")]
    [Tooltip("Centang HANYA untuk Grup 1 (Bagian paling awal). Grup 2 dan 3 jangan dicentang!")]
    public bool grupAktifDiAwal = false;

    [Header("Urutan Pemasangan (Syarat)")]
    [Tooltip("Masukkan Snap Zone SESUAI URUTAN. (Urutan 0 harus dipasang agar urutan 1 muncul, dst)")]
    public List<SnapZone> syaratSnapZones;

    [Header("Grup Selanjutnya (Target Unlock)")]
    [Tooltip("Masukkan script MANAGER dari grup selanjutnya (Misal: drag Manager_Bagian2 ke sini)")]
    public List<SnapGroupManager> managerGrupBerikutnya;

    void Start()
    {
        // Jalankan pengecekan di awal permainan
        CekStatusGrup();
    }

    // Dipanggil oleh Event BNG saat ada barang dipasang/dilepas
    public void CekStatusGrup()
    {
        // BUG FIX BNG: Kita tidak boleh langsung mengecek. 
        // Kita harus tunggu sepersekian detik agar BNG selesai mengosongkan status "HeldItem".
        StopAllCoroutines(); // Hentikan antrean cek sebelumnya agar tidak tabrakan
        StartCoroutine(PengecekanTertunda());
    }

    private IEnumerator PengecekanTertunda()
    {
        // Jeda waktu sangat singkat (sepersekian detik) yang tidak terasa oleh pemain,
        // tapi cukup bagi mesin Unity untuk mereset status barang menjadi "Kosong/Null".
        yield return new WaitForSeconds(0.05f);

        bool semuaTerpasang = true;

        // 1. LOGIKA URUTAN INTERNAL (Cek dari atas ke bawah)
        for (int i = 0; i < syaratSnapZones.Count; i++)
        {
            SnapZone currentSnap = syaratSnapZones[i];
            if (currentSnap == null) continue;

            // Jika grup ini sedang dikunci oleh Manager sebelumnya
            if (!grupAktifDiAwal)
            {
                if (currentSnap.HeldItem != null) currentSnap.ReleaseAll();
                currentSnap.gameObject.SetActive(false);
                semuaTerpasang = false;
                continue;
            }

            // Jika ini adalah part pertama (index 0), selalu nyalakan
            if (i == 0)
            {
                currentSnap.gameObject.SetActive(true);
            }
            else
            {
                // Mengecek status part SEBELUMNYA
                SnapZone previousSnap = syaratSnapZones[i - 1];
                
                if (previousSnap != null && previousSnap.HeldItem != null)
                {
                    // Part sebelumnya sudah terpasang, maka part ini boleh muncul!
                    currentSnap.gameObject.SetActive(true);
                }
                else
                {
                    // Part sebelumnya belum terpasang, part ini harus disembunyikan
                    if (currentSnap.HeldItem != null) currentSnap.ReleaseAll(); 
                    currentSnap.gameObject.SetActive(false);
                }
            }

            // Cek apakah part ini masih kosong
            if (currentSnap.HeldItem == null)
            {
                semuaTerpasang = false;
            }
        }

        // 2. LOGIKA MEMBUKA GRUP SELANJUTNYA
        foreach (SnapGroupManager nextManager in managerGrupBerikutnya)
        {
            if (nextManager != null)
            {
                if (semuaTerpasang)
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

    // Perintah yang dipanggil oleh Manager sebelumnya
    public void AktifkanGrup()
    {
        grupAktifDiAwal = true;
        CekStatusGrup(); 
    }

    public void NonaktifkanGrup()
    {
        grupAktifDiAwal = false;
        CekStatusGrup(); 
    }
}