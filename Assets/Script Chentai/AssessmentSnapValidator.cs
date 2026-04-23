using UnityEngine;
using System.Collections.Generic;
using BNG;

[RequireComponent(typeof(SnapZone))]
[RequireComponent(typeof(GrabbablesInTrigger))]
public class AssessmentSnapValidator : MonoBehaviour
{
    [Header("Assessment Target")]
    [Tooltip("Ketik Tag dari barang yang BENAR (Sama seperti di SnapGroupManager)")]
    public string requiredTag = "Untagged";

    [Header("Error Feedback")]
    [Tooltip("Suara saat part yang dimasukkan salah")]
    public AudioSource errorSound;
    
    [Tooltip("Warna part saat dilepas dan salah (Bawaan: Merah)")]
    public Color warnaPartSalah = Color.red;

    [Header("Urutan Pemasangan (Opsional)")]
    public SnapZone prerequisiteSnapZone;

    private SnapZone snapZone;
    private GrabbablesInTrigger gZone;
    
    private const string BLOCK_CODE = "BLOCK_WRONG_ITEM_ASSESSMENT";
    private Grabbable previouslyHoveredWrongItem = null;

    void Start()
    {
        snapZone = GetComponent<SnapZone>();
        gZone = GetComponent<GrabbablesInTrigger>();

        // Event listener ini tetap kita pertahankan jika di masa depan 
        // kamu ingin menambahkan efek suara sukses saat dipasang/dilepas
        snapZone.OnSnapEvent.AddListener(HandleItemSnapped);
        snapZone.OnDetachEvent.AddListener(HandleItemDetached);

        BlockSnapping();
    }

    private void HandleItemSnapped(Grabbable grabbedItem)
    {
        // Pemasangan berhasil. Sistem skor angka sudah dihapus.
    }

    private void HandleItemDetached(Grabbable grabbedItem)
    {
        // Barang dilepas. Sistem skor angka sudah dihapus.
    }

    void Update()
    {
        if (snapZone.HeldItem != null)
        {
            previouslyHoveredWrongItem = null;
            return;
        }

        Grabbable closest = GetClosestHoveredGrabbable();

        if (closest != null)
        {
            bool isUrutanBenar = true;
            if (prerequisiteSnapZone != null && prerequisiteSnapZone.HeldItem == null)
            {
                isUrutanBenar = false; 
            }

            if (closest.CompareTag(requiredTag) && isUrutanBenar)
            {
                previouslyHoveredWrongItem = null;
                AllowSnapping();
            }
            else
            {
                previouslyHoveredWrongItem = closest;
                BlockSnapping();
            }
        }
        else
        {
            if (previouslyHoveredWrongItem != null)
            {
                if (!previouslyHoveredWrongItem.BeingHeld && IsItemInsideTrigger(previouslyHoveredWrongItem))
                {
                    StartCoroutine(WaitAndHandleWrongDrop(previouslyHoveredWrongItem));
                }
                previouslyHoveredWrongItem = null;
            }
            BlockSnapping();
        }
    }

    private System.Collections.IEnumerator WaitAndHandleWrongDrop(Grabbable wrongItem)
    {
        yield return new WaitForSeconds(0.15f);
        if (wrongItem == null) yield break;

        SnapZone terpasangDiZonaMana = wrongItem.GetComponentInParent<SnapZone>();

        if (terpasangDiZonaMana == null && !wrongItem.BeingHeld)
        {
            Debug.Log("<color=red>[ASSESSMENT] Part salah dijatuhkan di SnapZone ini!</color>");
            if (errorSound != null) errorSound.Play();

            StartCoroutine(FlashPartMerahLaluKembalikan(wrongItem));
        }
    }

    private System.Collections.IEnumerator FlashPartMerahLaluKembalikan(Grabbable wrongItem)
    {
        Renderer[] renderers = wrongItem.GetComponentsInChildren<Renderer>();
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                originalColors[r] = r.material.color;
                r.material.color = warnaPartSalah;
            }
            else if (r.material.HasProperty("_BaseColor")) 
            {
                originalColors[r] = r.material.GetColor("_BaseColor");
                r.material.SetColor("_BaseColor", warnaPartSalah);
            }
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var r in renderers)
        {
            if (originalColors.ContainsKey(r))
            {
                if (r.material.HasProperty("_Color")) r.material.color = originalColors[r];
                else if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", originalColors[r]);
            }
        }

        BarangRespawn respawn = wrongItem.GetComponent<BarangRespawn>();
        if (respawn != null) respawn.KembalikanKeMeja();
    }

    private bool IsItemInsideTrigger(Grabbable item)
    {
        if (item == null) return false;
        foreach (var kvp in gZone.NearbyGrabbables)
        {
            if (kvp.Value == item) return true;
        }
        return false;
    }

    private void BlockSnapping()
    {
        if (snapZone.OnlyAllowNames == null) snapZone.OnlyAllowNames = new List<string>();
        if (!snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
            snapZone.OnlyAllowNames.Add(BLOCK_CODE);
        }
    }

    private void AllowSnapping()
    {
        if (snapZone.OnlyAllowNames != null && snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
        }
    }

    private Grabbable GetClosestHoveredGrabbable()
    {
        float closestDist = float.MaxValue;
        Grabbable closestGrab = null;

        foreach (var kvp in gZone.NearbyGrabbables)
        {
            Grabbable g = kvp.Value;
            if (g == null || !g.BeingHeld) continue; 

            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestGrab = g;
            }
        }
        return closestGrab;
    }
}