using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events;

// Serializable event that passes a string (used to update the UI instruction text).
[System.Serializable]
public class StringEvent : UnityEvent<string> {}

// Data for one snap slot in the group: which SnapZone, which table item, and the UI hint.
[System.Serializable]
public class SnapData
{
    public SnapZone snapZone;

    [Tooltip("Tag spesifik untuk part ini (Digunakan jika Mode Ambil Bebas dimatikan)")]
    public string tagSpesifik = "Untagged";

    [Header("Referensi Objek di Meja")]
    public KelapKelipTutorial highlightMeja;
    public Grabbable bendaDiMeja;

    [Header("Instruksi UI")]
    [TextArea(2, 3)]
    public string instruksiPart = "Pasang komponen ini...";
}

// Manages a sequence of snap zones that must be filled in order (or freely, in pool mode).
// Enables snap zones progressively, controls table-item highlighting, and fires events when done.
public class SnapGroupManager : MonoBehaviour
{
    [Header("Konfigurasi Mode")]
    [Tooltip("CENTANG untuk Part 1 (Cover): Pemain bebas ambil cover mana saja.")]
    public bool isIdenticalPartPool = false;

    [Header("Pengaturan Tag (Hanya Mode Ambil Bebas)")]
    [Tooltip("Ketik satu Tag yang sama untuk semua barang identik di grup ini (Misal: 'Cover')")]
    public string tagGrupIdentik = "Cover";

    [Header("Status Grup")]
    public bool grupAktifDiAwal = false;

    [Header("Urutan Pemasangan (Snap Zone)")]
    public List<SnapData> urutanSnap;

    [Header("Grup Selanjutnya")]
    public List<SnapGroupManager> managerGrupBerikutnya;

    [Header("Event UI & Kamera")]
    public UnityEvent onGrupSelesai;
    public StringEvent onUpdateInstruksiUI;

    private bool isSudahSelesai = false;

    // Runs the group check at startup.
    void Start()
    {
        CekStatusGrup();
    }

    // Re-evaluates the whole group after a short delay.
    public void CekStatusGrup()
    {
        StopAllCoroutines();
        if (gameObject.activeInHierarchy) StartCoroutine(PengecekanTertunda());
    }

    // Enables/disables snap zones based on fill order, then checks completion.
    private IEnumerator PengecekanTertunda()
    {
        yield return new WaitForSeconds(0.05f);

        // Enable or disable every table item according to the group's active state.
        foreach (var data in urutanSnap)
        {
            if (data.bendaDiMeja != null)
            {
                data.bendaDiMeja.enabled = grupAktifDiAwal;
            }
        }

        bool semuaTerpasangDanBenar = true;

        // Walk the snap order, turning each zone on only when the previous slot is correctly filled.
        for (int i = 0; i < urutanSnap.Count; i++)
        {
            SnapData currentData = urutanSnap[i];
            if (currentData.snapZone == null) continue;

            bool currentAdaBarang = currentData.snapZone.HeldItem != null;

            // Group not active — empty and disable every zone.
            if (!grupAktifDiAwal)
            {
                if (currentAdaBarang) currentData.snapZone.ReleaseAll();
                currentData.snapZone.gameObject.SetActive(false);
                semuaTerpasangDanBenar = false;
                continue;
            }

            // First slot is always active.
            if (i == 0)
            {
                currentData.snapZone.gameObject.SetActive(true);
            }
            else
            {
                // Later slots only activate once the previous slot holds a correctly-tagged part.
                SnapData previousData = urutanSnap[i - 1];
                bool prevAdaBarang = previousData.snapZone.HeldItem != null;

                string tagLalu = isIdenticalPartPool ? tagGrupIdentik : previousData.tagSpesifik;
                bool prevTagBenar = prevAdaBarang && previousData.snapZone.HeldItem.gameObject.CompareTag(tagLalu);

                if (prevTagBenar || currentAdaBarang) currentData.snapZone.gameObject.SetActive(true);
                else currentData.snapZone.gameObject.SetActive(false);
            }

            // Verify the current slot holds a correctly-tagged part.
            string tagTarget = isIdenticalPartPool ? tagGrupIdentik : currentData.tagSpesifik;
            if (!currentAdaBarang || !currentData.snapZone.HeldItem.gameObject.CompareTag(tagTarget))
            {
                semuaTerpasangDanBenar = false;
            }
        }

        UpdateHighlightBerurutan();

        // Everything correctly snapped — activate next groups and fire the completion event once.
        if (semuaTerpasangDanBenar)
        {
            foreach (SnapGroupManager nextManager in managerGrupBerikutnya)
                if (nextManager != null) nextManager.AktifkanGrup();

            if (!isSudahSelesai)
            {
                onGrupSelesai?.Invoke();
                isSudahSelesai = true;
            }
        }
        else isSudahSelesai = false;
    }

