using UnityEngine;
using System.Collections.Generic;
using BNG;

// Validates that only the correct part (by tag and prerequisite order) can be
// snapped into an assessment SnapZone. Wrong items are rejected, flashed red,
// and returned to their original position.
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

    // Caches component references and wires up snap/detach events.
    void Start()
    {
        snapZone = GetComponent<SnapZone>();
        gZone = GetComponent<GrabbablesInTrigger>();

        // Event listeners are kept in case success sound effects are added later.
        snapZone.OnSnapEvent.AddListener(HandleItemSnapped);
        snapZone.OnDetachEvent.AddListener(HandleItemDetached);

        BlockSnapping();
    }

    // Called when a part is snapped into the zone. The numeric scoring system
    // has been removed, so this is intentionally empty.
    private void HandleItemSnapped(Grabbable grabbedItem)
    {
        // Snapping succeeded. The numeric scoring system has been removed.
    }

    // Called when a part is detached from the zone. The numeric scoring system
    // has been removed, so this is intentionally empty.
    private void HandleItemDetached(Grabbable grabbedItem)
    {
        // Part detached. The numeric scoring system has been removed.
    }

    // Every frame, checks which grabbable is currently hovered and either allows
    // or blocks snapping based on tag and prerequisite order.
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

    // Waits briefly, then verifies the wrong part was dropped in the zone and
    // triggers the red flash + respawn feedback.
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

    // Flashes the part red for half a second, restores its original color,
    // then returns it to its starting position on the table.
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

    // Returns true if the given grabbable is inside this zone's trigger area.
    private bool IsItemInsideTrigger(Grabbable item)
    {
        if (item == null) return false;
        foreach (var kvp in gZone.NearbyGrabbables)
        {
            if (kvp.Value == item) return true;
        }
        return false;
    }

    // Adds a sentinel name to SnapZone.OnlyAllowNames so no part is allowed to
    // snap while an invalid item is hovered.
    private void BlockSnapping()
    {
        if (snapZone.OnlyAllowNames == null) snapZone.OnlyAllowNames = new List<string>();
        if (!snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
            snapZone.OnlyAllowNames.Add(BLOCK_CODE);
        }
    }

    // Removes the block sentinel so the correct part can snap into the zone.
    private void AllowSnapping()
    {
        if (snapZone.OnlyAllowNames != null && snapZone.OnlyAllowNames.Contains(BLOCK_CODE))
        {
            snapZone.OnlyAllowNames.Clear();
        }
    }

    // Finds the held grabbable closest to this zone's position.
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