    // Highlights exactly the table items the player should pick next, and updates the UI hint.
    public void UpdateHighlightBerurutan()
    {
        if (!grupAktifDiAwal) return;

        bool foundFirstEmpty = false;

        foreach (var data in urutanSnap)
        {
            if (data.bendaDiMeja == null) continue;

            // CARA PALING AMAN: Cek apakah fisik barang ini masuk ke SnapZone manapun di grup ini
            bool isObjectSnapped = false;
            foreach (var cekSnap in urutanSnap)
            {
                if (cekSnap.snapZone != null && cekSnap.snapZone.HeldItem != null)
                {
                    if (cekSnap.snapZone.HeldItem.gameObject == data.bendaDiMeja.gameObject)
                    {
                        isObjectSnapped = true;
                        break;
                    }
                }
            }

            if (isObjectSnapped)
            {
                // Kalau fisik barangnya udah nempel di mesin, matikan kelap-kelipnya di meja
                if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
            }
            else
            {
                if (isIdenticalPartPool)
                {
                    // MODE BEBAS (Part 1): Semua barang di meja yang BELUM nempel, tetap kedip
                    if (data.highlightMeja != null) data.highlightMeja.MulaiKedip();
                    data.bendaDiMeja.enabled = true;

                    // Instruksi UI cuma ambil dari slot kosong pertama di mesin
                    if (!foundFirstEmpty && (data.snapZone == null || data.snapZone.HeldItem == null))
                    {
                        onUpdateInstruksiUI?.Invoke(data.instruksiPart);
                        foundFirstEmpty = true;
                    }
                }
                else
                {
                    // MODE BERURUTAN (Part 2 & 3): Cuma barang giliran selanjutnya yang kedip
                    if (!foundFirstEmpty)
                    {
                        if (data.highlightMeja != null) data.highlightMeja.MulaiKedip();
                        data.bendaDiMeja.enabled = true;
                        onUpdateInstruksiUI?.Invoke(data.instruksiPart);
                        foundFirstEmpty = true;
                    }
                    else
                    {
                        if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
                        data.bendaDiMeja.enabled = false;
                    }
                }
            }
        }
    }

    // Stops all table-item blinking in this group.
    public void MatikanHighlight()
    {
        foreach (var data in urutanSnap)
            if (data.highlightMeja != null) data.highlightMeja.BerhentiKedip();
    }

    // Activates this group and re-runs the check.
    public void AktifkanGrup() { grupAktifDiAwal = true; CekStatusGrup(); }

    // Deactivates this group, but only if nothing is snapped yet.
    public void NonaktifkanGrup()
    {
        bool adaYangNempel = false;
        foreach (SnapData data in urutanSnap)
            if (data.snapZone != null && data.snapZone.HeldItem != null) { adaYangNempel = true; break; }

        if (adaYangNempel) return;
        grupAktifDiAwal = false;
        CekStatusGrup();
    }
}
